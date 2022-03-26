using System;
using UnityEngine;

namespace CustomBeatmaps.Util
{
    public static class GUIHelper
    {

        private static bool _canDoInputThisFrame;

        public static void AvoidInputOneFrame()
        {
            _canDoInputThisFrame = false;
        }

        public static void FreeInput()
        {
            _canDoInputThisFrame = true;
        }

        // This... does nothing...
        public static void SetDefaultStyles()
        {
            var bstyle = GUI.skin.button;
            bstyle.richText = true;
            GUI.skin.button = bstyle;
        }

        public static bool CanDoInput(bool forceSelected = false)
        {
            return Event.current.type == EventType.KeyUp || Event.current.type == EventType.KeyDown || (_canDoInputThisFrame && !forceSelected && Event.current.type == EventType.Repaint);
        }
    }
}