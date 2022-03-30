using HarmonyLib;
using Rhythm;

namespace CustomBeatmaps.Patches
{
    /// <summary>
    /// The practice room opening is the coolest way to start the game. Let's keep it as an opening. 
    /// </summary>
    public static class DisablePracticeRoomOpenerPatch
    {
        [HarmonyPatch(typeof(RhythmStencilMasks), "Init")]
        [HarmonyPrefix]
        private static void DisableWhenCustomBeatmaps(RhythmStencilMasks __instance, ref bool __runOriginal)
        {
            if (CustomBeatmapLoadingOverridePatch.CustomBeatmapSet())
            {
                __runOriginal = false;
                __instance.gameObject.SetActive(false);
            }
        }
    }
}