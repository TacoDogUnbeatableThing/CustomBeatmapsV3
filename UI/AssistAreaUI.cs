using System;
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

            Toggle(JeffBezosController.GetAssistMode(), "Assist Mode", JeffBezosController.SetAssistMode);
            Toggle(JeffBezosController.GetNoFail(), "No Fail", JeffBezosController.SetNoFail);

            // Custom settings, like One Life mode
            CustomBeatmaps.Memory.OneLifeMode = GUILayout.Toggle(CustomBeatmaps.Memory.OneLifeMode, "ONE LIFE");
            CustomBeatmaps.Memory.FlipMode = GUILayout.Toggle(CustomBeatmaps.Memory.FlipMode, "FLIP MODE");

            int prevSongSpeed = JeffBezosController.GetSongSpeed();
            int newSongSpeed = Toolbar.Render(prevSongSpeed,
                new[] {"Regular", "Half Time", "Double Time"});
            if (newSongSpeed != -1 && prevSongSpeed != newSongSpeed)
            {
                JeffBezosController.SetSongSpeed(newSongSpeed);
                FileStorage.profile.SaveBeatmapOptions();
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("Scroll Speed:", GUILayout.Width(64 + 32));
            scrollSpeedText = GUILayout.TextField(scrollSpeedText, GUILayout.ExpandWidth(false));
            GUILayout.Space(16);
            setScrollSpeedText(scrollSpeedText);
            int spd;
            if (int.TryParse(scrollSpeedText, out spd))
            {
                if (JeffBezosController.GetScrollSpeedIndex() != spd)
                {
                    JeffBezosController.SetScrollSpeed(spd);
                    FileStorage.profile.SaveBeatmapOptions();
                }
            }
            GUILayout.Label($"= {(JeffBezosController.GetScrollSpeedIndex() + 1) * 0.2f:0.0}");
            GUILayout.EndHorizontal();


            // Room options
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            GUILayout.Label("Stage:", GUILayout.Width(64));
            int newRoomSelected = Toolbar.Render(CustomBeatmaps.Memory.SelectedRoom,
                UnbeatableHelper.Rooms.Select(room => room.Name).ToArray());
            if (newRoomSelected != -1)
            {
                CustomBeatmaps.Memory.SelectedRoom = newRoomSelected;
            }
            GUILayout.EndHorizontal();

            GUILayout.EndHorizontal();

            GUILayout.EndScrollView();
        }
        private static int Toggle(int mode, string text, Action<int> setter)
        {
            int result = GUILayout.Toggle(mode != 0, text, GUILayout.ExpandWidth(false)) ? 1 : 0;

            if (result != mode)
            {
                // Write UI because otherwise it gets overriden.
                // Might be fixed in later versions of the game
                setter(result);
                FileStorage.profile.SaveBeatmapOptions();
            }

            return result;
        }
    }
}