using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CustomBeatmaps.Util
{
    public struct CustomServerPackageList
    {
        [JsonProperty("packages")]
        public CustomServerPackage[] Packages;

        public override string ToString()
        {
            return string.Join(",\n", Packages);
        }
    }

    public struct CustomServerPackage
    {
        [JsonProperty("filePath")]
        public string ServerURL;
        [JsonProperty("time")]
        public DateTime UploadTime;
        [JsonProperty("beatmaps")]
        public Dictionary<string, CustomServerBeatmap> Beatmaps;
        public override string ToString()
        {
            return $"{{[{string.Join(", ", Beatmaps)}] at {ServerURL} on {UploadTime}}}";
        }

        public string GetServerPackageURL()
        {
            return ServerURL;
        }
    }

    public struct CustomServerBeatmap
    {
        [JsonProperty("name")]
        public string Name;
        [JsonProperty("artist")]
        public string Artist;
        [JsonProperty("creator")]
        public string Creator;
        [JsonProperty("difficulty")]
        public string Difficulty;
        [JsonProperty("audioFileName")]
        public string AudioFileName;

        public override string ToString()
        {
            return $"{{{Name} ({Difficulty}) by {Artist}: mapped by {Creator}}}";
        }
    }

    public struct ServerSubmissionPackage
    {
        [JsonProperty("username")]
        public string Username;
        [JsonProperty("avatarURL")]
        public string AvatarURL;
        [JsonProperty("downloadURL")]
        public string DownloadURL;
    }
    public struct UserInfo
    {
        [JsonProperty("name")]
        public string Name;
    }

    public struct NewUserInfo
    {
        [JsonProperty("id")]
        public string UniqueId;
    }

    public struct BeatmapHighScoreEntry
    {
        [JsonProperty("score")]
        public int Score;
        [JsonProperty("accuracy")]
        public float Accuracy;
        // TODO: just use an enum for gods sake
        // 0: None
        // 1: No misses
        // 2: Full clear
        [JsonProperty("fc")]
        public int FullComboMode;
    }
    public struct UserHighScores
    {
        // scores["beatmap key"]["Username"] = score
        public Dictionary<string, Dictionary<string, BeatmapHighScoreEntry>> Scores;
    }
}
