using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CustomBeatmaps.UI.PackageList
{
    public class PackageBeatmapPickerUI
    {
        public static void Render(List<BeatmapHeader> packageBeatmaps, int selectedBeatmapIndex,
            Action<int> onBeatmapSelect)
        {
            // If No beatmaps...

            if (packageBeatmaps.Count == 0)
            {
                return;
            }

            // Construct list of unique map names and our currently selected map's difficulties

            if (selectedBeatmapIndex >= packageBeatmaps.Count)
            {
                selectedBeatmapIndex = packageBeatmaps.Count - 1;
            }

            var selected = packageBeatmaps[selectedBeatmapIndex];
            string selectedName = selected.Name;
            int selectedNameIndex = 0;
            int selectedDifficultyIndex = 0;

            List<string> uniqueNames = new List<string>();
            List<string> selectedMapDifficulties = new List<string>();

            for (int i = 0; i < packageBeatmaps.Count; ++i)
            {
                var packageBeatmap = packageBeatmaps[i];
                string name = packageBeatmap.Name;
                bool isSelected = i == selectedBeatmapIndex;
                bool isSelectedName = (name == selectedName);
                if (!uniqueNames.Contains(name))
                {
                    if (isSelectedName)
                        selectedNameIndex = uniqueNames.Count;
                    uniqueNames.Add(name);
                }
                // We have this map name selected, add it to our difficulty selection.
                if (isSelectedName)
                {
                    if (isSelected)
                        selectedDifficultyIndex = selectedMapDifficulties.Count;
                    string difficulty = packageBeatmap.Difficulty;
                    selectedMapDifficulties.Add(difficulty);
                }
            }

            // Map picker

            if (uniqueNames.Count > 1)
            {
                GUILayout.BeginHorizontal();
                int newMapSelect = Toolbar.Render(selectedNameIndex, uniqueNames.ToArray());
                GUILayout.EndHorizontal();
                if (newMapSelect != -1 && newMapSelect != selectedNameIndex)
                {
                    // We have a new map name, find the first one with this name.
                    string newSelectionName = uniqueNames[newMapSelect];
                    for (int i = 0; i < packageBeatmaps.Count; ++i)
                    {
                        var packageBeatmap = packageBeatmaps[i];
                        if (packageBeatmap.Name == newSelectionName)
                        {
                            onBeatmapSelect(i);
                            break;
                        }
                    }
                    // Should never get here
                }
            }

            // Difficulty picker

            if (selectedMapDifficulties.Count > 1)
            {
                var bStyle = GUI.skin.button;
                int bFontSize = (int)(bStyle.fontSize * 1.5);
                GUILayout.BeginHorizontal();
                int newDifficultySelect = Toolbar.Render(selectedDifficultyIndex, selectedMapDifficulties.Select(s => $"<size={bFontSize}>{s}</size>").ToArray());
                GUILayout.EndHorizontal();
                if (newDifficultySelect != -1 && newDifficultySelect != selectedDifficultyIndex)
                {
                    // We have a new map name, find the first one with this name and difficulty.
                    string newDifficultyName = selectedMapDifficulties[newDifficultySelect];
                    for (int i = 0; i < packageBeatmaps.Count; ++i)
                    {
                        var packageBeatmap = packageBeatmaps[i];
                        if (packageBeatmap.Name == selectedName && packageBeatmap.Difficulty == newDifficultyName)
                        {
                            onBeatmapSelect(i);
                            break;
                        }
                    }
                    // Should never get here
                }
            }
        }
    }
}