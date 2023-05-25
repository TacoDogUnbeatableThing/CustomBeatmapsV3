using System;
using System.IO;
using System.Threading.Tasks;
using System.Timers;
using BepInEx;
using CustomBeatmaps.CustomPackages;
using CustomBeatmaps.Patches;
using CustomBeatmaps.UI;
using CustomBeatmaps.Util;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace CustomBeatmaps
{
    [BepInPlugin("tacodog.unbeatable.custombeatmaps", "Custom Beatmaps V3", "3.3.4")]
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
        public static GameMemory Memory { get; private set; }

        private static readonly string MEMORY_LOCATION = "CustomBeatmapsV3-Data/.memory";

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

            if (!Directory.Exists("CustomBeatmapsV3-Data"))
                Directory.CreateDirectory("CustomBeatmapsV3-Data");

            // Load game memory from disk
            Memory = GameMemory.Load(MEMORY_LOCATION);

            ConfigHelper.LoadConfig("custombeatmaps_config.json",() => new ModConfig(), config =>
            {
                ModConfig = config;
                // Local package folders
                LocalUserPackages.SetFolder(config.UserPackagesDir);
                LocalServerPackages.SetFolder(config.ServerPackagesDir);
                OSUBeatmapManager.SetOverride(config.OsuSongsOverrideDirectory);
                PlayedPackageManager = new PlayedPackageManager(config.PlayedBeatmapList);
            });
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
            Type[] classesToPatch = {
                typeof(DebugLogPatch),
                typeof(WhiteLabelMainMenuPatch),
                typeof(CustomBeatmapLoadingOverridePatch),
                typeof(OsuEditorPatch),
                typeof(HighScoreScreenPatch),
                typeof(PauseMenuPatch),
                typeof(DisablePracticeRoomOpenerPatch),
                typeof(CursorUnhidePatch),
                typeof(OneLifeModePatch),
                typeof(FlipModePatch),
                typeof(SimpleJankHighScoreSongReplacementPatch),
            };
            foreach (var toPatch in classesToPatch)
            {
                try
                {
                    Harmony.CreateAndPatchAll(toPatch);
                }
                catch (Exception e)
                {
                    Logger.LogError("EXCEPTION CAUGHT while PATCHING:");
                    Logger.LogError(e.ToString());
                }
            }

            // Disclaimer screen
            if (!Memory.OpeningDisclaimerDisabled)
            {
                foreach (var obj in FindObjectsOfType(typeof(GameObject)))
                {
                    var gobject = (GameObject) obj;
                    var ourselves = gobject.GetComponentInChildren<CustomBeatmaps>();
                    if (ourselves == null)
                    {
                        DestroyImmediate(gobject);
                    }
                }

                var disclaimer = new GameObject().AddComponent<OpeningDisclaimerUIBehaviour>();
                disclaimer.OnSelect += () =>
                {
                    // Reload game
                    Memory.OpeningDisclaimerDisabled = true;
                    GameMemory.Save(MEMORY_LOCATION, Memory);
                    SceneManager.LoadScene(0);
                };
            }
        }

        private static bool _quitted;
        private void OnDestroy()
        {
            // Save our memory
            if (!_quitted)
                GameMemory.Save(MEMORY_LOCATION, Memory);
            _quitted = true;
        }

        private void OnApplicationQuit()
        {
            // Save our memory
            if (!_quitted)
                GameMemory.Save(MEMORY_LOCATION, Memory);
            _quitted = true;
        }
    }
}
