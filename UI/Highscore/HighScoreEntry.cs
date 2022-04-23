using System;
using System.Text.RegularExpressions;
using CustomBeatmaps.Util;
using UnityEngine;

namespace CustomBeatmaps.UI.Highscore
{
    public static class HighScoreEntry
    {
        private static readonly int AccuracyRightPad = 64 + 128;
        private static readonly int ScoreRightPad = 64;
        private static readonly int RankLeftPad = 16;
        private static readonly int GradeLeftPad = 32;
        private static readonly int NameLeftPad = 48 + 16;
        private static readonly int FcModeLeftPad = 256 + 16;
        public static void Render(int rank, string player, BeatmapHighScoreEntry entry)
        {
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(24));
            var r = GUILayoutUtility.GetLastRect();

            var lastLabelStyle = GUI.skin.label;
            var rightPadStyle = new GUIStyle(lastLabelStyle);
            rightPadStyle.alignment = TextAnchor.MiddleRight;

            string fcModeLabel;
            switch (entry.FullComboMode)
            {
                case 1:
                    fcModeLabel = "<b>NO MISS</b>";
                    break;
                case 2:
                    fcModeLabel = "<i><b><size=18>FC!</size></b></i>";
                    break;
                default:
                    fcModeLabel = "";
                    break;
            }

            // Read state
            var pfail = JeffBezosController.prevFail;
            JeffBezosController.prevFail = false; // This messes with score. I love imperative programming.
            string grade = CleanUpGrade(HighScoreScreen.GetLetterGrade(entry.Accuracy, entry.FullComboMode > 0));
            string gradeWithColor = $"<color={GradeToColor(grade)}>{grade}</color>";
            JeffBezosController.prevFail = pfail;
            if (CustomBeatmaps.UserSession.LoggedIn && CustomBeatmaps.UserSession.Username == player)
            {
                // Highlight ourselves
                player = $"<color=yellow><b>{player}</b></color>";
            }

            int minL = FcModeLeftPad + 64;
            bool tooSmall = r.width - AccuracyRightPad < minL;


            // I love manual adjustment of padding
            // (note: GUILayout.BeginArea wasn't working)

            int fcModeLeftPad = FcModeLeftPad;
            if (tooSmall)
            {
                fcModeLeftPad = (int)(r.width - AccuracyRightPad - 64);
            }

            GUI.Label(new Rect(r.x, r.y - 2, RankLeftPad, r.height), $"<b>{rank.ToString()}</b>", rightPadStyle);
            GUI.Label(Oxl(r, GradeLeftPad), gradeWithColor, lastLabelStyle);
            GUI.Label(Oxl(r, NameLeftPad), player, lastLabelStyle);
            GUI.Label(Oxl(r, fcModeLeftPad), fcModeLabel, lastLabelStyle);


            // Make sure the stuff on the right isn't overlapping...
            int accuracyRightPad = AccuracyRightPad;
            int scoreRightPad = ScoreRightPad;
            if (tooSmall)
            {
                scoreRightPad = 64;
                accuracyRightPad = scoreRightPad + 64;
            }

            GUI.Label(Oxr(r, accuracyRightPad), $"{entry.Accuracy * 100:0.00}%", lastLabelStyle);
            GUI.Label(Oxr(r, scoreRightPad), entry.Score.ToString(), lastLabelStyle);

            GUI.skin.label = lastLabelStyle;
        }

        private static Rect Oxl(in Rect r, int dx)
        {
            return new Rect(r.x + dx, r.y, r.width - dx, r.height);
        }

        private static Rect Oxr(in Rect r, int dx)
        {
            return new Rect(r.x + r.width - dx, r.y, dx, r.height);
        }

        private static string CleanUpGrade(string grade)
        {
            string pattern = "(.*<size=)(\\d+)%(.*>.*)";
            return Regex.Replace(grade, pattern, m =>
            {
                if (m.Groups.Count > 1)
                {
                    float percent = float.Parse(m.Groups[2].Value);
                    return $"{m.Groups[1]}{PercentToPx(percent)}{m.Groups[3]}</size>";
                }

                return m.Groups[0].ToString();
            });
        }
        private static int PercentToPx(float percent)
        {
            return Math.Max(4, (int)((percent / 100) * 20));
        }
        
        private static string GradeToColor(string grade)
        {
            if (grade.Contains("S++"))
                return "#ff7997ff";
            if (grade.Contains("S+"))
                return "#ff9c7aff";
            if (grade.Contains("S"))
                return "#7affffff";
            if (grade.Contains("A"))
                return "#7aff7aff";
            if (grade.Contains("B"))
                return "#33cc33ff";
            if (grade.Contains("C") || grade.Contains("D"))
                return "#ffff99ff";
            if (grade.Contains("HOW"))
                return "#0000ccff";
            return "ffffffff";
        }
    }
}