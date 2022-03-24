using System;
using System.Collections.Generic;
using Cinemachine;
using CustomBeatmaps.InvestigationUtils;
using FMODUnity;
using HarmonyLib;
using Rewired;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CustomBeatmaps.Patches
{
    public static class WhiteLabelMainMenuPatch
    {
        /// <summary>
        /// TODO:
        /// - Find a camera angle that works (move camera around w/ wasd + rotate?)
        /// - Add extra UI option
        /// </summary>
        private static readonly WhiteLabelMainMenu.MenuState _customMenuState =
            (WhiteLabelMainMenu.MenuState) 6;

        // Camera
        private static readonly Vector3 _customMenuCamPos = new Vector3(-4.8906f, -0.0448f, 7.3123f);
        private static readonly Quaternion _customMenuCamRot = Quaternion.Euler(343.5f, 36.5f, 0);
        private static CinemachineVirtualCamera _customMenuCam;

        // New Option
        private static UISelectionButton _customOption;

        [HarmonyPatch(typeof(WhiteLabelMainMenu), "Start")]
        [HarmonyPrefix]
        static void PreStart(WhiteLabelMainMenu __instance)
        {
            UISelectionButton toCopy = __instance.PlayOption;
            _customOption = Object.Instantiate(toCopy, toCopy.transform.parent);
            var copy = _customOption.transform.position;
            copy.z = 11.5527f; // Eh just move it
            _customOption.transform.position = copy;
            _customOption.setting.text = "CUSTOM\n<size=50%>   BEATMAPS</size>";

            __instance.TopLayerOptions.Add(_customOption);
        }

        [HarmonyPatch(typeof(WhiteLabelMainMenu), "Start")]
        [HarmonyPostfix]
        static void PostStart(WhiteLabelMainMenu __instance, ref WrapCounter ___selectionInc)
        {
            _customMenuCam = new GameObject().AddComponent<CinemachineVirtualCamera>();
            _customMenuCam.transform.position = _customMenuCamPos;
            _customMenuCam.transform.rotation = _customMenuCamRot;

            // Add one more option (our own!)
            ___selectionInc = new WrapCounter(___selectionInc.count + 1, 2);
        }

        [HarmonyPatch(typeof(WhiteLabelMainMenu), "Update")]
        [HarmonyPostfix]
        static void PostUpdate(WhiteLabelMainMenu __instance, Player ___rewired)
        {
            if (__instance.menuState == _customMenuState)
            {
                if (___rewired.GetButtonDown("Cancel") || ___rewired.GetButtonDown("Back"))
                {
                    ChooseCamera(__instance, __instance.defaultCam);
                    __instance.menuState = WhiteLabelMainMenu.MenuState.DEFAULT;
                    RuntimeManager.PlayOneShot(__instance.menuBackEvent);
                    return;
                }
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
                __runOriginal = false;
            }
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(WhiteLabelMainMenu), "ChooseCamera")]
        private static void ChooseCamera(object instance, CinemachineVirtualCamera camera)
        {
            throw new NotImplementedException("It's a stub");
        }
        
        [HarmonyPatch(typeof(WhiteLabelMainMenu), "ChooseCamera")]
        [HarmonyPrefix]
        static void ExtraChooseCamera(CinemachineVirtualCamera camera)
        {
            // Include custom cameras
            _customMenuCam.Priority = 10;
        }
    }
}