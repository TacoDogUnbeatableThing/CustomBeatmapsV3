using CustomBeatmaps.Util;
using HarmonyLib;
using Rhythm;

namespace CustomBeatmaps.Patches
{
    public static class FlipModePatch
    {
        [HarmonyPatch(typeof(Rhythm.RhythmController), "CreateNote", typeof(NoteInfo), typeof(bool))]
        [HarmonyPrefix]
        private static void FlipNotesBeforeGenerating(ref NoteInfo info)
        {
            if (CustomBeatmaps.Memory.FlipMode)
            {
                switch (info.height)
                {
                    case Height.Low:
                        info.height = Height.Top;
                        break;
                    case Height.Top:
                        info.height = Height.Low;
                        break;
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
