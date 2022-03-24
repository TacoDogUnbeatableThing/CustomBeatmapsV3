using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CustomBeatmaps.Util
{
    public static class ConfigHelper
    {
        private static readonly Dictionary<string, Action> _loadedConfigs = new Dictionary<string, Action>();
        private static readonly HashSet<string> _filesChanged = new HashSet<string>();

        public static void LoadConfig<T>(string filePath, Func<T> getDefaultConfig, Action<T> onReload)
        {
            // Register config file to be reloadable later, and load it now.
            Action reload = () =>
            {
                var toSet = GetConfig(filePath, getDefaultConfig);
                ScheduleHelper.SafeLog("RELOADING: ");
                ScheduleHelper.SafeLog(toSet);
                onReload(toSet);
            };
            _loadedConfigs.Add(Path.GetFullPath(filePath), reload);
            ScheduleHelper.SafeLog("doing the reload");
            reload.Invoke();
            // Listen for file changes
            FileWatchHelper.WatchFileForModifications(filePath, () =>
            {
                lock (_filesChanged)
                {
                    _filesChanged.Add(Path.GetFullPath(filePath));
                }
            });
        }

        /// <summary>
        /// Reloads all config files manually
        /// </summary>
        public static void ReloadAllConfigs()
        {
            foreach (var onReload in _loadedConfigs.Values)
            {
                onReload?.Invoke();
            }
        }

        /// <summary>
        /// Reloads all config files that have changed/been written to
        /// </summary>
        public static void ReloadChangedConfigs()
        {
            lock (_filesChanged)
            {
                foreach (var filePath in _filesChanged)
                {
                    if (_loadedConfigs.ContainsKey(filePath))
                    {
                        Debug.Log($"Config Changed Reload: {filePath}");
                        _loadedConfigs[filePath]?.Invoke();
                    }
                }
                _filesChanged.Clear();
            }
        }

        private static T GetConfig<T>(string filePath, Func<T> getDefaultConfig)
        {
            try
            {
                T configToLoad = SerializeHelper.LoadYAML<T>(filePath);
                if (configToLoad == null)
                    throw new NullReferenceException();
                return configToLoad;
            }
            catch (Exception e)
            {
                T defaultConfig = getDefaultConfig();
                SerializeHelper.SaveYAML(filePath, defaultConfig);
                return defaultConfig;
            }
        }
    }
}
