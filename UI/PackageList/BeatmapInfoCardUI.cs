using UnityEngine;

namespace CustomBeatmaps.UI.PackageList
{
    public static class BeatmapInfoCardUI
    {
        public static void Render(BeatmapHeader beatmapHeader)
        {
            var cardStyle = new GUIStyle(GUI.skin.box);
            var m = cardStyle.margin;
            var padH = 16;
            cardStyle.margin = new RectOffset(m.left + padH, m.right + padH, m.top, m.bottom);
            GUILayout.BeginHorizontal(cardStyle);
            // TODO: Icon if provided! For fun!
                GUILayout.BeginVertical();
                    GUILayout.Label($"<b>{beatmapHeader.Name}</b>");
                    GUILayout.Label($"by <b>{beatmapHeader.Artist}</b>");
                GUILayout.EndVertical();

                GUILayout.FlexibleSpace();

                GUILayout.BeginVertical();
                    GUILayout.Label($"{beatmapHeader.Difficulty}");
                    GUILayout.Label($"mapper: {beatmapHeader.Creator}");
                GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
    }
}