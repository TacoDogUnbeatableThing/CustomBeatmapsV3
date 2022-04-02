using CustomBeatmaps.Util;
using UnityEngine;

namespace CustomBeatmaps.UI
{
    public static class DiscordButtonUI
    {
        public static void Render(string label="Check out our Discord!")
        {
            if (GUILayout.Button(label, GUILayout.ExpandWidth(false)))
            {
                Application.OpenURL(Config.Backend.DiscordInviteLink);
            }
        }
    }
}