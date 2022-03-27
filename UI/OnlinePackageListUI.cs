using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Timers;
using CustomBeatmaps.CustomPackages;
using CustomBeatmaps.Patches;
using CustomBeatmaps.UI.PackageList;
using CustomBeatmaps.UISystem;
using CustomBeatmaps.Util;
using UnityEngine;

namespace CustomBeatmaps.UI
{
    public static class OnlinePackageListUI
    {
        private static CustomServerPackageList _list;
        private static bool _loaded;
        private static string _failure;

        public static void Render(Action onRenderAboveList)
        {
            var (selectedPackageIndex, setSelectedPackageIndex) = Reacc.UseState(0);
            var (selectedBeatmapIndex, setSelectedBeatmapIndex) = Reacc.UseState(0);

            // Load packages from server the first time we open this
            Reacc.UseEffect(ReloadPackageList);

            onRenderAboveList();

            if (_failure != null)
            {
                RenderReloadButton($"Failed to grab packages from server: {_failure}");
                return;
            }

            if (!_loaded)
            {
                RenderReloadButton("Loading...");
                return;
            }

            if (_list.Packages.Length == 0)
            {
                RenderReloadButton("No packages found!");
                return;
            }

            // Map server headers -> UI headers

            List<PackageHeader> headers = new List<PackageHeader>(_list.Packages.Length);
            foreach (var serverPackage in _list.Packages)
            {
                // Get unique song count
                HashSet<string> songs = new HashSet<string>();
                string creator = "";
                foreach (var bmap in serverPackage.Beatmaps)
                {
                    songs.Add(bmap.AudioFileName);
                    creator += bmap.Creator + ", ";
                }

                if (creator.EndsWith(", "))
                    creator = creator.Substring(0, creator.Length - ", ".Length);

                string serverUrl = serverPackage.ServerURL;
                string name = Path.GetFileName(serverUrl);
                var downloadStatus = CustomBeatmaps.Downloader.GetDownloadStatus(serverUrl);
                headers.Add(new PackageHeader(name, songs.Count, serverPackage.Beatmaps.Length, creator, true, downloadStatus));
            }
            // Beatmaps
            var selectedPackage = _list.Packages[selectedPackageIndex];
            // Jank overflow fix between packages with varying beatmap sizes
            if (selectedBeatmapIndex >= selectedPackage.Beatmaps.Length)
            {
                selectedBeatmapIndex = selectedPackage.Beatmaps.Length - 1;
                setSelectedBeatmapIndex(selectedBeatmapIndex);
            }
            List<BeatmapHeader> selectedBeatmaps = new List<BeatmapHeader>(selectedPackage.Beatmaps.Length);
            foreach (var bmap in selectedPackage.Beatmaps)
            {
                selectedBeatmaps.Add(new BeatmapHeader(
                    bmap.Name,
                    bmap.Artist,
                    bmap.Creator,
                    bmap.Difficulty,
                    null
                ));
            }

            GUILayout.BeginHorizontal();
                // Render list
                GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                    RenderReloadButton($"Got {_list.Packages.Length} Packages");
                    PackageListUI.Render($"Server Packages", headers, selectedPackageIndex, setSelectedPackageIndex);
                GUILayout.EndVertical();

                // Render Right Info
                PackageInfoUI.Render(
                    () =>
                    {
                        PackageInfoTopUI.Render(selectedBeatmaps, selectedBeatmapIndex, setSelectedBeatmapIndex);
                    },
                    () =>
                    {
                        if (selectedPackage.Beatmaps.Length != 0)
                        {
                            // LOCAL high score
                            var downloadStatus = CustomBeatmaps.Downloader.GetDownloadStatus(selectedPackage.ServerURL);
                            if (downloadStatus == BeatmapDownloadStatus.Downloaded)
                            {
                                var selectedBeatmap = selectedPackage.Beatmaps[selectedBeatmapIndex];
                                var localPackages = CustomBeatmaps.LocalServerPackages;
                                var customBeatmapInfo = localPackages.FindCustomBeatmapInfoFromServer(selectedPackage.ServerURL, selectedBeatmap);
                                PersonalHighScoreUI.Render(customBeatmapInfo.OsuPath);
                            }
                            // SERVER high scores
                            GUILayout.Label("SERVER High Scores go here");
                        }
                    },
                    () =>
                    {
                        if (selectedPackage.Beatmaps.Length == 0)
                        {
                            GUILayout.Label("No beatmaps found...");
                        }
                        else
                        {
                            var downloadStatus = CustomBeatmaps.Downloader.GetDownloadStatus(selectedPackage.ServerURL);

                            var selectedBeatmap = selectedPackage.Beatmaps[selectedBeatmapIndex];

                            string buttonText = "??";
                            string buttonSub = "";
                            switch (downloadStatus)
                            {
                                case BeatmapDownloadStatus.Downloaded:
                                    buttonText = "PLAY";
                                    buttonSub = $"{selectedBeatmap.Name}: {selectedBeatmap.Difficulty}";
                                    break;
                                case BeatmapDownloadStatus.CurrentlyDownloading:
                                    buttonText = "Downloading...";
                                    break;
                                case BeatmapDownloadStatus.Queued:
                                    buttonText = "Queued for download...";
                                    break;
                                case BeatmapDownloadStatus.NotDownloaded:
                                    buttonText = "DOWNLOAD";
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            bool buttonPressed = PlayButtonUI.Render(buttonText, buttonSub);
                            switch (downloadStatus)
                            {
                                case BeatmapDownloadStatus.Downloaded:
                                    var localPackages = CustomBeatmaps.LocalServerPackages;
                                    // Play a local beatmap
                                    var customBeatmapInfo = localPackages.FindCustomBeatmapInfoFromServer(selectedPackage.ServerURL, selectedBeatmap);
                                    // Preview, cause we can!
                                    WhiteLabelMainMenuPatch.PlaySongPreview(customBeatmapInfo.RealAudioKey);
                                    if (buttonPressed)
                                    {
                                        UnbeatableHelper.PlayBeatmap(customBeatmapInfo, selectedPackage.ServerURL);
                                    }
                                    break;
                                case BeatmapDownloadStatus.NotDownloaded:
                                    if (buttonPressed)
                                    {
                                        CustomBeatmaps.Downloader.QueueDownloadPackage(selectedPackage.ServerURL);
                                    }
                                    break;
                            }

                        }
                    }
                );
            GUILayout.EndHorizontal();
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

        private static void ReloadPackageList()
        {
            ScheduleHelper.SafeLog("RELOADING Packages from Server...");
            CustomPackageHelper.FetchServerPackageList(CustomBeatmaps.BackendConfig.ServerPackageList).ContinueWith(result =>
            {
                if (result.Exception != null)
                {
                    _failure = "";
                    foreach (var ex in result.Exception.InnerExceptions)
                        _failure += ex.Message + " ";
                    EventBus.ExceptionThrown?.Invoke(result.Exception);
                    return;
                }
                _failure = null;
                _loaded = false; // Impromptu mutex :P
                _list = result.Result;
                _loaded = true;
            });
        }
    }
}