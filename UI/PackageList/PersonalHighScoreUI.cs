using CustomBeatmaps.UISystem;
using UnityEngine;

namespace CustomBeatmaps.UI.PackageList
{
    public static class PersonalHighScoreUI
    {
        public static void Render(string beatmapPath)
        {
            // Kinda copied this over... Why not use UseEffect or something?
            (HighScoreList highScores, _) =
                Reacc.UseState(() => HighScoreScreen.LoadHighScores("wl-highscores"));

            var score = highScores.GetScoreItem(beatmapPath);
            GUILayout.Label($"<color=green>{score.score:00000000}</color> PTS, <color=blue>{(score.accuracy*100, 0.00)}%</color>");
        }
    }
}
