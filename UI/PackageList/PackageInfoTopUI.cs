using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustomBeatmaps.UI.PackageList
{
    public static class PackageInfoTopUI
    {
        public static void Render(List<BeatmapHeader> packageBeatmaps, int selectedBeatmapIndex)
        {
            // If No beatmaps...

            if (packageBeatmaps.Count == 0)
            {
                GUILayout.Label("No beatmaps provided!");
                return;
            }

            // Construct list of unique map names and our currently selected map's difficulties

            if (selectedBeatmapIndex >= packageBeatmaps.Count)
            {
                selectedBeatmapIndex = packageBeatmaps.Count - 1;
            }

            var selected = packageBeatmaps[selectedBeatmapIndex];

            // Beatmap Info Card
            BeatmapInfoCardUI.Render(selected);

            // Leaderboards and the "PLAY/DOWNLOAD" button are rendered separately
        }
    }
}