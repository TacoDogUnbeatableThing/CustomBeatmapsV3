﻿using System;
using System.IO;
using BepInEx;
using CustomBeatmaps.Patches;
using CustomBeatmaps.UI.PackageList;
using CustomBeatmaps.UISystem;
using CustomBeatmaps.Util;
using UnityEngine;

namespace CustomBeatmaps.UI
{
    public static class OSUPackageListUI
    {
        public static void Render(Action onRenderAboveList)
        {
            var (selectedBeatmapIndex, setSelectedBeatmapIndex) = Reacc.UseState(0);
            var (scroll, setScroll) = Reacc.UseState(Vector2.zero);

            var osuBeatmaps = CustomBeatmaps.OSUBeatmapManager.OsuBeatmaps;

            onRenderAboveList();

            if (CustomBeatmaps.OSUBeatmapManager.Error != null)
            {
                GUILayout.Label(CustomBeatmaps.OSUBeatmapManager.Error);
                WhiteLabelMainMenuPatch.StopSongPreview();
                return;
            }
            
            if (osuBeatmaps.Length == 0)
            {
                GUILayout.Label("No OSU beatmaps found!");
                WhiteLabelMainMenuPatch.StopSongPreview();
                return;
            }

            // Out of bounds net
            if (selectedBeatmapIndex >= osuBeatmaps.Length)
            {
                selectedBeatmapIndex = osuBeatmaps.Length - 1;
                setSelectedBeatmapIndex(selectedBeatmapIndex);
            }

            GUILayout.BeginHorizontal();

            setScroll(GUILayout.BeginScrollView(scroll));
            for (int i = 0; i < osuBeatmaps.Length; ++i)
            {
                var osuBmap = osuBeatmaps[i];
                string name = Path.GetFileName(osuBmap.OsuPath);
                if (GUILayout.Button(name))
                {
                    setSelectedBeatmapIndex(i);
                }
            }
            GUILayout.EndScrollView();

            var selectedBeatmap = osuBeatmaps[selectedBeatmapIndex];
            WhiteLabelMainMenuPatch.PlaySongPreview(selectedBeatmap.RealAudioKey);

            PackageInfoUI.Render(
                () =>
                {
                    BeatmapInfoCardUI.Render(UIConversionHelper.CustomBeatmapInfoToBeatmapHeader(selectedBeatmap));
                },
                () => { },
                () =>
                {
                    if (GUILayout.Button($"EXPORT"))
                    {
                        string exportFolder = Config.Mod.OsuExportDirectory;
                        string exportName = selectedBeatmap.SongName + ".zip";
                        //OSUHelper.CreateExportZipFile(selectedBeatmap.OsuPath, Path.Join(exportFolder, exportName));
                    }
                    if (PlayButtonUI.Render($"EDIT: {selectedBeatmap.SongName}"))
                    {
                        UnbeatableHelper.PlayBeatmapEdit(selectedBeatmap);
                    }
                }
            );

            GUILayout.EndHorizontal();
        }
    }
}