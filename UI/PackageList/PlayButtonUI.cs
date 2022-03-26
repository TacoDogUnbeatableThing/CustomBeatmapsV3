using CustomBeatmaps.Util;
using UnityEngine;

namespace CustomBeatmaps.UI.PackageList
{
    public static class PlayButtonUI
    {
        public static bool Render(string labelBig, string labelSmall="")
        {
            return GUILayout.Button($"<size=24>{labelBig}</size>\n{labelSmall}", GUILayout.Height(72), GUILayout.ExpandHeight(false)) 
                   || (GUIHelper.CanDoInput() && (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.KeypadEnter)));
        }
    }
}
