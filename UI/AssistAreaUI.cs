using CustomBeatmaps.UISystem;
using UnityEngine;

namespace CustomBeatmaps.UI
{
    public static class AssistAreaUI
    {
        public static void Render()
        {
            // Kinda spooky but 4 is the magic number.
            (string scrollSpeedText, var setScrollSpeedText) = Reacc.UseState("4");

            GUILayout.BeginHorizontal();

            JeffBezosController.SetAssistMode(Toggle(JeffBezosController.GetAssistMode(), "Assist Mode"));
            JeffBezosController.SetNoFail(Toggle(JeffBezosController.GetNoFail(), "No Fail"));
            JeffBezosController.SetSongSpeed(GUILayout.Toolbar(JeffBezosController.GetSongSpeed(),
                new[] {"Regular", "Half Time", "Double Time"}));

            GUILayout.BeginHorizontal();
            GUILayout.Label("Scroll Speed:", GUILayout.ExpandWidth(false));
            scrollSpeedText = GUILayout.TextField(scrollSpeedText, GUILayout.ExpandWidth(false));
            setScrollSpeedText(scrollSpeedText);
            int spd;
            if (int.TryParse(scrollSpeedText, out spd))
            {
                JeffBezosController.SetScrollSpeed(spd);
            }
            GUILayout.Label($"= {(JeffBezosController.GetScrollSpeedIndex() + 1) * 0.2f:0.0}");
            GUILayout.EndHorizontal();
            
            GUILayout.EndHorizontal();
        }
        private static int Toggle(int mode, string text)
        {
            return GUILayout.Toggle(mode != 0, text, GUILayout.ExpandWidth(false)) ? 1 : 0;
        }
    }
}