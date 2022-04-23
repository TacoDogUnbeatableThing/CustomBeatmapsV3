using System.Collections.Generic;
using CustomBeatmaps.Util;
using HarmonyLib;
using Rhythm;

namespace CustomBeatmaps.Patches
{
    public static class FlipModePatch
    {
        [HarmonyPatch(typeof(Rhythm.RhythmController), "Start")]
        [HarmonyPostfix]
        private static void FlipLoadedBeatmapsAfterLoad(ref Queue<NoteInfo> ___notes)
        {
            FlipInfo f;
            if (CustomBeatmaps.Memory.FlipMode)
            {
                var og = ___notes.ToArray();
                ___notes.Clear();
                foreach (var note in og)
                {
                    switch (note.height)
                    {
                        case Height.Low:
                            note.height = Height.Top;
                            break;
                        case Height.Top:
                            note.height = Height.Low;
                            break;
                    }
                    ___notes.Enqueue(note);
                }
            }
        }

        [HarmonyPatch(typeof(BeatmapOptionsMenu), "Start")]
        [HarmonyPostfix]
        private static void AddNoMissOption(BeatmapOptionsMenu __instance)
        {
            // Add a new option at the END
            var flipMode = UnbeatableHelper.InsertBeatmapOptionInMenu(__instance, size => size, "<i>FLIP MODE</i>");
            flipMode.GetValue += () => CustomBeatmaps.Memory.FlipMode ? 1 : 0;
            flipMode.SetValue += ind => CustomBeatmaps.Memory.FlipMode = (ind == 1);
        }
    }
}
