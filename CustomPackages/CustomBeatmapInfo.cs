using Rhythm;
using UnityEngine;

namespace CustomBeatmaps.CustomPackages
{
    public class CustomBeatmapInfo : BeatmapInfo
    {
        public readonly string Artist;
        public readonly string BeatmapCreator;
        public readonly string SongName;
        public readonly string Difficulty;
        public readonly string RealAudioKey;
        public readonly string OsuPath;

        public CustomBeatmapInfo(TextAsset textAsset, string artist,
            string beatmapCreator, string songName, string difficulty, string realAudioKey, string osuPath) : base(textAsset, difficulty)
        {
            RealAudioKey = realAudioKey;
            OsuPath = osuPath;
            Artist = artist;
            SongName = songName;
            Difficulty = difficulty;
            BeatmapCreator = beatmapCreator;
        }

        public override string ToString()
        {
            return $"{{{SongName} by {Artist} ({Difficulty}) mapped {BeatmapCreator} ({RealAudioKey})}}";
        }
    }
}