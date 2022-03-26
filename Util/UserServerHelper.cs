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
            [JsonProperty("serverPackageURL")] public string ServerPackageURL;
            [JsonProperty("beatmapIndex")] public int BeatmapIndex;
            [JsonProperty("score")] public int Score;
            [JsonProperty("accuracy")] public float Accuracy;
            [JsonProperty("fc")] public bool Fc;

            public PostScoreRequest(string uniqueUserId, string serverPackageURL, int beatmapIndex, int score, float accuracy, bool fc)
            {
                UniqueUserId = uniqueUserId;
                ServerPackageURL = serverPackageURL;
                BeatmapIndex = beatmapIndex;
                Score = score;
                Accuracy = accuracy;
                Fc = fc;
            }
        }

        public struct ReceiveScoreData
        {
            [JsonProperty("highscore")] public bool GotNewHighscore;
        }
        
        public static async Task<NewUserInfo> RegisterUser(string userServerURL, string username)
        {
            return await FetchHelper.PostJSON<NewUserInfo>(userServerURL + "/newuser", new RegisterUserRequest(username));
        }

        public static async Task<UserInfo> GetUserInfo(string userServerURL, string uniqueUserId)
        {
            return await FetchHelper.PostJSON<UserInfo>(userServerURL + "/user", new GetUserDataRequest(uniqueUserId));
        }

        public static async Task<bool> PostScore(string userServerURL, PostScoreRequest request)
        {
            var response = await FetchHelper.PostJSON<ReceiveScoreData>(userServerURL + "/score", request);
            return response.GotNewHighscore;
        }

        public static async Task<HighscoreTable> GetHighscores(
            string url)
        {
            var scoreTable = await FetchHelper.GetJSON<Dictionary<string, Dictionary<string, Dictionary<string, HighscoreTable.UserScore>>>>(url);
            return new HighscoreTable(scoreTable);
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

    public class HighscoreTable
    {
        public struct UserScore
        {
            [JsonProperty("score")] public int Score;
            [JsonProperty("accuracy")] public float Accuracy;
            [JsonProperty("fc")] public bool FullClear;
        }

        private readonly Dictionary<string, Dictionary<string, Dictionary<string, UserScore>>> _scores;

        public HighscoreTable(Dictionary<string, Dictionary<string, Dictionary<string, UserScore>>> scores)
        {
            _scores = scores;
        }

        [CanBeNull]
        public Dictionary<string, UserScore> GetUserScores(string serverPackageURL, int beatmapIndex)
        {
            if (_scores.TryGetValue(serverPackageURL, out var beatmapScores) &&
                beatmapScores.TryGetValue(beatmapIndex.ToString(), out Dictionary<string, UserScore> userScores))
            {
                return userScores;
            }
            return null;
        }
    }
}
