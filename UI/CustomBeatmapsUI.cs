
using System;
using CustomBeatmaps.UISystem;
using CustomBeatmaps.Util;
using UnityEngine;

namespace CustomBeatmaps.UI
{
    public static class CustomBeatmapsUI
    {
        public static void Render()
        {
            /*
             *  State:
             *      - Which tab we're on
             *  
             *  UI:
             *      List:
             *      - Tab Picker
             *      - User/Online Info
             *      - Depending on current tab, local/server/submission/OSU!
             *      - Assist info on the bottom
             */

            GUIHelper.SetDefaultStyles();

            var (tab, setTab) = Reacc.UseState(Tab.Online);

            switch (tab)
            {
                case Tab.Online:
                    OnlinePackageListUI.Render(() => RenderListTop(tab, setTab));
                    break;
                case Tab.Local:
                    LocalPackageListUI.Render(() => RenderListTop(tab, setTab));
                    break;
                case Tab.Submissions:
                    RenderListTop(tab, setTab);
                    GUILayout.Label("Not implemented yet");
                    break;
                case Tab.Osu:
                    RenderListTop(tab, setTab);
                    GUILayout.Label("Not implemented yet");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Keyboard Shortcut: Cycle tabs
            if (GUIHelper.CanDoInput() && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            {
                int count = Enum.GetValues(typeof(Tab)).Length;
                int ind = (int)tab;
                if (Input.GetKeyDown(KeyCode.PageDown))
                {
                    ind -= 1;
                    if (ind < 0)
                        ind = count - 1;
                    setTab((Tab) ind);
                }
                else if (Input.GetKeyDown(KeyCode.PageUp))
                {
                    ind += 1;
                    ind %= count;
                    setTab((Tab) ind);
                }
            }
        }

        private static void RenderListTop(Tab tab, Action<Tab> onSetTab)
        {
            EnumTooltipPickerUI.Render(tab, onSetTab);
            UserOnlineInfoBarUI.Render();
        }
    }
}