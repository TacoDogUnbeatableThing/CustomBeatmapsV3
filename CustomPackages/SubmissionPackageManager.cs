using System;
using System.Collections.Generic;
using CustomBeatmaps.Util;

namespace CustomBeatmaps.CustomPackages
{
    public class SubmissionPackageManager
    {
        private readonly List<ServerSubmissionPackage> _submissionPackages = new List<ServerSubmissionPackage>();
        private bool _fetching = false;

        private string _downloadedPackageURL;
        private CustomLocalPackage _localPackage;

        public bool LocalPackageDownloaded => _downloadedPackageURL != null;
        public string LocalPackageDownloadURL => _downloadedPackageURL;
        public CustomLocalPackage LocalPackage => _localPackage;

        private readonly Action<BeatmapException> _onLoadException;

        public SubmissionPackageManager(Action<BeatmapException> onLoadException)
        {
            _onLoadException = onLoadException;
        }

        public List<ServerSubmissionPackage> SubmissionPackages
        {
            get
            {
                lock (_submissionPackages)
                {
                    return _submissionPackages;
                }
            }
        }

        public async void DownloadSubmission(string url)
        {
            string submissionPackageFolder = Config.Mod.TemporarySubmissionPackageFolder;

            await CustomPackageHelper.DownloadTemporarySubmissionPackage(url, submissionPackageFolder);

            // Load the local package after downloading
            if (CustomPackageHelper.TryLoadLocalPackage(submissionPackageFolder, ".",
                    out _localPackage, true, _onLoadException))
            {
                _downloadedPackageURL = url;
            }
            else
            {
                ScheduleHelper.SafeLog("DOWNLOAD SUBMISSION FAILED!");
                _downloadedPackageURL = null;
            }
        }

        public async void RefreshServerSubmissions()
        {
            // Already being handled.
            if (_fetching)
                return;
            _fetching = true;
            var packages = await CustomPackageHelper.FetchServerSubmissions(Config.Backend.ServerSubmissionList);
            lock (packages)
            {
                _submissionPackages.Clear();
                _submissionPackages.AddRange(packages.Values);
            }
            _fetching = false;
        }
    }
}