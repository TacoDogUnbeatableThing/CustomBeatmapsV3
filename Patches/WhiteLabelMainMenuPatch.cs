using System;
using System.Reflection;
using Cinemachine;
using CustomBeatmaps.UI;
using CustomBeatmaps.UI.Highscore;
using CustomBeatmaps.Util;
using DG.Tweening;
using DG.Tweening.Core;
using FMOD.Studio;
using FMODUnity;
using HarmonyLib;
using Rewired;
using Rhythm;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CustomBeatmaps.Patches
{
    public static class WhiteLabelMainMenuPatch
    {
        private static WhiteLabelMainMenu _current;

        private static string SelectedSong => _current != null? (string)SelectedWhiteLabelBeatmapInfo.GetValue(_current) : null;

        /// <summary>
        /// TODO:
        /// - Find a camera angle that works (move camera around w/ wasd + rotate?)
        /// - Add extra UI option
        /// </summary>
        private static readonly WhiteLabelMainMenu.MenuState _customMenuState =
            (WhiteLabelMainMenu.MenuState) 6;

        // Camera
        private static readonly Vector3 CustomMenuCamPos = new Vector3(-4.8906f, -0.0448f, 7.3123f);
        private static readonly Quaternion CustomMenuCamRot = Quaternion.Euler(343.5f, 36.5f, 0);
        private static CinemachineVirtualCamera _customMenuCam;

        // New Option
        private static UISelectionButton _customOption;
        private static int _mainMenuWrapCount;

        // CustomBeatmaps UI Behavior
        private static CustomBeatmapsUIBehaviour _customBeatmapsUIBehaviour;

        private static string _currentSongPreview;
        
        // Walkman High Score
        private static HighScoreUIBehaviour _walkmanHighScoreUI;

        private static readonly FieldInfo SongPreviewInstanceInfo = typeof(WhiteLabelMainMenu).GetField("songPreviewInstance", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly PropertyInfo SelectedWhiteLabelBeatmapInfo = typeof(WhiteLabelMainMenu).GetProperty("selectedBeatmapPath", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetProperty);

        // Song Preview Access
        public static void PlaySongPreview(string audioFile)
        {
            if (_currentSongPreview == audioFile)
                return;
            if (_current != null)
            {
                _currentSongPreview = audioFile;

                // MANUAL REWRITE replacing PlaySource.FromTable to PlaySource.FromFile

                var songPreviewInstance = (EventInstance) SongPreviewInstanceInfo.GetValue(_current);
                songPreviewInstance.setVolume(1.0f);

                if (songPreviewInstance.isValid())
                {
                    int num1 = (int) songPreviewInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                    int num2 = (int) songPreviewInstance.release();
                }
                songPreviewInstance = RuntimeManager.CreateInstance(_current.songPreviewEvent);
                SongPreviewInstanceInfo.SetValue(_current, songPreviewInstance);
                RhythmTracker.PrepareInstance(songPreviewInstance, PlaySource.FromFile, audioFile);
                int num3 = (int) songPreviewInstance.setPitch(JeffBezosController.songSpeed);
                int num4 = (int) songPreviewInstance.start();
            }
        }
        public static void StopSongPreview()
        {
            if (_current != null && _currentSongPreview != null)
            {
                StopSongPreview(_current);
                _currentSongPreview = null;
            }
        }

        public static void DisableBGM()
        {
            if (_current == null)
                return;
            DOTween.To(() => GetMenuMusicVolume(_current), val => SetMenuMusicVolume(_current, val), 0.0f, 1f);
            DOTween.To(() => GetTrainLoopVolume(_current), val => SetTrainLoopVolume(_current, val), 0.1f, 1f);
        }

        public static void EnableBGM()
        {
            if (_current == null)
                return;
            DOTween.To(() => GetMenuMusicVolume(_current), val => SetMenuMusicVolume(_current, val), 1f, 1f);
            DOTween.To(() => GetTrainLoopVolume(_current), val => SetTrainLoopVolume(_current, val), 1f, 1f);
        }


        [HarmonyPatch(typeof(WhiteLabelMainMenu), "Start")]
        [HarmonyPrefix]
        static void PreStart(WhiteLabelMainMenu __instance)
        {
            _current = __instance;

            // Create custom UI
            _customBeatmapsUIBehaviour = new GameObject().AddComponent<CustomBeatmapsUIBehaviour>();
            _walkmanHighScoreUI = new GameObject().AddComponent<HighScoreUIBehaviour>();
            // For our walkman, get our high score from the selected beatmap
            _walkmanHighScoreUI.Init(true, () => UserServerHelper.GetHighScoreBeatmapKeyFromUnbeatableBeatmap(SelectedSong));

            // Create custom option button
            UISelectionButton toCopy = __instance.PlayOption;
            _customOption = Object.Instantiate(toCopy, toCopy.transform.parent);
            var copy = _customOption.transform.position;
            copy.z = 11.5527f + 0.2f; // Eh just move it
            _customOption.transform.position = copy;
            _customOption.setting.text = "CUSTOM\n<size=50%>   BEATMAPS</size>";

            // Custom menu camera
            _customMenuCam = new GameObject().AddComponent<CinemachineVirtualCamera>();
            _customMenuCam.transform.position = CustomMenuCamPos;
            _customMenuCam.transform.rotation = CustomMenuCamRot;

            __instance.TopLayerOptions.Add(_customOption);

            // Post-Custom Beatmap Play
            if (CustomBeatmapLoadingOverridePatch.CustomBeatmapSet())
            {
                // Avoid going to regular beatmap view
                JeffBezosController.returnFromArcade = false;
                // Load our custombeatmaps menu
                ChooseCamera(__instance, _customMenuCam);
                __instance.menuState = _customMenuState;
                _customBeatmapsUIBehaviour.Open();
                DisableBGM();
            }

            // Reset custom beatmap playing
            CustomBeatmapLoadingOverridePatch.ResetOverrideBeatmap();
            // Also reset server high score key so we don't interfere with vanilla beatmaps 
            CustomBeatmaps.ServerHighScoreManager.ResetCurrentBeatmapKey();

            // Load our high scores just to be clean/up to date
            CustomBeatmaps.ServerHighScoreManager.Reload();
        }

        [HarmonyPatch(typeof(WhiteLabelMainMenu), "Start")]
        [HarmonyPostfix]
        static void PostStart(WhiteLabelMainMenu __instance, ref WrapCounter ___selectionInc)
        {

            // Add one more option (our own!)
            _mainMenuWrapCount = ___selectionInc.count + 1;
            ___selectionInc = new WrapCounter(_mainMenuWrapCount, 2);
        }

        [HarmonyPatch(typeof(WhiteLabelMainMenu), "Update")]
        [HarmonyPostfix]
        static void PostUpdateCustomMenuEscape(WhiteLabelMainMenu __instance, Player ___rewired)
        {
            // Escape our menu
            if (__instance.menuState == _customMenuState)
            {
                if (___rewired.GetButtonDown("Cancel") || ___rewired.GetButtonDown("Back"))
                {
                    ChooseCamera(__instance, __instance.defaultCam);
                    __instance.menuState = WhiteLabelMainMenu.MenuState.DEFAULT;
                    RuntimeManager.PlayOneShot(__instance.menuBackEvent);
                    _customBeatmapsUIBehaviour.Close();
                    return;
                }

                // For some reason it's hidden by default?
                Cursor.visible = true;
            }
        }

        [HarmonyPatch(typeof(WhiteLabelMainMenu), "Update")]
        [HarmonyPostfix]
        static void PostUpdateCustomHighScore(WhiteLabelMainMenu __instance)
        {
            if (__instance.menuState == WhiteLabelMainMenu.MenuState.LEVELSELECT)
            {
                _walkmanHighScoreUI.Open();
                // Let us move the high score UI around.
                Cursor.visible = true;
            }
            else
                _walkmanHighScoreUI.Close();
            
        }

        [HarmonyPatch(typeof(WhiteLabelMainMenu), "MenuDefaultUpdate")]
        [HarmonyPrefix]
        static void MenuDefaultPreUpdate(WhiteLabelMainMenu __instance, ref bool __runOriginal, ref WrapCounter ___selectionInc, Player ___rewired)
        {
            // We selected our custom option
            if (___selectionInc == 3 && ___rewired.GetButtonDown("Interact"))
            {
                Debug.Log(_customMenuCam);
                ChooseCamera(__instance, _customMenuCam);
                __instance.menuState = _customMenuState;
                RuntimeManager.PlayOneShot(__instance.menuAcceptEvent);
                _customBeatmapsUIBehaviour.Open();
                __runOriginal = false;
            }
        }

        [HarmonyPatch(typeof(WhiteLabelMainMenu), "MenuQuitUpdate")]
        [HarmonyPostfix]
        static void MenuQuitPostUpdate(ref WrapCounter ___selectionInc, Player ___rewired)
        {
            // Make our post quit wrap counter match our new wrap counter
            if (___rewired.GetButtonDown("Cancel") || ___rewired.GetButtonDown("Back"))
            {
                ___selectionInc = new WrapCounter(_mainMenuWrapCount, 0);
            }
        }

        [HarmonyPatch(typeof(WhiteLabelMainMenu), "ChooseCamera")]
        [HarmonyPrefix]
        static void ExtraChooseCamera(CinemachineVirtualCamera camera)
        {
            // Include custom cameras
            _customMenuCam.Priority = 10;
        }

        /*
        [HarmonyPatch(typeof(WhiteLabelMainMenu), "PlaySongPreview", typeof(string))]
        [HarmonyPrefix]
        static void Test(string audioPath)
        {
            Debug.Log($"SONG: {audioPath}");
        }
        */
        
        // REVERSE PATCHES
        
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(WhiteLabelMainMenu), "ChooseCamera")]
        private static void ChooseCamera(object instance, CinemachineVirtualCamera camera) => throw new InvalidOperationException("Stub Function");

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(WhiteLabelMainMenu), "StopSongPreview")]
        private static void StopSongPreview(object instance) =>
            throw new InvalidOperationException("Stub Function");

        private static readonly MethodInfo GetMenuMusicInfo = typeof(WhiteLabelMainMenu).GetMethod("GetMenuMusicVolume", BindingFlags.NonPublic | BindingFlags.Instance);
        private static float GetMenuMusicVolume(object instance)
        {
            return (float) GetMenuMusicInfo.Invoke(instance, Array.Empty<object>());
        }
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(WhiteLabelMainMenu), "SetMenuMusicVolume")]
        private static void SetMenuMusicVolume(object instance, float volume) => throw new InvalidOperationException("Stub Function");
        private static readonly MethodInfo GetTrainLoopInfo = typeof(WhiteLabelMainMenu).GetMethod("GetTrainLoopVolume", BindingFlags.NonPublic | BindingFlags.Instance);
        private static float GetTrainLoopVolume(object instance)
        {
            return (float) GetTrainLoopInfo.Invoke(instance, Array.Empty<object>());
        }
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(WhiteLabelMainMenu), "SetTrainLoopVolume")]
        private static void SetTrainLoopVolume(object instance, float volume) => throw new InvalidOperationException("Stub Function");
    }
}
