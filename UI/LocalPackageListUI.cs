using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CustomBeatmaps.CustomPackages;
using CustomBeatmaps.Patches;
using CustomBeatmaps.UI.PackageList;
using CustomBeatmaps.UISystem;
using CustomBeatmaps.Util;
using HarmonyLib;
using UnityEngine;

namespace CustomBeatmaps.UI
{
    public static class LocalPackageListUI
    {
        public static void Render(Action onRenderAboveList)
        {
            var (selectedPackageIndex, setSelectedPackageIndex) = Reacc.UseState(0);
            var (selectedBeatmapIndex, setSelectedBeatmapIndex) = Reacc.UseState(0);

            var (sortMode, setSortMode) = Reacc.UseState(SortMode.New);

            var localPackages = CustomBeatmaps.LocalUserPackages.Packages;

            // This is... kinda highly inefficient but whatever?
            UIConversionHelper.SortLocalPackages(localPackages, sortMode);

            // No packages?

            if (localPackages.Count == 0)
            {
                onRenderAboveList();
                GUILayout.Label($"No Local Packages Found in {Config.Mod.UserPackagesDir}");
                return;
            }

            // Clamp packages to fit in the event of package list changing while the UI is open
            if (selectedPackageIndex > localPackages.Count)
            {
                selectedPackageIndex = localPackages.Count - 1;
            }

            // Map local packages -> package header

            var selectedPackage = localPackages[selectedPackageIndex];

            List<PackageHeader> headers = new List<PackageHeader>(localPackages.Count);
            foreach (var p in localPackages)
            {
                // Get unique song count
                HashSet<string> songs = new HashSet<string>();
                foreach (var bmap in p.Beatmaps)
                {
                    songs.Add(bmap.RealAudioKey);
                }

                string name = p.Beatmaps.Length == 1 ? p.Beatmaps[0].SongName : Path.GetFileName(p.FolderName);
                string creator = p.Beatmaps.Join(binfo => binfo.BeatmapCreator, ",");
                bool isNew = !CustomBeatmaps.PlayedPackageManager.HasPlayed(p.FolderName);
                headers.Add(new PackageHeader(name, songs.Count, p.Beatmaps.Length, creator, isNew, BeatmapDownloadStatus.Downloaded));
            }

            // Beatmaps of selected package

            List<BeatmapHeader> selectedBeatmaps =
                UIConversionHelper.CustomBeatmapInfosToBeatmapHeaders(selectedPackage.Beatmaps.ToList());

            var selectedBeatmap = selectedPackage.Beatmaps[selectedBeatmapIndex];

            // Preview audio
            WhiteLabelMainMenuPatch.PlaySongPreview(selectedBeatmap.RealAudioKey);

            // Render
            onRenderAboveList();

            GUILayout.BeginHorizontal();
                // Render list
                GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                    SortModePickerUI.Render(sortMode, setSortMode);
                    PackageListUI.Render($"Local Packages in {Config.Mod.UserPackagesDir}", headers, selectedPackageIndex, setSelectedPackageIndex);
                    AssistAreaUI.Render();
                GUILayout.EndVertical();

                // Render Right Info
                PackageInfoUI.Render(
                    () =>
                    {
                        PackageInfoTopUI.Render(selectedBeatmaps, selectedBeatmapIndex, setSelectedBeatmapIndex);
                    },
                    () =>
                    {
                        PersonalHighScoreUI.Render(selectedBeatmap.OsuPath);
                    },
                    () =>
                    {
                        if (PlayButtonUI.Render("PLAY", $"{selectedBeatmap.SongName}: {selectedBeatmap.Difficulty}"))
                        {
                            // Play a local beatmap
                            var package = localPackages[selectedPackageIndex];
                            var customBeatmapInfo = package.Beatmaps[selectedBeatmapIndex];
                            UnbeatableHelper.PlayBeatmap(customBeatmapInfo, false);
                            CustomBeatmaps.PlayedPackageManager.RegisterPlay(package.FolderName);
                        }
                    }
                );
            GUILayout.EndHorizontal();
        }
    }
}
