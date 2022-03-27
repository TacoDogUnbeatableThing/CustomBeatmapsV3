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

            var localPackages = CustomBeatmaps.LocalUserPackages.Packages;

            // No packages?

            if (localPackages.Count == 0)
            {
                onRenderAboveList();
                GUILayout.Label($"No Local Packages Found in {Config.Mod.UserPackagesDir}");
                return;
            }

            // Clamp packages to fit in the event of package list changing while the UI is open
            if (selectedBeatmapIndex > localPackages.Count)
            {
                selectedBeatmapIndex = localPackages.Count - 1;
            }

            // Map local packages -> package header

            var selectedPackage = localPackages[selectedBeatmapIndex];

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
                headers.Add(new PackageHeader(name, songs.Count, p.Beatmaps.Length, creator, true, BeatmapDownloadStatus.Downloaded));
            }

            // Beatmaps of selected package

            List<BeatmapHeader> selectedBeatmaps =
                UIConversionHelper.CustomBeatmapInfosToBeatmapHeaders(selectedPackage.Beatmaps.ToList());

            var selectedBeatmap = selectedPackage.Beatmaps[selectedBeatmapIndex];

            WhiteLabelMainMenuPatch.PlaySongPreview(selectedBeatmap.RealAudioKey);

            // Render

            GUILayout.BeginHorizontal();
                // Render list
                GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                    onRenderAboveList();
                    PackageListUI.Render($"Local Packages in {Config.Mod.UserPackagesDir}", headers, selectedPackageIndex, setSelectedPackageIndex);
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
                            var customBeatmapInfo = localPackages[selectedPackageIndex].Beatmaps[selectedBeatmapIndex];
                            UnbeatableHelper.PlayBeatmap(customBeatmapInfo);
                        }
                    }
                );
            GUILayout.EndHorizontal();
        } 
    }
}
