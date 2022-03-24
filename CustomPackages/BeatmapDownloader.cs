using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CustomBeatmaps.Util;

namespace CustomBeatmaps.CustomPackages
{
    public class BeatmapDownloader
    {
        private readonly Queue<string> _queuedIdsToDownload = new Queue<string>();
        private string _currentlyDownloading;

        public Action<string> PackageDownloaded;

        public BeatmapDownloadStatus GetDownloadStatus(string serverPackageDirectory, string serverPackageURL)
        {
            // Check the local package folder, if it exists then we've downloaded it
            string packageFolder = CustomPackageHelper.GetLocalFolderFromServerPackageURL(serverPackageDirectory, serverPackageURL);
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

            ModConfig config = CustomBeatmaps.ModConfig;
            string localURL = await CustomPackageHelper.DownloadPackage(config.ServerStorageURL, config.ServerPackageRoot,
                config.ServerPackagesDir, serverPackageURL);

            PackageDownloaded?.Invoke(localURL);

            // After we're done, grab the next one.
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

        public void DownloadPackage(string serverPackageURL)
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