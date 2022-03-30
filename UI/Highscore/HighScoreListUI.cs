using System.Collections.Generic;
using CustomBeatmaps.UISystem;
using CustomBeatmaps.Util;
using UnityEngine;

namespace CustomBeatmaps.UI.Highscore
{
    public static class HighScoreListUI
    {
        public static void Render(string beatmapKey)
        {
            var highScores = CustomBeatmaps.ServerHighScoreManager.GetHighScores(beatmapKey);
            var lowScores = CustomBeatmaps.ServerHighScoreManager.GetLowScores(beatmapKey);

            GUILayout.Label("<size=32>HIGH SCORES</size>");
            if (!RenderScores(highScores, 0))
            {
                GUILayout.Label("Can't find High Scores...");
            }
            GUILayout.Label("<size=32>LOW BALLERS</size>");
            if (!RenderScores(lowScores, 1))
            {
                GUILayout.Label("Can't find Low Scores...");
            }
        }

        private static bool RenderScores(List<KeyValuePair<string, BeatmapHighScoreEntry>> scores, int uniqueLineNumber)
        {
            var (scroll, setScroll) = Reacc.UseState(Vector2.zero, uniqueLineNumber);
            if (scores != null)
            {
                setScroll(GUILayout.BeginScrollView(scroll));
                if (scores.Count == 0)
                {
                    GUILayout.Label("(Be the first to score!)");
                }
                else
                {
                    int rank = 1;
                    foreach (var scoreEntry in scores)
                    {
                        HighScoreEntry.Render(rank, scoreEntry.Key, scoreEntry.Value);
                        ++rank;
                    }
                }
                GUILayout.EndScrollView();
                return true;
            }
            return false;
        }
    }
}