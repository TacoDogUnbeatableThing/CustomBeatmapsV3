using UnityEngine;

namespace CustomBeatmaps.UI
{
    public static class ProgressBarUI
    {
        public static void Render(float percent, string text, params GUILayoutOption[] options)
        {
            // BG Rect
            GUILayout.Box("", options);
            // Progress fill rect
            var r = GUILayoutUtility.GetLastRect();
            var fillRect = new Rect(r.x, r.y, r.width * percent, r.height);
            GUI.Box(fillRect, Texture2D.whiteTexture);
            // Text
            GUI.Label(r, text);
        }
    }
}
