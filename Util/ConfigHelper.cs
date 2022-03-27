using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CustomBeatmaps.Util
{
    public static class ConfigHelper
    {
        private static readonly Dictionary<string, Action> LoadedConfigs = new Dictionary<string, Action>();
        private static readonly HashSet<string> FilesChanged = new HashSet<string>();

        public static void LoadConfig<T>(string filePath, Func<T> getDefaultConfig, Action<T> onReload)
        {
            // Register config file to be reloadable later, and load it now.
            Action reload = () =>
            {
                var toSet = GetConfig(filePath, getDefaultConfig);
                onReload(toSet);
            };
            LoadedConfigs.Add(Path.GetFullPath(filePath), reload);
            reload.Invoke();
            // Listen for file changes
            FileWatchHelper.WatchFileForModifications(filePath, () =>
            {
                lock (FilesChanged)
                {
                    FilesChanged.Add(Path.GetFullPath(filePath));
                }
            });
        }

        /// <summary>
        /// Reloads all config files manually
        /// </summary>
        public static void ReloadAllConfigs()
        {
            foreach (var onReload in LoadedConfigs.Values)
            {
                onReload?.Invoke();
            }
        }

        /// <summary>
        /// Reloads all config files that have changed/been written to
        /// </summary>
        public static void ReloadChangedConfigs()
        {
            lock (FilesChanged)
            {
                foreach (var filePath in FilesChanged)
                {
                    if (LoadedConfigs.ContainsKey(filePath))
                    {
                        Debug.Log($"Config Changed Reload: {filePath}");
                        LoadedConfigs[filePath]?.Invoke();
                    }
                }
                FilesChanged.Clear();
            }
        }

        private static T GetConfig<T>(string filePath, Func<T> getDefaultConfig)
        {
            try
            {
                T configToLoad = SerializeHelper.LoadJSON<T>(filePath);
                if (configToLoad == null)
                    throw new NullReferenceException();
                return configToLoad;
            }
            catch (Exception e)
            {
                ScheduleHelper.SafeLog($"FAILED CONFIG: {e}");
                T defaultConfig = getDefaultConfig();
                SerializeHelper.SaveJSON(filePath, defaultConfig);
                return defaultConfig;
            }
        }
    }
}
