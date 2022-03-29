using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CustomBeatmaps.UI.Highscore;
using CustomBeatmaps.Util;
using JetBrains.Annotations;

namespace CustomBeatmaps.CustomPackages
{
    public class ServerHighScoreManager
    {
        public string Failure { get; private set; }
        public bool Loaded { get; private set; }
        private bool _loading = false;

        private ScoreManager HighScoreManager = new ScoreManager();
        private ScoreManager LowScoreManager = new ScoreManager();

        public bool CanSendScoreForCurrentMap => CurrentBeatmapKey != null;

        public string CurrentBeatmapKey { get; private set; }

        public DesyncedScore[] DesyncedHighScores { get; private set; } = Array.Empty<DesyncedScore>();

        public void Reload()
        {
            // Already handling request
            if (_loading)
                return;

            Failure = null;
            _loading = true;
            Task.WhenAll(HighScoreManager.Reload(Config.Backend.ServerHighScores),
                LowScoreManager.Reload(Config.Backend.ServerLowScores)).ContinueWith(
                task =>
                {
                    _loading = false;

                    if (task.Exception != null)
                    {
                        Failure = "Failed to load server scores!";
                        return;
                    }

                    Loaded = true;

                    // If we're logged in already, check for high scores we may want to update on the server!
                    if (CustomBeatmaps.UserSession.LoggedIn)
                    {
                        CheckDesyncedHighScores(CustomBeatmaps.UserSession.Username);
                    }
                });
        }

        public void SetCurrentBeatmapKey(string beatmapKey)
        {
            ScheduleHelper.SafeLog($"HIGH SCORE KEY: {beatmapKey}");
            CurrentBeatmapKey = beatmapKey;
        }
        public void ResetCurrentBeatmapKey()
        {
            ScheduleHelper.SafeLog($"HIGH SCORE KEY reset");
            CurrentBeatmapKey = null;
        }

        public async void SendScore(string beatmapKey, int score, float accuracy, bool noMiss, bool fullCombo)
        {
            if (!CustomBeatmaps.UserSession.LoggedIn)
                throw new InvalidOperationException("Can't send high score because user is not logged in!");
            string uniqueId = CustomBeatmaps.UserSession.UniqueId;
            int fullComboMode = fullCombo ? 2 : (noMiss ? 1 : 0);
            await UserServerHelper.PostScore(Config.Backend.ServerUserURL, new UserServerHelper.PostScoreRequest(
                uniqueId, beatmapKey, score, accuracy, fullComboMode
            ));
        }

        [CanBeNull]
        public List<KeyValuePair<string, BeatmapHighScoreEntry>> GetHighScores(string beatmapKey)
        {
            var result = HighScoreManager.GetScores(beatmapKey);
            if (result != null)
            {
                result.Sort((left, right) => right.Value.Score.CompareTo(left.Value.Score));
                return result;
            }
            return null;
        }
        [CanBeNull]
        public List<KeyValuePair<string, BeatmapHighScoreEntry>> GetLowScores(string beatmapKey)
        {
            var result = LowScoreManager.GetScores(beatmapKey);
            if (result != null)
            {
                result.Sort((left, right) => left.Value.Score.CompareTo(right.Value.Score));
                return result;
            }
            return null;
        }

        /// <summary>
        /// Get all high scores that we CAN update on the server side
        /// </summary>
        public void CheckDesyncedHighScores(string username)
        {
            DesyncedHighScores = GetDesyncedHighScoresInternal(UnbeatableHelper.LoadWhiteLabelHighscores(), username,
                HighScoreManager);
        }

        /// <summary>
        /// Sends every high score from
        /// </summary>
        public void SyncUpHighScoresFromLocal()
        {
            foreach (var score in DesyncedHighScores)
            {
                var key = score.ServerBeatmapKey;
                ScheduleHelper.SafeLog($"SYNCING {key} SCORE TO SERVER");
                var s = score.LocalHighScore;
                bool noMiss = s._notes.ContainsKey("Miss") && s._notes["Miss"] <= 0;
                bool fc = noMiss && s._notes.ContainsKey("Barely") && s._notes["Barely"] <= 0;
                SendScore(key, s.score, s.accuracy, noMiss, fc);
            }

            // Clear
            DesyncedHighScores = new DesyncedScore[0];
        }

        private static DesyncedScore[] GetDesyncedHighScoresInternal(HighScoreList whiteLabelHighScores, string username, ScoreManager highScores)
        {
            var (whiteLabelScores, serverScores ) = UserServerHelper.FilterValidHighScores(whiteLabelHighScores);

            List<DesyncedScore> resultingBeatmapKeys = new List<DesyncedScore>();

            string serverPackagesFullPath = Path.GetFullPath(Config.Mod.ServerPackagesDir);

            foreach (var score in serverScores)
            {
                if (score.song.StartsWith("CUSTOMBEATMAPS_SERVER::"))
                {
                    string relativeToServerPackages = score.song.Substring("CUSTOMBEATMAPS_SERVER::".Length);
                    string localPath = Path.Combine(serverPackagesFullPath, relativeToServerPackages);
                    string key =
                        UserServerHelper.GetHighScoreBeatmapKeyFromLocalBeatmap(Config.Mod.ServerPackagesDir,
                            localPath);
                    if (CanUpdateHighScoreFromWhiteLabel(score, highScores, username, key))
                    {
                        resultingBeatmapKeys.Add(new DesyncedScore(key, score));
                    }
                }
            }
            foreach (var score in whiteLabelScores)
            {
                ScheduleHelper.SafeLog($"WHITE LABEL SCORE: {score.song}");
                string key = UserServerHelper.GetHighScoreBeatmapKeyFromUnbeatableBeatmap(score.song);
                if (CanUpdateHighScoreFromWhiteLabel(score, highScores, username, key))
                {
                    resultingBeatmapKeys.Add(new DesyncedScore(key, score));
                }
            }

            return resultingBeatmapKeys.ToArray();
        }

        private static bool CanUpdateHighScoreFromWhiteLabel(HighScoreItem whiteLabelScore, ScoreManager highScores, string username, string beatmapKey)
        {
            var mapScores = highScores.GetScores(beatmapKey);
            // Assume this map doesn't have scores yet, which is fine!
            if (mapScores == null)
            {
                return true;
            }

            foreach (var keyValuePair in mapScores)
            {
                if (keyValuePair.Key == username)
                {
                    return whiteLabelScore.score > keyValuePair.Value.Score;
                }
            }

            // Assume we don't have our user registered yet, which is fine!
            return true;
        }

        private class ScoreManager
        {
            private UserHighScores _scores;
            public async Task Reload(string url)
            {
                _scores = await UserServerHelper.GetUserScores(url);
            }

            [CanBeNull]
            public List<KeyValuePair<string, BeatmapHighScoreEntry>> GetScores(string beatmapKey)
            {
                if (_scores.Scores != null && _scores.Scores.TryGetValue(beatmapKey, out var userScores))
                {
                    var result = userScores.ToList();
                    return result;
                }
                return null;
            }
        }
        
        public struct DesyncedScore
        {
            public string ServerBeatmapKey;
            public HighScoreItem LocalHighScore;

            public DesyncedScore(string serverBeatmapKey, HighScoreItem localHighScore)
            {
                ServerBeatmapKey = serverBeatmapKey;
                LocalHighScore = localHighScore;
            }
        }
    }
}
