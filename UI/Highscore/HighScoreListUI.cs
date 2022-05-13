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

            int highScoreCount = highScores?.Count ?? 0;
            int lowScoreCount = lowScores?.Count ?? 0;
            int userHighPlace = -1,
                userLowPlace = -1;

            if (CustomBeatmaps.UserSession.LoggedIn)
            {
                var username = CustomBeatmaps.UserSession.Username;
                if (highScores != null)
                    userHighPlace = highScores.FindIndex(place => place.Key == username);
                if (lowScores != null)
                    userLowPlace = lowScores.FindIndex(place => place.Key == username);
            }

            // TODO (for fun) Color code 1st 2nd and 3rd place (gold, silver, bronze)
            string highScoreRankLabel = userHighPlace != -1 ? $"Ranked {userHighPlace + 1} / {highScoreCount}" : $"{highScoreCount}";
            string lowScoreRankLabel = userLowPlace != -1 ? $"Ranked {userLowPlace + 1} / {lowScoreCount}" : $"{lowScoreCount}";
            string highScoreLabel = $"HIGH SCORES <size=16>({highScoreRankLabel})</size>";
            string lowScoreLabel = $"LOW BALLERS <size=16>({lowScoreRankLabel})</size>";

            GUILayout.Label($"<size=24>{highScoreLabel}</size>");
            if (!RenderScores(highScores, 0))
            {
                GUILayout.Label("Can't find High Scores...");
            }
            GUILayout.Label($"<size=24>{lowScoreLabel}</size>");
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