using System;
using CustomBeatmaps.CustomPackages;
using CustomBeatmaps.Util;
using HarmonyLib;
using Rhythm;
using UnityEngine;

namespace CustomBeatmaps.Patches
{
    public static class CustomBeatmapLoadingOverridePatch
    {
        private static CustomBeatmapInfo _override;

        public static void SetOverrideBeatmap(CustomBeatmapInfo toOverride)
        {
            _override = toOverride;
        }

        public static void ResetOverrideBeatmap()
        {
            _override = null;
        }

        public static bool CustomBeatmapSet()
        {
            return _override != null;
        }

        // Inject this into our beatmap parsing to replace our beatmap with the override beatmap
        private static void OverrideBeatmapParsing(out BeatmapInfo beatmapInfo, out Beatmap beatmap, out string audioKey, out string songName)
        {
            ScheduleHelper.SafeLog($"PARSING BEATMAP {_override.OsuPath}");
            BeatmapParserEngine beatmapParserEngine = new BeatmapParserEngine();
            beatmap = ScriptableObject.CreateInstance<Beatmap>();
            beatmapInfo = _override;
            beatmapParserEngine.ReadBeatmap(beatmapInfo.text, ref beatmap);
            songName = _override.SongName;
            audioKey = _override.RealAudioKey;
        }

        [HarmonyPatch(typeof(BeatmapParser), "ParseBeatmap", new Type[0])]
        [HarmonyPrefix]
        private static void ParseBeatmapInstance(BeatmapParser __instance, ref bool __runOriginal)
        {
            if (CustomBeatmapSet())
            {
                __runOriginal = false;
                OverrideBeatmapParsing(out _, out __instance.beatmap, out __instance.audioKey, out _);
            }
        }

        [HarmonyPatch(
            typeof(BeatmapParser), "ParseBeatmap",
            new[]{typeof(BeatmapIndex), typeof(string), typeof(BeatmapInfo), typeof(Beatmap), typeof(string)},
            new [] {ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out})
        ]
        [HarmonyPrefix]
        private static void ParseBeatmapStatic(BeatmapIndex beatmapIndex, string beatmapPath, out BeatmapInfo beatmapInfo, out Beatmap beatmap, out string songName, ref bool __runOriginal)
        {
            if (CustomBeatmapSet())
            {
                __runOriginal = false;
                OverrideBeatmapParsing(out beatmapInfo, out beatmap, out _, out songName);
            }
            else
            {
                // Required to set these, but they will get overriden by the original function.
                beatmapInfo = null;
                beatmap = null;
                songName = null;
            }
        }

        [HarmonyPatch(typeof(Rhythm.RhythmController), "Start")]
        [HarmonyPostfix]
        private static void StartPostfix(Rhythm.RhythmController __instance)
        {
            if (_override != null)
            {
                string loadFrom = __instance.parser.audioKey;
                //Debug.Log($"PRELOADING: {loadFrom}");
                __instance.songTracker.PreloadFromFile(loadFrom);
            }
        }
    }
}