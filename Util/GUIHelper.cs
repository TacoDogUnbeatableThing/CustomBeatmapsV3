using UnityEngine;

namespace CustomBeatmaps.Util
{
    public static class GUIHelper
    {

        private static bool _canDoInputThisFrame;

        private static Color _defaultBackground;

        // Screen scaling jank (for dill)
        public static float PerformScreenScale()
        {
            float upperScalingWidth = 1024;
            float lowerScalingWidth = 640;
            float lowerScalingPercent = 0.7f;
            if (Screen.width <= upperScalingWidth)
            {
                float scaleFactor = Mathf.InverseLerp(upperScalingWidth, lowerScalingWidth, Screen.width);
                float scale = Mathf.Lerp(1.0f, lowerScalingPercent, scaleFactor);
                GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
                return scale;
            }
            return 1;
        }
        
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

        public static Vector2 CalculateSize(GUIContent content, GUIStyle style, params GUILayoutOption[] options)
        {
            GUILayout.BeginArea(new Rect(0, 0, 999999, 99999));
            Vector2 size = GUILayoutUtility.GetRect(content, style, options).size;
            GUILayout.EndArea();
            return size;
        }

        public static bool CanDoInput()
        {
            if (_canDoInputThisFrame && Event.current.type == EventType.Repaint)
            {
                // Input fields, keyboardControl is 0 when no input field is active
                return GUIUtility.keyboardControl == 0;
            }

            return false;
        }
    }
}