using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CustomBeatmaps.CustomPackages;
using CustomBeatmaps.Patches;
using CustomBeatmaps.UI.Highscore;
using CustomBeatmaps.UI.PackageList;
using CustomBeatmaps.UISystem;
using CustomBeatmaps.Util;
using HarmonyLib;
using UnityEngine;

namespace CustomBeatmaps.UI
{
    public static class OnlinePackageListUI
    {
        private static CustomServerPackageList _list;
        private static bool _loaded;
        private static string _failure;
        
        // To preserve across play sessions
        private static int _selectedPackageIndex;

        public static void Render(Action onRenderAboveList)
        {
            var (selectedPackageIndex, setSelectedPackageIndex) = Reacc.UseState(0);
            (int selectedBeatmapIndex, Action<int>setSelectedBeatmapIndex) =
                (_selectedPackageIndex, val => _selectedPackageIndex = val);
            var (sortMode, setSortMode) = Reacc.UseState(SortMode.New);

            // Load packages from server the first time we open this
            Reacc.UseEffect(() => ReloadPackageList(sortMode));
            
            // Change sort mode: sort our results.
            Reacc.UseEffect(() => SortResults(sortMode), new object[]{sortMode});

            onRenderAboveList();

            if (_failure != null)
            {
                RenderReloadHeader($"Failed to grab packages from server: {_failure}");
                return;
            }

            if (!_loaded)
            {
                RenderReloadHeader("Loading...");
                return;
            }

            if (_list.Packages.Length == 0)
            {
                RenderReloadHeader("No packages found!");
                return;
            }

            // Map server headers -> UI headers

            List<PackageHeader> headers = new List<PackageHeader>(_list.Packages.Length);
            foreach (var serverPackage in _list.Packages)
            {
                // Get unique song count
                HashSet<string> songs = new HashSet<string>();
                string creator = "";
                string name = "";
                foreach (var bmap in serverPackage.Beatmaps.Values)
                {
                    songs.Add(bmap.AudioFileName);
                    creator += bmap.Creator + ", ";
                    name += bmap.Name + " | ";
                }

                if (serverPackage.Beatmaps.Count >= 1)
                {
                    // Trim
                    name = name.Substring(0, name.Length - " | ".Length);
                    creator = creator.Substring(0, creator.Length - ", ".Length);
                }

                string serverUrl = serverPackage.ServerURL;
                var downloadStatus = CustomBeatmaps.Downloader.GetDownloadStatus(serverUrl);
                bool isNew = !CustomBeatmaps.PlayedPackageManager.HasPlayed(
                    CustomPackageHelper.GetLocalFolderFromServerPackageURL(Config.Mod.ServerPackagesDir, serverUrl));
                headers.Add(new PackageHeader(name, songs.Count, serverPackage.Beatmaps.Count, creator, isNew, downloadStatus));
            }
            // Beatmaps
            var selectedPackage = _list.Packages[selectedPackageIndex];
            var selectedServerBeatmapKVPairs = selectedPackage.Beatmaps.ToArray();
            // Jank overflow fix between packages with varying beatmap sizes
            if (selectedBeatmapIndex >= selectedPackage.Beatmaps.Count)
            {
                selectedBeatmapIndex = selectedPackage.Beatmaps.Count - 1;
                setSelectedBeatmapIndex(selectedBeatmapIndex);
            }
            List<BeatmapHeader> selectedBeatmaps = new List<BeatmapHeader>(selectedPackage.Beatmaps.Count);
            foreach (var bmapKVPair in selectedServerBeatmapKVPairs)
            {
                var bmap = bmapKVPair.Value;
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
                    RenderReloadHeader($"Got {_list.Packages.Length} Packages", () =>
                    {
                        SortModePickerUI.Render(sortMode, setSortMode);
                    }, sortMode);
                    PackageListUI.Render($"Server Packages", headers, selectedPackageIndex, setSelectedPackageIndex);
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
                        if (selectedPackage.Beatmaps.Count != 0)
                        {
                            // LOCAL high score
                            var downloadStatus = CustomBeatmaps.Downloader.GetDownloadStatus(selectedPackage.ServerURL);
                            var selectedBeatmapKeyPath = selectedServerBeatmapKVPairs[selectedBeatmapIndex].Key;
                            if (downloadStatus == BeatmapDownloadStatus.Downloaded)
                            {
                                var localPackages = CustomBeatmaps.LocalServerPackages;
                                var (_, customBeatmapInfo) = localPackages.FindCustomBeatmapInfoFromServer(selectedPackage.ServerURL, selectedBeatmapKeyPath);
                                PersonalHighScoreUI.Render(customBeatmapInfo.OsuPath);
                            }
                            // SERVER high scores
                            HighScoreListUI.Render(UserServerHelper.GetHighScoreBeatmapKeyFromServerBeatmap(selectedPackage.ServerURL, selectedBeatmapKeyPath));
                        }
                    },
                    () =>
                    {
                        if (selectedPackage.Beatmaps.Count == 0)
                        {
                            GUILayout.Label("No beatmaps found...");
                        }
                        else
                        {
                            var downloadStatus = CustomBeatmaps.Downloader.GetDownloadStatus(selectedPackage.ServerURL);

                            var selectedBeatmap = selectedServerBeatmapKVPairs[selectedBeatmapIndex].Value;
                            var selectedBeatmapKeyPath = selectedServerBeatmapKVPairs[selectedBeatmapIndex].Key;

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

                            if (UnbeatableHelper.UsingHighScoreProhibitedAssists())
                            {
                                GUILayout.Label("<b>USING ASSISTS</b> (no high score)");
                            }
                            
                            bool buttonPressed = PlayButtonUI.Render(buttonText, buttonSub);
                            switch (downloadStatus)
                            {
                                case BeatmapDownloadStatus.Downloaded:
                                    var localPackages = CustomBeatmaps.LocalServerPackages;
                                    // Play a local beatmap
                                    var (localPackage, customBeatmapInfo) = localPackages.FindCustomBeatmapInfoFromServer(selectedPackage.ServerURL, selectedBeatmapKeyPath);
                                    // Preview, cause we can!
                                    WhiteLabelMainMenuPatch.PlaySongPreview(customBeatmapInfo.RealAudioKey);
                                    if (buttonPressed)
                                    {
                                        UnbeatableHelper.PlayBeatmap(customBeatmapInfo, true);
                                        CustomBeatmaps.PlayedPackageManager.RegisterPlay(localPackage.FolderName);
                                    }
                                    break;
                                case BeatmapDownloadStatus.NotDownloaded:
                                    WhiteLabelMainMenuPatch.StopSongPreview();
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

        private static void RenderReloadHeader(string label, Action renderHeaderSortPicker = null, SortMode sortMode = SortMode.New) // ok this part is jank but that's all I need
        {
            GUILayout.BeginHorizontal();
                if (GUILayout.Button("Reload", GUILayout.ExpandWidth(false)))
                {
                    ReloadPackageList(sortMode);
                }
                
                renderHeaderSortPicker?.Invoke();
                
                GUILayout.Label(label, GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();
            GUILayout.Space(20);
        }

        private static void SortResults(SortMode sortMode)
        {
            if (_list.Packages == null) // forgor guard
                return;

            var l = _list.Packages.ToList();
            UIConversionHelper.SortServerPackages(l, sortMode);
            _list.Packages = l.ToArray();
        }
        
        private static void ReloadPackageList(SortMode sortMode)
        {
            ScheduleHelper.SafeLog("RELOADING Packages from Server...");
            // Also reload high scores because... yeah
            CustomBeatmaps.ServerHighScoreManager.Reload();
            _failure = "loading...";
            CustomPackageHelper.FetchServerPackageList(CustomBeatmaps.BackendConfig.ServerPackageList).ContinueWith(result =>
            {
                if (result.Exception != null)
                {
                    foreach (var ex in result.Exception.InnerExceptions)
                        _failure += ex.Message + " ";
                    EventBus.ExceptionThrown?.Invoke(result.Exception);
                    return;
                }
                _failure = null;
                _loaded = false; // Impromptu mutex :P
                _list = result.Result;
                SortResults(sortMode);
                _loaded = true;
            });
        }
    }
}