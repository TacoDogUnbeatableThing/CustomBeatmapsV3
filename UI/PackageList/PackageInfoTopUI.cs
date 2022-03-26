using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustomBeatmaps.UI.PackageList
{
    public static class PackageInfoTopUI
    {
        public static void Render(List<BeatmapHeader> packageBeatmaps, int selectedBeatmapIndex, Action<int> onBeatmapSelect)
        {
            // If No beatmaps...

            if (packageBeatmaps.Count == 0)
            {
                GUILayout.Label("No beatmaps provided!");
                return;
            }

            // Construct list of unique map names and our currently selected map's difficulties

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

            // TODO: If there is only one, ignore.

            int newMapSelect = GUILayout.Toolbar(selectedNameIndex, uniqueNames.ToArray());
            if (newMapSelect != selectedNameIndex)
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

            // Beatmap Info Card
            BeatmapInfoCardUI.Render(selected);

            // Difficulty picker

            int newDifficultySelect = GUILayout.Toolbar(selectedDifficultyIndex, selectedMapDifficulties.ToArray());
            if (newDifficultySelect != selectedDifficultyIndex)
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

            // Leaderboards and the "PLAY/DOWNLOAD" button are rendered separately
        }
    }
}