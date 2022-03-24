using System.Timers;
using BepInEx;
using CustomBeatmaps.CustomPackages;
using CustomBeatmaps.Patches;
using CustomBeatmaps.Util;
using HarmonyLib;
using UnityEngine;

namespace CustomBeatmaps
{
    [BepInPlugin("tacodog.unbeatable.custombeatmaps", "Custom Beatmaps V3", "3.0.0")]
    public class CustomBeatmaps : BaseUnityPlugin
    {
        public static ModConfig ModConfig { get; private set; }
        public static UserSession UserSession { get; private set; }

        static CustomBeatmaps()
        {
            ConfigHelper.LoadConfig("custombeatmaps_config.yaml",() => new ModConfig(), config => ModConfig = config);
            UserSession = new UserSession();
        }

        // Check for config reload every 2 seconds
        private readonly Timer _checkConfigReload = new Timer(2000);

        private void Awake()
        {
            Logger.LogInfo("CustomBeatmapsV3: Awake?");

            // At a regular interval, reload changed configs.
            _checkConfigReload.Elapsed += (obj, evt) => ScheduleHelper.SafeInvoke(ConfigHelper.ReloadChangedConfigs);
            _checkConfigReload.Start();

            // User session
            UserSession.LoadUserSession(ModConfig.UserUniqueIdFile);

            // Harmony Patching
            Harmony.CreateAndPatchAll(typeof(DebugLogPatch));
            Harmony.CreateAndPatchAll(typeof(WhiteLabelMainMenuPatch));

            // Test fetching our package list
            CustomPackageHelper.FetchServerPackageList(ModConfig.ServerPackageList).ContinueWith(i =>
            {
                ScheduleHelper.SafeLog(i.Result);
            });
        }
    }
}
