using System;
using System.Threading.Tasks;
using System.Timers;
using BepInEx;
using CustomBeatmaps.CustomPackages;
using CustomBeatmaps.Patches;
using CustomBeatmaps.Util;
using FMOD;
using HarmonyLib;
using Debug = UnityEngine.Debug;

namespace CustomBeatmaps
{
    [BepInPlugin("tacodog.unbeatable.custombeatmaps", "Custom Beatmaps V3", "3.0.0")]
    public class CustomBeatmaps : BaseUnityPlugin
    {
        public static ModConfig ModConfig { get; private set; }
        public static BackendConfig BackendConfig { get; private set; }

        public static UserSession UserSession { get; private set; }

        public static LocalPackageManager LocalUserPackages { get; private set; }
        public static LocalPackageManager LocalServerPackages { get; private set; }
        public static SubmissionPackageManager SubmissionPackageManager { get; private set; }

        public static BeatmapDownloader Downloader { get; private set; }

        // Check for config reload every 2 seconds
        private readonly Timer _checkConfigReload = new Timer(2000);

        static CustomBeatmaps()
        {
            // Log inner exceptions by default
            EventBus.ExceptionThrown += ex => ScheduleHelper.SafeInvoke(() => Debug.LogException(ex));
            
            // Anything with Static access should be ALWAYS present.
            LocalUserPackages = new LocalPackageManager(OnError);
            LocalServerPackages = new LocalPackageManager(OnError);
            SubmissionPackageManager = new SubmissionPackageManager(OnError);

            ConfigHelper.LoadConfig("custombeatmaps_config.yaml",() => new ModConfig(), config =>
            {
                ModConfig = config;
                // Local package folders
                LocalUserPackages.SetFolder(config.UserPackagesDir);
                LocalServerPackages.SetFolder(config.ServerPackagesDir);
            });
            ConfigHelper.LoadConfig("custombeatmaps_backend.yaml", () => new BackendConfig(), config => BackendConfig = config);

            UserSession = new UserSession();

            Downloader = new BeatmapDownloader();
        }

        private static void OnError(Exception ex)
        {
            EventBus.ExceptionThrown?.Invoke(ex);
        }

        private void Awake()
        {
            Logger.LogInfo("CustomBeatmapsV3: Awake?");

            // At a regular interval, reload changed configs.
            _checkConfigReload.Elapsed += (obj, evt) => ScheduleHelper.SafeInvoke(ConfigHelper.ReloadChangedConfigs);
            _checkConfigReload.Start();

            // User session
            Task.Run(UserSession.AttemptLogin);

            // Harmony Patching
            Harmony.CreateAndPatchAll(typeof(DebugLogPatch));
            Harmony.CreateAndPatchAll(typeof(WhiteLabelMainMenuPatch));
            Harmony.CreateAndPatchAll(typeof(CustomBeatmapLoadingOverridePatch));

            /*
            // Test fetching our package list
            CustomPackageHelper.FetchServerPackageList(BackendConfig.ServerPackageList).ContinueWith(i =>
            {
                ScheduleHelper.SafeLog(i.Result);
            });
            */
        }
    }
}
