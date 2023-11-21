using System;
using UnityEngine;

namespace CustomBeatmaps.UI.PackageList
{
    public static class SortModePickerUI
    {
        public static void Render(SortMode sortMode, Action<SortMode> setSortMode)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Sort By", GUILayout.ExpandWidth(false));
            EnumTooltipPickerUI.Render(sortMode, setSortMode);
            GUILayout.EndHorizontal();
        }
    }
}