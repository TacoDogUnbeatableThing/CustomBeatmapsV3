using System;
using System.Collections.Generic;
using System.IO;
using CustomBeatmaps.Util;
using UnityEngine;

namespace CustomBeatmaps.CustomPackages
{
    /// <summary>
    /// Manages Local packages in the USER_PACKAGES or SERVER_PACKAGES directory
    /// </summary>
    public class LocalPackageManager
    {
        private readonly List<CustomLocalPackage> _packages = new List<CustomLocalPackage>();
        private readonly Action<BeatmapException> _onLoadException;

        private string _folder;
        private FileSystemWatcher _watcher;

        public LocalPackageManager(Action<BeatmapException> onLoadException)
        {
            _onLoadException = onLoadException;
        }

        private void Reload()
        {
            if (_folder == null)
                return;
            lock (_packages)
            {
                _packages.Clear();
                _packages.AddRange(CustomPackageHelper.LoadLocalPackages(_folder, _onLoadException));
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
            _watcher = FileWatchHelper.WatchFolder(folder, true, Reload);
            // Reload now
            Reload();
        }
    }
}
