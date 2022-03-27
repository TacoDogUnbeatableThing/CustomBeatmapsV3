using System;
using CustomBeatmaps.Patches;
using UnityEngine;

namespace CustomBeatmaps.UI.OSUEditMode
{
    public static class OSUEditUI
    {
        public static void Render()
        {
            int w = 64;
            int h = 128;
            int p = 8;
            GUILayout.Window(0, new Rect(p, Screen.height - h - p, w, h), id =>
            {
                OsuEditorPatch.AutoReload = GUILayout.Toggle(OsuEditorPatch.AutoReload, "Auto Reload");
                GUILayout.Label("Space: Pause");
                GUILayout.Label("R: Reload");
            }, "Tools");
            float total = OsuEditorPatch.SongTotalMS;
            float progress = total > 0? (OsuEditorPatch.SongTimeMS / total) : -1;
            RenderTimer(OsuEditorPatch.SongTimeMS, progress);
        }

        private static void RenderTimer(int totalMS, float progressBarProgress=-1)
        {
            int ms = totalMS % 1000;
            int seconds = (totalMS / 1000) % 60;
            int minutes = (totalMS / (1000 * 60));

            int w = (Screen.width / 3);
            int h = Math.Min(w / 2, 64);
            int p = 24;
            var originalLabelStyle = GUI.skin.GetStyle("Label"); 
            var centeredStyle = new GUIStyle(originalLabelStyle);
            centeredStyle.alignment = TextAnchor.MiddleRight;
            var r = new Rect(Screen.width - w - p, Screen.height - h - p, w, h);
            var t = $"<b>{minutes:00}:{seconds:00}:{ms:0000}</b>";
            GUI.Label(new Rect(r.position + Vector2.right * 3, r.size), $"<color=black><size={h + 3}>{t}</size></color>", centeredStyle);
            GUI.Label(r, $"<size={h}>{t}</size>", centeredStyle);
            GUI.skin.label = originalLabelStyle;

            if (progressBarProgress >= 0)
                RenderProgressBar(new Rect(r.x, r.y - 16, r.width, 16), progressBarProgress);
        }

        private static void RenderProgressBar(Rect rect, float progress)
        {
            GUI.HorizontalSlider(rect, progress, 0, 1);
        }
    }
}
