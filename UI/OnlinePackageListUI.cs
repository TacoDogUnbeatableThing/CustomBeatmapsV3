using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
        private static List<PackageHeader> _headers;
        private static bool _loaded;
        private static string _failure;

        // To preserve across play sessions
        private static int _selectedHeaderIndex;
        private static string _searchQuery;
        private static int _selectedBeatmapIndex;

        static OnlinePackageListUI()
        {
            // Regen headers after updating a server package
            CustomBeatmaps.LocalServerPackages.PackageUpdated += package =>
            {
                RegenerateHeaders(true);
            };
        }
        
        public static void Render(Action onRenderAboveList)
        {
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

            if (_list.Packages.Length == 0 || _headers == null)
            {
                RenderReloadHeader("No packages found!");
                return;
            }

            // Map server headers -> UI headers

            // Beatmaps
            int selectedPackageIndex = _headers[_selectedHeaderIndex].PackageIndex; 
            var selectedPackage = _list.Packages[selectedPackageIndex];
            var selectedServerBeatmapKVPairs = selectedPackage.Beatmaps.ToArray();
            // Jank overflow fix between packages with varying beatmap sizes
            if (_selectedBeatmapIndex >= selectedPackage.Beatmaps.Count)
            {
                _selectedBeatmapIndex = selectedPackage.Beatmaps.Count - 1;
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
                    string searchTextInput = GUILayout.TextArea(_searchQuery);
                    if (searchTextInput != _searchQuery)
                    {
                        _searchQuery = searchTextInput;
                        RegenerateHeaders(false);
                    }
                    PackageListUI.Render($"Server Packages", _headers, _selectedHeaderIndex, newVal => _selectedHeaderIndex = newVal);
                    AssistAreaUI.Render();
                GUILayout.EndVertical();

                // Render Right Info
                PackageInfoUI.Render(
                    () =>
                    {
                        PackageInfoTopUI.Render(selectedBeatmaps, _selectedBeatmapIndex);
                    },
                    () =>
                    {
                        if (selectedPackage.Beatmaps.Count != 0)
                        {
                            // LOCAL high score
                            var downloadStatus = CustomBeatmaps.Downloader.GetDownloadStatus(selectedPackage.ServerURL);
                            var selectedBeatmapKeyPath = selectedServerBeatmapKVPairs[_selectedBeatmapIndex].Key;
                            if (downloadStatus == BeatmapDownloadStatus.Downloaded)
                            {
                                var localPackages = CustomBeatmaps.LocalServerPackages;
                                try
                                {
                                    var (_, selectedBeatmap) =
                                        localPackages.FindCustomBeatmapInfoFromServer(selectedPackage.ServerURL,
                                            selectedBeatmapKeyPath);
                                    string highScoreKey =
                                        UserServerHelper.GetHighScoreLocalEntryFromCustomBeatmap(
                                            Config.Mod.ServerPackagesDir, Config.Mod.UserPackagesDir,
                                            selectedBeatmap.OsuPath);
                                    PersonalHighScoreUI.Render(highScoreKey);
                                }
                                catch (Exception e)
                                {
                                    Debug.LogWarning("Invalid package found: (ignoring)");
                                    Debug.LogException(e);
                                }
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

                            var selectedBeatmap = selectedServerBeatmapKVPairs[_selectedBeatmapIndex].Value;
                            var selectedBeatmapKeyPath = selectedServerBeatmapKVPairs[_selectedBeatmapIndex].Key;

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

                            PackageBeatmapPickerUI.Render(selectedBeatmaps, _selectedBeatmapIndex, newVal => _selectedBeatmapIndex = newVal);

                            if (UnbeatableHelper.UsingHighScoreProhibitedAssists())
                            {
                                GUILayout.Label("<size=24><b>USING ASSISTS</b></size> (no high score)");
                            } else if (!CustomBeatmaps.UserSession.LoggedIn)
                            {
                                GUILayout.Label("<b>Register above to post your own high scores!<b>");
                            }

                            bool buttonPressed = PlayButtonUI.Render(buttonText, buttonSub);
                            switch (downloadStatus)
                            {
                                case BeatmapDownloadStatus.Downloaded:
                                    try
                                    {
                                        var localPackages = CustomBeatmaps.LocalServerPackages;
                                        // Play a local beatmap
                                        var (localPackage, customBeatmapInfo) =
                                            localPackages.FindCustomBeatmapInfoFromServer(selectedPackage.ServerURL,
                                                selectedBeatmapKeyPath);
                                        // Preview, cause we can!
                                        if (customBeatmapInfo != null)
                                            WhiteLabelMainMenuPatch.PlaySongPreview(customBeatmapInfo.RealAudioKey);
                                        if (buttonPressed)
                                        {
                                            UnbeatableHelper.PlayBeatmap(customBeatmapInfo, true,
                                                UnbeatableHelper.GetSceneNameByIndex(CustomBeatmaps.Memory
                                                    .SelectedRoom));
                                            CustomBeatmaps.PlayedPackageManager.RegisterPlay(localPackage.FolderName);
                                        }
                                    }
                                    catch (InvalidOperationException)
                                    {
                                        if (PlayButtonUI.Render("INVALID PACKAGE: Redownload"))
                                        {
                                            // Delete + redownload
                                            Directory.Delete(CustomPackageHelper.GetLocalFolderFromServerPackageURL(Config.Mod.ServerPackagesDir, selectedPackage.ServerURL), true);
                                            CustomBeatmaps.Downloader.QueueDownloadPackage(selectedPackage.ServerURL);
                                        }
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
                GUILayout.Label(label, GUILayout.ExpandWidth(false));

                renderHeaderSortPicker?.Invoke();
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
            RegenerateHeaders(false);
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

        private static bool _headerRegenerationQueued;
        private static void RegenerateHeaders(bool delayed)
        {
            if (_headerRegenerationQueued)
                return;
            _headerRegenerationQueued = true;
            Task.Run(async () =>
            {
                // Wait a bit for file system to uh do its thing
                if (delayed)
                    await Task.Delay(300);
                ScheduleHelper.SafeLog("    RELOADING HEADERS");
                _headers = new List<PackageHeader>(_list.Packages.Length);
                int packageIndex = -1;
                foreach (var serverPackage in _list.Packages)
                {
                    ++packageIndex;
                    // Get unique song count
                    HashSet<string> songs = new HashSet<string>();
                    HashSet<string> names = new HashSet<string>();
                    HashSet<string> creators = new HashSet<string>();
                    foreach (var bmap in serverPackage.Beatmaps.Values)
                    {
                        songs.Add(bmap.AudioFileName);
                        names.Add(bmap.Name);
                        creators.Add(bmap.Creator);
                    }

                    if (!UIConversionHelper.PackageMatchesFilter(serverPackage, _searchQuery))
                    {
                        continue;
                    }

                    string creator = creators.Join(x => x," | ");
                    string name = names.Join(x => x,", ");

                    string serverUrl = serverPackage.ServerURL;
                    var downloadStatus = CustomBeatmaps.Downloader.GetDownloadStatus(serverUrl);
                    bool isNew = !CustomBeatmaps.PlayedPackageManager.HasPlayed(
                        CustomPackageHelper.GetLocalFolderFromServerPackageURL(Config.Mod.ServerPackagesDir, serverUrl));
                    _headers.Add(new PackageHeader(name, songs.Count, serverPackage.Beatmaps.Count, creator, isNew, downloadStatus, packageIndex));
                }
                
                // Ensure we have SOMETHING visible
                if (_selectedHeaderIndex >= _headers.Count)
                {
                    _selectedHeaderIndex = 0;
                }

                _headerRegenerationQueued = false;
            });
        }
    }
}