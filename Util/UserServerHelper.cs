using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;

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

        public static string GetHighScoreBeatmapKeyFromLocalBeatmap(string packageDir, string beatmapOSUPath)
        {
            string fullPath = Path.GetFullPath(beatmapOSUPath);
            string packageFullPath = Path.GetFullPath(packageDir);
            return $"online/{fullPath.Substring(packageFullPath.Length + 1)}";
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
    }

}
