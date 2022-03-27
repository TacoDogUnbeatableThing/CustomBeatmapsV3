using System;
using System.Collections.Generic;
using System.Linq;
using CustomBeatmaps.Util;
using JetBrains.Annotations;

namespace CustomBeatmaps.CustomPackages
{
    public class ServerHighScoreManager
    {
        public string Failure { get; private set; }
        public bool Loaded { get; private set; }

        private ScoreManager HighScoreManager = new ScoreManager();
        private ScoreManager LowScoreManager = new ScoreManager();

        public bool CanSendScoreForCurrentMap => _currentBeatmapKey != null;
        
        private string _currentBeatmapKey;

        public void Reload()
        {
            Failure = null;
            HighScoreManager.Reload(Config.Backend.ServerHighScores, () => Loaded = true, fail => Failure = fail);
            LowScoreManager.Reload(Config.Backend.ServerLowScores, () => Loaded = true, fail => Failure = fail);
        }
        public void SetCurrentBeatmapKey(string beatmapKey)
        {
            ScheduleHelper.SafeLog($"HIGH SCORE KEY: {beatmapKey}");
            _currentBeatmapKey = beatmapKey;
        }
        public void ResetCurrentBeatmapKey()
        {
            ScheduleHelper.SafeLog($"HIGH SCORE KEY reset");
            _currentBeatmapKey = null;
        }

        public async void SendScore(int score, float accuracy, bool noMiss, bool fullCombo)
        {
            if (_currentBeatmapKey == null)
                throw new InvalidOperationException("Can't send high score because a current beatmap/package is not set!");
            if (!CustomBeatmaps.UserSession.LoggedIn)
                throw new InvalidOperationException("Can't send high score because user is not logged in!");
            string uniqueId = CustomBeatmaps.UserSession.UniqueId;
            int fullComboMode = fullCombo ? 2 : (noMiss ? 1 : 0);
            await UserServerHelper.PostScore(Config.Backend.ServerUserURL, new UserServerHelper.PostScoreRequest(
                uniqueId, _currentBeatmapKey, score, accuracy, fullComboMode
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

        private class ScoreManager
        {
            private UserHighScores _scores;
            public async void Reload(string url, Action onSuccess, Action<string> onFail)
            {
                try
                {
                    _scores = await UserServerHelper.GetUserScores(url);
                    onSuccess?.Invoke();
                }
                catch (Exception e)
                {
                    onFail?.Invoke(e.Message);
                    EventBus.ExceptionThrown?.Invoke(e);
                }
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
    }
}
