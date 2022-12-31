using HarmonyLib;

namespace CustomBeatmaps.Patches
{
    public static class PauseMenuPatch
    {
        [HarmonyPatch(typeof(PauseMenu), "Start")]
        [HarmonyPostfix]
        private static void DisableQuitNameChange(ref string ___cachedSceneName)
        {
            if (CustomBeatmapLoadingOverridePatch.CustomBeatmapSet())
            {
                ___cachedSceneName = ""; // We don't care what it is tbh
            }
        }
    }
}