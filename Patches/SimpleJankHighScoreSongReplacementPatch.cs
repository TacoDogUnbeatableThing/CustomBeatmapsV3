using System.IO;
using CustomBeatmaps.Util;
using HarmonyLib;

namespace CustomBeatmaps.Patches
{
    /// <summary>
    /// TLDR:
    ///
    /// Custom Beatmaps are loaded via full path
    ///
    /// This means the high scores are also stored by the beatmap's full path
    ///
    /// The proper solution would be to ALWAYS reference custom beatmaps by the local path
    ///
    /// However, that would require changes to this system, so I'll just INJECT when a high score is saved
    /// to account for custom cases
    ///
    /// Not the best practice but I spent my whole weekend working on this and I just wanna finish implementing high scores lel
    /// </summary>
    public static class SimpleJankHighScoreSongReplacementPatch
    {
        [HarmonyPatch(typeof(HighScoreList), "ReplaceHighScore")]
        [HarmonyPrefix]
        private static void ReplaceHighScoreInjectCustomPath(ref string song)
        {
            string serverMapsPath = Path.GetFullPath(Config.Mod.ServerPackagesDir);
            string localMapsPath = Path.GetFullPath(Config.Mod.UserPackagesDir);
            if (song.StartsWith(serverMapsPath))
            {
                song = $"CUSTOMBEATMAPS_SERVER::{song.Substring(serverMapsPath.Length + 1)}";
            } else if (song.StartsWith(localMapsPath))
            {
                song = $"CUSTOMBEATMAPS_USER::{song.Substring(localMapsPath.Length + 1)}";
            }
        }
    }
}