using CustomBeatmaps.Util;
using UnityEngine;

namespace CustomBeatmaps.UI
{
    public static class DiscordButtonUI
    {
        public static void Render(string label="<color=orange><b>Check out our Discord!</b></color>")
        {
            if (GUILayout.Button(label, GUILayout.ExpandWidth(false)))
            {
                Application.OpenURL(Config.Backend.DiscordInviteLink);
            }
        }
    }
}