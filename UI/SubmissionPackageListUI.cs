using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CustomBeatmaps.Patches;
using CustomBeatmaps.UI.PackageList;
using CustomBeatmaps.UISystem;
using CustomBeatmaps.Util;
using HarmonyLib;
using UnityEngine;

namespace CustomBeatmaps.UI
{
    // TODO: Big code duplication with OnlinePackageListUI

    public static class SubmissionPackageListUI
    {

        public static void Render(Action onRenderAboveList)
        {
            var (scroll, setScroll) = Reacc.UseState(Vector2.zero);
            var (selectedBeatmapIndex, setSelectedBeatmapIndex) = Reacc.UseState(0);

            Reacc.UseEffect(ReloadPackageList);

            onRenderAboveList();

            var p = CustomBeatmaps.SubmissionPackageManager;
            
            if (p.ListLoadFailures.Count != 0)
            {
                RenderReloadButton($"Failed to grab packages from server: {p.ListLoadFailures.Join()}");
                return;
            }

            if (!p.ListLoaded)
            {
                RenderReloadButton("Loading...");
                return;
            }

            if (p.SubmissionPackages.Count == 0)
            {
                RenderReloadButton("No packages found!");
                return;
            }

            RenderReloadButton($"Found {p.SubmissionPackages.Count} submissions");

            GUILayout.BeginHorizontal();

            setScroll(GUILayout.BeginScrollView(scroll, GUILayout.ExpandWidth(true)));
            foreach (var serverSubmissionPackage in p.SubmissionPackages)
            {
                GUILayout.BeginHorizontal(GUI.skin.box);
                string downloadName = Path.GetFileName(serverSubmissionPackage.DownloadURL);
                GUILayout.Label($"{serverSubmissionPackage.Username}: {downloadName}", GUILayout.ExpandWidth(false));
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("DOWNLOAD", GUILayout.ExpandWidth(false)))
                {
                    CustomBeatmaps.SubmissionPackageManager.DownloadSubmission(serverSubmissionPackage.DownloadURL);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();

            bool hasPackage = CustomBeatmaps.SubmissionPackageManager.LocalPackageDownloaded;
            var package = CustomBeatmaps.SubmissionPackageManager.LocalPackage;

            var beatmaps = hasPackage? UIConversionHelper.CustomBeatmapInfosToBeatmapHeaders(package.Beatmaps.ToList()) : new List<BeatmapHeader>();

            PackageInfoUI.Render(
                () =>
                {
                    if (hasPackage)
                    {
                        PackageInfoTopUI.Render(beatmaps, selectedBeatmapIndex);
                    }
                    else
                    {
                        GUILayout.Label("Download one of the submission packages and test them here!");
                    }
                },
                () => { },
                () =>
                {
                    if (hasPackage)
                    {
                        if (package.Beatmaps.Length > 0)
                        {
                            if (selectedBeatmapIndex >= package.Beatmaps.Length)
                                selectedBeatmapIndex = package.Beatmaps.Length - 1;
                            var selectedBeatmap = package.Beatmaps[selectedBeatmapIndex];
                            
                            WhiteLabelMainMenuPatch.PlaySongPreview(selectedBeatmap.RealAudioKey);

                            PackageBeatmapPickerUI.Render(beatmaps, selectedBeatmapIndex, setSelectedBeatmapIndex);

                            if (PlayButtonUI.Render("PLAY",
                                    $"{selectedBeatmap.SongName}: {selectedBeatmap.Difficulty}"))
                            {
                                // Play a local beatmap
                                var customBeatmapInfo = package.Beatmaps[selectedBeatmapIndex];
                                UnbeatableHelper.PlayBeatmap(customBeatmapInfo, false, UnbeatableHelper.GetSceneNameByIndex(CustomBeatmaps.Memory.SelectedRoom));
                            }
                        }
                        else
                        {
                            WhiteLabelMainMenuPatch.StopSongPreview();
                            GUILayout.Label("No beatmaps found!");
                        }
                    }
                    else
                    {
                        WhiteLabelMainMenuPatch.StopSongPreview();
                    }
                }
            );

            GUILayout.EndHorizontal();
        }

        private static void ReloadPackageList()
        {
            ScheduleHelper.SafeLog("RELOADING Submissions from Server...");
            CustomBeatmaps.SubmissionPackageManager.RefreshServerSubmissions();
        }
        
        private static void RenderReloadButton(string label)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Reload", GUILayout.ExpandWidth(false)))
            {
                ReloadPackageList();
            }
            GUILayout.Label(label, GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();
            GUILayout.Space(20);
        }
    }
}
