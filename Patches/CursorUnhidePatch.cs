using CustomBeatmaps.UI;
using CustomBeatmaps.UI.Highscore;
using HarmonyLib;
using UnityEngine;

namespace CustomBeatmaps.Patches
{
    /// <summary>
    /// JeffBezosController hides the cursor, but we have our UI's open sometimes so don't do that lel
    /// </summary>
    public static class CursorUnhidePatch
    {
        [HarmonyPatch(typeof(JeffBezosController), "Update")]
        [HarmonyPostfix]
        public static void JeffBezosPostUpdate()
        {
            if (CustomBeatmapsUIBehaviour.Opened || HighScoreUIBehaviour.Opened)
                Cursor.visible = true;
        }
    }
}