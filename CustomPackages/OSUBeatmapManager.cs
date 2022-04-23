using System;
using System.Collections.Generic;
using System.IO;
using CustomBeatmaps.Util;

namespace CustomBeatmaps.CustomPackages
{
    public class OSUBeatmapManager
    {
        private readonly List<CustomBeatmapInfo> _beatmaps = new List<CustomBeatmapInfo>();
        private string _folderOverride;

        private FileSystemWatcher _watcher;

        public CustomBeatmapInfo[] OsuBeatmaps {
            get
            {
                lock (_beatmaps)
                {
                    return _beatmaps.ToArray();
                }
            }
        }
        
        public string Error { get; private set; }

        public void SetOverride(string folderOverride)
        {
            if (!string.Equals(_folderOverride, folderOverride, StringComparison.Ordinal))
            {
                _folderOverride = folderOverride;
            }
            
            // Clear previous watcher
            if (_watcher != null)
            {
                _watcher.Dispose();
            }

            string folder = OSUHelper.GetOsuPath(Config.Mod.OsuSongsOverrideDirectory);
            try
            {
                // Watch for changes
                _watcher = FileWatchHelper.WatchFolder(folder, true, evt => Reload());
            }
            catch (Exception e)
            {
                EventBus.ExceptionThrown?.Invoke(new DirectoryNotFoundException($"Can't find folder: {folder}", e));
            }

            // Reload now
            Reload();
        }

        private void Reload()
        {
            if (_folderOverride == null)
                return;
            lock (_beatmaps)
            {
                string folder = OSUHelper.GetOsuPath(Config.Mod.OsuSongsOverrideDirectory);
                var bmaps = OSUHelper.LoadOsuBeatmaps(folder);
                if (bmaps != null)
                {
                    _beatmaps.Clear();
                    _beatmaps.AddRange(bmaps);
                    Error = null;
                }
                else
                {
                    Error = $"Can't find OSU songs path at {folder}";
                    EventBus.ExceptionThrown?.Invoke(new DirectoryNotFoundException(Error));
                }
            }
        }
    }
}
