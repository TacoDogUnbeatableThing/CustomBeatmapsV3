using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CustomBeatmaps.Util;

namespace CustomBeatmaps.CustomPackages
{
    /// <summary>
    /// Manages Local packages in the USER_PACKAGES or SERVER_PACKAGES directory
    /// </summary>
    public class LocalPackageManager
    {
        private readonly List<CustomLocalPackage> _packages = new List<CustomLocalPackage>();
        private readonly Action<BeatmapException> _onLoadException;

        private readonly Queue<string> _loadQueue = new Queue<string>();

        private string _folder;
        private FileSystemWatcher _watcher;

        public LocalPackageManager(Action<BeatmapException> onLoadException)
        {
            _onLoadException = onLoadException;
        }

        private void ReloadAll()
        {
            if (_folder == null)
                return;
            lock (_packages)
            {
                ScheduleHelper.SafeLog($"RELOADING ALL PACKAGES FROM {_folder}");

                _packages.Clear();
                _packages.AddRange(CustomPackageHelper.LoadLocalPackages(_folder, _onLoadException));
            }
        }

        private void UpdatePackage(string folderPath)
        {
            ScheduleHelper.SafeLog($"UPDATING PACKAGE: {folderPath}");
            if (CustomPackageHelper.TryLoadLocalPackage(folderPath, _folder, out CustomLocalPackage package, true,
                    _onLoadException))
            {
                lock (_packages)
                {
                    // Remove old package if there was one and update
                    int toRemove = _packages.FindIndex(check => check.FolderName == package.FolderName);
                    if (toRemove != -1)
                        _packages.RemoveAt(toRemove);
                    _packages.Add(package);
                }
            }
            else
            {
                ScheduleHelper.SafeLog("    cannot find package!!!");
            }
        }

        public List<CustomLocalPackage> Packages {
            get
            {
                lock (_packages)
                {
                    return _packages;
                }
            }
        }

        public bool PackageExists(string folder)
        {
            string targetFullPath = Path.GetFullPath(folder);
            foreach (var package in Packages)
            {
                if (Path.GetFullPath(package.FolderName) == targetFullPath)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Given a server package URL and a beatmap info from the server, find our local beatmap.
        /// </summary>
        public (CustomLocalPackage, CustomBeatmapInfo) FindCustomBeatmapInfoFromServer(string serverPackageURL, string beatmapRelativeKeyPath)
        {
            beatmapRelativeKeyPath = beatmapRelativeKeyPath.Replace('/', '\\');

            string targetPackageFullPath = CustomPackageHelper.GetLocalFolderFromServerPackageURL(
                Config.Mod.ServerPackagesDir, serverPackageURL);
            targetPackageFullPath = Path.GetFullPath(targetPackageFullPath);

            bool foundPackage = false;
            foreach (var package in Packages)
            {
                string currentFullPath = Path.GetFullPath(package.FolderName); 
                bool samePackage = currentFullPath == targetPackageFullPath;
                //Debug.Log($"{currentFullPath} compared to {targetFullPath}");
                if (samePackage)
                {
                    foundPackage = true;
                    foreach (var cbinfo in package.Beatmaps)
                    {
                        string fullOSUPath = Path.GetFullPath(cbinfo.OsuPath);
                        string relativeOSUPath = fullOSUPath.Substring(targetPackageFullPath.Length + 1);
                        if (beatmapRelativeKeyPath == relativeOSUPath)
                        {
                            return (package, cbinfo);
                        }
                    }
                }
            }

            if (!foundPackage)
            {
                throw new InvalidOperationException($"Can't find package at {targetPackageFullPath}");
            }
            throw new InvalidOperationException(
                $"Can't find beatmap {beatmapRelativeKeyPath} at folder {targetPackageFullPath}");
        }

        public void SetFolder(string folder)
        {
            folder = Path.GetFullPath(folder);
            if (folder == _folder)
                return;

            _folder = folder;

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            // Clear previous watcher
            if (_watcher != null)
            {
                _watcher.Dispose();
            }

            // Watch for changes
            _watcher = FileWatchHelper.WatchFolder(folder, true, OnFileChange);
            // Reload now
            ReloadAll();
        }

        private void OnFileChange(FileSystemEventArgs evt)
        {
            string changedFilePath = Path.GetFullPath(evt.FullPath);
            // The root folder within the packages folder we consider to be a "package"
            string basePackageFolder = Path.Combine(_folder, StupidMissingTypesHelper.GetPathRoot(changedFilePath.Substring(_folder.Length + 1)));
            ScheduleHelper.SafeLog($"Local Package Change: {basePackageFolder}");

            lock (_loadQueue)
            {
                // We should refresh queued packages in bulk.
                bool isFirst = _loadQueue.Count == 0;
                if (!_loadQueue.Contains(basePackageFolder))
                {
                    _loadQueue.Enqueue(basePackageFolder);
                }

                if (isFirst)
                {
                    // Wait for potential other loads to come in
                    Task.Run(async () =>
                    {
                        await Task.Delay(400);
                        RefreshQueuedPackages();
                    });
                }
            }
        }

        private void RefreshQueuedPackages()
        {
            while (true)
            {
                lock (_loadQueue)
                {
                    if (_loadQueue.Count <= 0)
                        break;
                    UpdatePackage(_loadQueue.Dequeue());
                }
            }
        }
    }
}
