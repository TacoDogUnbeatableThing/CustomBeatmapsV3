﻿using System;
using Cinemachine;
using CustomBeatmaps.UI;
using FMODUnity;
using HarmonyLib;
using Rewired;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CustomBeatmaps.Patches
{
    public static class WhiteLabelMainMenuPatch
    {
        private static WhiteLabelMainMenu _current;

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
        // Song Preview Access
        public static void PlaySongPreview(string songName)
        {
            if (_currentSongPreview == songName)
                return;
            if (_current != null)
            {
                _currentSongPreview = songName;
                PlaySongPreview(_current, songName);
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

        [HarmonyPatch(typeof(WhiteLabelMainMenu), "Start")]
        [HarmonyPrefix]
        static void PreStart(WhiteLabelMainMenu __instance)
        {
            // Don't have custom beatmaps here
            CustomBeatmapLoadingOverridePatch.ResetOverrideBeatmap();
            _current = __instance;

            // Create custom option button
            UISelectionButton toCopy = __instance.PlayOption;
            _customOption = Object.Instantiate(toCopy, toCopy.transform.parent);
            var copy = _customOption.transform.position;
            copy.z = 11.5527f; // Eh just move it
            _customOption.transform.position = copy;
            _customOption.setting.text = "CUSTOM\n<size=50%>   BEATMAPS</size>";

            __instance.TopLayerOptions.Add(_customOption);

            // Create custom beatmaps UI
            _customBeatmapsUIBehaviour = new GameObject().AddComponent<CustomBeatmapsUIBehaviour>();
        }

        [HarmonyPatch(typeof(WhiteLabelMainMenu), "Start")]
        [HarmonyPostfix]
        static void PostStart(WhiteLabelMainMenu __instance, ref WrapCounter ___selectionInc)
        {
            _customMenuCam = new GameObject().AddComponent<CinemachineVirtualCamera>();
            _customMenuCam.transform.position = CustomMenuCamPos;
            _customMenuCam.transform.rotation = CustomMenuCamRot;

            // Add one more option (our own!)
            _mainMenuWrapCount = ___selectionInc.count + 1;
            ___selectionInc = new WrapCounter(_mainMenuWrapCount, 2);
        }

        [HarmonyPatch(typeof(WhiteLabelMainMenu), "Update")]
        [HarmonyPostfix]
        static void PostUpdate(WhiteLabelMainMenu __instance, Player ___rewired)
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

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(WhiteLabelMainMenu), "ChooseCamera")]
        private static void ChooseCamera(object instance, CinemachineVirtualCamera camera) => throw new InvalidOperationException("Stub Function");

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(WhiteLabelMainMenu), "PlaySongPreview", typeof(string))]
        private static void PlaySongPreview(object instance, string audioPath) =>
            throw new InvalidOperationException("Stub Function");
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(WhiteLabelMainMenu), "StopSongPreview")]
        private static void StopSongPreview(object instance) =>
            throw new InvalidOperationException("Stub Function");
        
        [HarmonyPatch(typeof(WhiteLabelMainMenu), "ChooseCamera")]
        [HarmonyPrefix]
        static void ExtraChooseCamera(CinemachineVirtualCamera camera)
        {
            // Include custom cameras
            _customMenuCam.Priority = 10;
        }
    }
}