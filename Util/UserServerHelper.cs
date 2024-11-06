using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;

using File = Pri.LongPath.File;
using Path = Pri.LongPath.Path;
using Directory = Pri.LongPath.Directory;

namespace CustomBeatmaps.Util
{
    public static class UserServerHelper
    {
        struct RegisterUserRequest
        {
            [JsonProperty("username")] public string Username;
            public RegisterUserRequest(string username) : this()
            {
                Username = username;
            }
        }

        struct GetUserDataRequest
        {
            [JsonProperty("id")] public string UniqueUserId;
            public GetUserDataRequest(string uniqueUserId)
            {
                UniqueUserId = uniqueUserId;
            }
        }

        public struct PostScoreRequest
        {
            [JsonProperty("uniqueUserId")] public string UniqueUserId;
            [JsonProperty("beatmapKey")] public string BeatmapKey;
            [JsonProperty("score")] public int Score;
            [JsonProperty("accuracy")] public float Accuracy;
            [JsonProperty("fc")] public int FullComboMode;

            public PostScoreRequest(string uniqueUserId, string beatmapKey, int score, float accuracy, int fullComboMode)
            {
                UniqueUserId = uniqueUserId;
                BeatmapKey = beatmapKey;
                Score = score;
                Accuracy = accuracy;
                FullComboMode = fullComboMode;
            }
        }

        public struct ReceiveScoreData
        {
            [JsonProperty("highscore")] public bool GotNewHighscore;
        }

        public static string GetHighScoreBeatmapKeyFromServerBeatmap(string serverPackageURL, string beatmapRelativePath)
        {
            string packageName = Path.GetFileName(serverPackageURL);
            return $"online/{packageName}/{beatmapRelativePath}";
        }

        public static string GetHighScoreBeatmapKeyFromLocalBeatmap(string packageListDir, string beatmapOSUPath)
        {
            string fullPath = Path.GetFullPath(beatmapOSUPath);
            string packageFullPath = Path.GetFullPath(packageListDir);
            return $"online/{fullPath.Substring(packageFullPath.Length + 1).Replace("\\", "/")}";
        }

        public static string GetHighScoreBeatmapKeyFromUnbeatableBeatmap(string beatmapPath)
        {
            if (beatmapPath == null)
                return null;
            return $"game/{beatmapPath}";
        }

        public static string GetHighScoreLocalEntryFromCustomBeatmap(string serverPackageDir, string localPackageDir,
            string beatmapOSUPath)
        {
            serverPackageDir = Path.GetFullPath(serverPackageDir);
            localPackageDir = Path.GetFullPath(localPackageDir);
            if (beatmapOSUPath.StartsWith(serverPackageDir))
            {
                return $"CUSTOMBEATMAPS_SERVER::{beatmapOSUPath.Substring(serverPackageDir.Length + 1)}";
            }
            if (beatmapOSUPath.StartsWith(localPackageDir))
            {
                return $"CUSTOMBEATMAPS_USER::{beatmapOSUPath.Substring(localPackageDir.Length + 1)}";
            }

            EventBus.ExceptionThrown?.Invoke(new InvalidOperationException($"Custom beatmap not in server/local folder: {beatmapOSUPath}"));
            return beatmapOSUPath;
        }

        public static async Task<NewUserInfo> RegisterUser(string userServerURL, string username)
        {
            return await FetchHelper.PostJSON<NewUserInfo>(userServerURL + "/newuser", new RegisterUserRequest(username));
        }

        public static async Task<UserInfo> GetUserInfo(string userServerURL, string uniqueUserId)
        {
            return await FetchHelper.PostJSON<UserInfo>(userServerURL + "/user", new GetUserDataRequest(uniqueUserId));
        }

        public static async Task<UserHighScores> GetUserScores(string highScoreURL)
        {
            Dictionary<string, Dictionary<string, BeatmapHighScoreEntry>> scores =
                await FetchHelper.GetJSON<Dictionary<string, Dictionary<string, BeatmapHighScoreEntry>>>(highScoreURL);
            return new UserHighScores
            {
                Scores = scores
            };
        }

        public static async Task<bool> PostScore(string userServerURL, PostScoreRequest request)
        {
            var response = await FetchHelper.PostJSON<ReceiveScoreData>(userServerURL + "/score", request);
            return response.GotNewHighscore;
        }

        public static string LoadUserSession(string path)
        {
            // Huh...
            return File.ReadAllText(path);
        }

        public static bool LocalUserSessionExists(string path)
        {
            // Huh...
            return File.Exists(path);
        }

        public static void SaveUserSession(string path, string uniqueUserId)
        {
            // Huh...
            File.WriteAllText(path, uniqueUserId);
        }

        /// <summary>
        /// We could have a variety of high scores, but we really only care about two types:
        ///
        /// CUSTOMBEATMAPS_SERVER::[BEATMAP PATH relative to SERVER_PACKAGES].osu
        /// [WHITE LABEL SONG]/[DIFFICULTY]
        ///
        /// Everything else should be filtered out, as I've probably generated it by accident during testing lel
        /// </summary>
        public static (List<HighScoreItem> whiteLabelScores, List<HighScoreItem> serverScores) FilterValidHighScores(
            HighScoreList list)
        {
            List<HighScoreItem> whiteLabelScores = new List<HighScoreItem>();
            List<HighScoreItem> serverScores = new List<HighScoreItem>();


            string serverPackagePrefix = "CUSTOMBEATMAPS_SERVER::";

            foreach (var score in list._highScores.Values)
            {
                string songPath = score.song;
                // Try to parse as server?
                // CUSTOMBEATMAPS_SERVER:: ...
                if (songPath.StartsWith(serverPackagePrefix))
                {
                    //string beatmapRelativeToServerPackages = key.Substring(serverPackagePrefix.Length);
                    serverScores.Add(score);
                    continue;
                }
                // Try to parse as white label?
                // [Song]/[Difficulty]
                if (UnbeatableHelper.IsValidUnbeatableSongPath(score.song))
                {
                    whiteLabelScores.Add(score);
                    continue;
                }
                ScheduleHelper.SafeLog($"    (filtered out score: {score.song}");
            }

            return (whiteLabelScores, serverScores);
        }

    }

}
