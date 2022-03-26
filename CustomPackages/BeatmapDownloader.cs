using System;
using System.Collections.Generic;
using System.IO;
using CustomBeatmaps.Util;

namespace CustomBeatmaps.CustomPackages
{
    public class BeatmapDownloader
    {
        private readonly Queue<string> _queuedIdsToDownload = new Queue<string>();
        private string _currentlyDownloading;

        public Action<string> PackageDownloaded;

        public BeatmapDownloadStatus GetDownloadStatus(string serverPackageURL)
        {
            // Check the local package folder, if it exists then we've downloaded it
            string packageFolder = CustomPackageHelper.GetLocalFolderFromServerPackageURL(Config.Mod.ServerPackagesDir, serverPackageURL);

            if (Directory.Exists(packageFolder))
                return BeatmapDownloadStatus.Downloaded;

            // Check if we're downloading/are queued to download this file...
            lock (_queuedIdsToDownload)
            {
                if (_currentlyDownloading == serverPackageURL)
                    return BeatmapDownloadStatus.CurrentlyDownloading;
                if (_queuedIdsToDownload.Contains(serverPackageURL))
                    return BeatmapDownloadStatus.Queued;
            }
            // It's not in the queue
            return BeatmapDownloadStatus.NotDownloaded;
        }

        private async void DownloadPackageInner(string serverPackageURL)
        {
            _currentlyDownloading = serverPackageURL;

            string localURL = await CustomPackageHelper.DownloadPackage(Config.Backend.ServerStorageURL, Config.Backend.ServerPackageRoot,
                Config.Mod.ServerPackagesDir, serverPackageURL);
            PackageDownloaded?.Invoke(localURL);

            // We downloaded one, grab the next one.
            lock (_queuedIdsToDownload)
            {
                if (_queuedIdsToDownload.TryDequeue(out var upNext))
                {
                    DownloadPackageInner(upNext);
                }
                else
                {
                    // No more left, we're done downloading.
                    _currentlyDownloading = null;
                }
            }
        }

        public void QueueDownloadPackage(string serverPackageURL)
        {
            lock (_queuedIdsToDownload)
            {
                bool notDownloading = _currentlyDownloading == null;
                if (notDownloading)
                {
                    DownloadPackageInner(serverPackageURL);
                }
                else
                {
                    _queuedIdsToDownload.Enqueue(serverPackageURL);
                }

            }
        }
    }
}