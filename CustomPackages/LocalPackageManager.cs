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

        /// <summary>
        /// Given a server package URL and a beatmap info from the server, find our local beatmap.
        /// This is the most fragile point of this system, as modifying the server files breaks the system.
        /// </summary>
        public CustomBeatmapInfo FindCustomBeatmapInfoFromServer(string serverPackageURL, CustomServerBeatmap beatmapInfo)
        {
            string targetFullPath = CustomPackageHelper.GetLocalFolderFromServerPackageURL(
                Config.Mod.ServerPackagesDir, serverPackageURL);
            targetFullPath = Path.GetFullPath(targetFullPath);
            bool foundPackage = false;
            foreach (var package in Packages)
            {
                string currentFullPath = Path.GetFullPath(package.FolderName); 
                bool samePackage = currentFullPath == targetFullPath;
                //Debug.Log($"{currentFullPath} compared to {targetFullPath}");
                if (samePackage)
                {
                    foundPackage = true;
                    foreach (var cbinfo in package.Beatmaps)
                    {
                        //Debug.Log($"    {cbinfo} compared to {beatmapInfo.Artist}, {beatmapInfo.Difficulty}, {beatmapInfo.Creator}, {beatmapInfo.Name}");
                        if (cbinfo.Artist == beatmapInfo.Artist &&
                            cbinfo.Difficulty == beatmapInfo.Difficulty &&
                            cbinfo.BeatmapCreator == beatmapInfo.Creator &&
                            cbinfo.SongName == beatmapInfo.Name)
                        {
                            return cbinfo;
                        }
                    }
                }
            }

            if (!foundPackage)
            {
                throw new InvalidOperationException($"Can't find package at {targetFullPath}");
            }
            throw new InvalidOperationException(
                $"Can't find beatmap {beatmapInfo} at folder {targetFullPath}");
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
