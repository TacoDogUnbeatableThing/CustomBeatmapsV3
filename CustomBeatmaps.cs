using System;
using System.IO;
using System.Threading.Tasks;
using System.Timers;
using BepInEx;
using CustomBeatmaps.CustomPackages;
using CustomBeatmaps.Patches;
using CustomBeatmaps.Util;
using HarmonyLib;
using Debug = UnityEngine.Debug;

namespace CustomBeatmaps
{
    [BepInPlugin("tacodog.unbeatable.custombeatmaps", "Custom Beatmaps V3", "3.1.2")]
    public class CustomBeatmaps : BaseUnityPlugin
    {
        public static ModConfig ModConfig { get; private set; }
        public static BackendConfig BackendConfig { get; private set; }

        public static UserSession UserSession { get; private set; }

        public static LocalPackageManager LocalUserPackages { get; private set; }
        public static LocalPackageManager LocalServerPackages { get; private set; }
        public static SubmissionPackageManager SubmissionPackageManager { get; private set; }
        public static OSUBeatmapManager OSUBeatmapManager { get; private set; }
        public static PlayedPackageManager PlayedPackageManager { get; private set; }
        public static ServerHighScoreManager ServerHighScoreManager { get; private set; }
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
            OSUBeatmapManager = new OSUBeatmapManager();
            ServerHighScoreManager = new ServerHighScoreManager();

            ConfigHelper.LoadConfig("custombeatmaps_config.json",() => new ModConfig(), config =>
            {
                ModConfig = config;
                // Local package folders
                LocalUserPackages.SetFolder(config.UserPackagesDir);
                LocalServerPackages.SetFolder(config.ServerPackagesDir);
                OSUBeatmapManager.SetOverride(config.OsuSongsOverrideDirectory);
                PlayedPackageManager = new PlayedPackageManager(config.PlayedBeatmapList);
            });
            if (!Directory.Exists("CustomBeatmapsV3-Data"))
                Directory.CreateDirectory("CustomBeatmapsV3-Data");
            ConfigHelper.LoadConfig("CustomBeatmapsV3-Data/custombeatmaps_backend.json", () => new BackendConfig(), config => BackendConfig = config);

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
            Harmony.CreateAndPatchAll(typeof(OsuEditorPatch));
            Harmony.CreateAndPatchAll(typeof(HighScoreScreenPatch));
            Harmony.CreateAndPatchAll(typeof(SimpleJankHighScoreSongReplacementPatch));

        }
    }
}
