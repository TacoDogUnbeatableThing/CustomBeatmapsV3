using HarmonyLib;
using Rewired;
using UnityEngine;

namespace CustomBeatmaps.Patches
{
    /// <summary>
    /// Starting patch V1.0.11, the mouse selects things which kinda breaks our UI
    /// </summary>
    public static class DisableRewiredMouseInputPatch
    {

        static void OverrideButton(string actionName, ref bool __result, ref bool __runOriginal)
        {
            switch (actionName)
            {
                case "Interact" when Input.GetMouseButtonDown(0):
                    __runOriginal = false;
                    __result = false;
                    break;
                case "Vertical":
                case "Horizontal" when Input.mouseScrollDelta.magnitude > 0:
                    __runOriginal = false;
                    __result = false;
                    break;
            }
        }
        
        [HarmonyPatch(typeof(Player), "GetButtonDown", typeof(string))]
        [HarmonyPrefix]
        static void OverrideGetButtonDown(string actionName, ref bool __result, ref bool __runOriginal)
        {
            OverrideButton(actionName, ref __result, ref __runOriginal);
        }
        [HarmonyPatch(typeof(Player), "GetNegativeButtonDown", typeof(string))]
        [HarmonyPrefix]
        static void OverrideGetNegativeButtonDown(string actionName, ref bool __result, ref bool __runOriginal)
        {
            OverrideButton(actionName, ref __result, ref __runOriginal);
        }
    }
}