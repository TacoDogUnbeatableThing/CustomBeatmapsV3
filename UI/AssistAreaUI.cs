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
            var (scrollSpeedText, setScrollSpeedText) = Reacc.UseState(() => "" + JeffBezosController.GetScrollSpeedIndex());
            var (horizontalScrollPos, setHorizontalScrollPos) = Reacc.UseState(Vector2.zero);

            Reacc.UseEffect(() =>
            {
                setScrollSpeedText("" + JeffBezosController.GetScrollSpeedIndex());
            }, new object[] {JeffBezosController.GetScrollSpeedIndex()});

            setHorizontalScrollPos(GUILayout.BeginScrollView(horizontalScrollPos, GUILayout.Height(48)));
            GUILayout.BeginHorizontal();

            JeffBezosController.SetAssistMode(Toggle(JeffBezosController.GetAssistMode(), "Assist Mode"));
            JeffBezosController.SetNoFail(Toggle(JeffBezosController.GetNoFail(), "No Fail"));
            // Custom settings, like One Life mode
            CustomBeatmaps.Memory.OneLifeMode = GUILayout.Toggle(CustomBeatmaps.Memory.OneLifeMode, "ONE LIFE");
            CustomBeatmaps.Memory.FlipMode = GUILayout.Toggle(CustomBeatmaps.Memory.FlipMode, "FLIP MODE");
            JeffBezosController.SetSongSpeed(GUILayout.Toolbar(JeffBezosController.GetSongSpeed(),
                new[] {"Regular", "Half Time", "Double Time"}));

            GUILayout.BeginHorizontal();
            GUILayout.Label("Scroll Speed:", GUILayout.Width(64 + 32));
            scrollSpeedText = GUILayout.TextField(scrollSpeedText, GUILayout.ExpandWidth(false));
            GUILayout.Space(16);
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
            GUILayout.Label("Stage:", GUILayout.Width(64));
            CustomBeatmaps.Memory.SelectedRoom = GUILayout.Toolbar(CustomBeatmaps.Memory.SelectedRoom,
                UnbeatableHelper.Rooms.Select(room => room.Name).ToArray(), GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUILayout.EndHorizontal();
            
            GUILayout.EndScrollView();
        }
        private static int Toggle(int mode, string text)
        {
            return GUILayout.Toggle(mode != 0, text, GUILayout.ExpandWidth(false)) ? 1 : 0;
        }
    }
}