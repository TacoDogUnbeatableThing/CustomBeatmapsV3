using System;
using UnityEngine;

namespace CustomBeatmaps.UI.PackageList
{
    public static class SortModePickerUI
    {
        public static void Render(SortMode sortMode, Action<SortMode> setSortMode)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Sort By");
            EnumTooltipPickerUI.Render(sortMode, setSortMode, GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();
        }
    }
}