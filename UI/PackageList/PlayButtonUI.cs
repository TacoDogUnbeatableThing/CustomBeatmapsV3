using CustomBeatmaps.UISystem;
using CustomBeatmaps.Util;
using Rewired;
using UnityEngine;

namespace CustomBeatmaps.UI.PackageList
{
    public static class PlayButtonUI
    {
        private static Player _rewired;

        public static bool Render(string labelBig, string labelSmall="")
        {
            Reacc.UseEffect(() =>
            {
                _rewired = ReInput.players.GetPlayer(0);
            });

            return GUILayout.Button($"<size=24>{labelBig}</size>\n{labelSmall}", GUILayout.Height(72), GUILayout.ExpandHeight(false)) 
                   || (GUIHelper.CanDoInput() && (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.KeypadEnter) || _rewired.GetButtonDown("Interact")));
        }
    }
}
