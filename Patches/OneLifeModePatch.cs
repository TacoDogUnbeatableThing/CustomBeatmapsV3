using System;
using CustomBeatmaps.Util;
using HarmonyLib;
using UnityEngine;

namespace CustomBeatmaps.Patches
{
    public static class OneLifeModePatch
    {
        [HarmonyPatch(typeof(Rhythm.RhythmController), "Miss", new Type[0])]
        [HarmonyPostfix, HarmonyPriority(Priority.HigherThanNormal)] // This will override beatmap testing, as we may want to test that.
        private static void MissFails1(Rhythm.RhythmController __instance)
        {
            MissOneLifeInject(__instance);
        }

        [HarmonyPatch(typeof(Rhythm.RhythmController), "Miss", typeof(float), typeof(bool))]
        [HarmonyPostfix, HarmonyPriority(Priority.HigherThanNormal)] // This will override beatmap testing, as we may want to test that.
        private static void MissFails2(Rhythm.RhythmController __instance)
        {
            MissOneLifeInject(__instance);
        }

        private static void MissOneLifeInject(Rhythm.RhythmController __instance)
        {
            if (CustomBeatmaps.Memory.OneLifeMode)
            {
                // One life only
                __instance.song.health = -1;
            }
        }

        [HarmonyPatch(typeof(BeatmapOptionsMenu), "Start")]
        [HarmonyPostfix]
        private static void AddNoMissOption(BeatmapOptionsMenu __instance)
        {
            // Add a new option at the END
            var noFail = UnbeatableHelper.InsertBeatmapOptionInMenu(__instance, size => size, "<i>ONE LIFE</i>");
            noFail.GetValue += () => CustomBeatmaps.Memory.OneLifeMode ? 1 : 0;
            noFail.SetValue += ind => CustomBeatmaps.Memory.OneLifeMode = (ind == 1);
        }
    }
}
