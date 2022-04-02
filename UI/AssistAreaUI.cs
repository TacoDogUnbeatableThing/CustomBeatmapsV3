using System.Linq;
using CustomBeatmaps.UISystem;
using CustomBeatmaps.Util;
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
            // Custom settings, like One Life mode
            CustomBeatmaps.Memory.OneLifeMode = GUILayout.Toggle(CustomBeatmaps.Memory.OneLifeMode, "ONE LIFE");
            CustomBeatmaps.Memory.FlipMode = GUILayout.Toggle(CustomBeatmaps.Memory.FlipMode, "FLIP MODE");
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


            // Room options
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            GUILayout.Label("Stage:", GUILayout.ExpandWidth(false));
            CustomBeatmaps.Memory.SelectedRoom = GUILayout.Toolbar(CustomBeatmaps.Memory.SelectedRoom,
                UnbeatableHelper.Rooms.Select(room => room.Name).ToArray(), GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUILayout.EndHorizontal();
        }
        private static int Toggle(int mode, string text)
        {
            return GUILayout.Toggle(mode != 0, text, GUILayout.ExpandWidth(false)) ? 1 : 0;
        }
    }
}