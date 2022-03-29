using UnityEngine;

namespace CustomBeatmaps.Util
{
    public static class GUIHelper
    {

        private static bool _canDoInputThisFrame;

        private static Color _defaultBackground;

        static GUIHelper()
        {
            _defaultBackground = GUI.backgroundColor;
        }

        public static void SetDarkMode(bool darkMode)
        {
            GUI.backgroundColor = darkMode? Color.black : _defaultBackground;
        }
        
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
            return forceSelected
                ? (Event.current.type == EventType.KeyUp || Event.current.type == EventType.KeyDown)
                : (_canDoInputThisFrame && Event.current.type == EventType.Repaint);
        }
    }
}