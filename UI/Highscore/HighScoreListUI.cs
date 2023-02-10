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

            int highScoreCount = highScores?.Count ?? 0;
            int userHighPlace = -1;

            if (CustomBeatmaps.UserSession.LoggedIn)
            {
                var username = CustomBeatmaps.UserSession.Username;
                if (highScores != null)
                    userHighPlace = highScores.FindIndex(place => place.Key == username);
            }

            // TODO (for fun) Color code 1st 2nd and 3rd place (gold, silver, bronze)
            string highScoreRankLabel = userHighPlace != -1 ? $"Ranked {userHighPlace + 1} / {highScoreCount}" : $"{highScoreCount}";
            string highScoreLabel = $"HIGH SCORES <size=16>({highScoreRankLabel})</size>";

            GUILayout.Label($"<size=24>{highScoreLabel}</size>");
            if (!RenderScores(highScores, 0))
            {
                if (CustomBeatmaps.ServerHighScoreManager.Failure != null)
                {
                    GUILayout.Label($"Load Failed: {CustomBeatmaps.ServerHighScoreManager.Failure}");
                }
                else
                {
                    GUILayout.Label("Can't find High Scores...");
                }
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