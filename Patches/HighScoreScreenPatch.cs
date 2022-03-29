﻿using CustomBeatmaps.UI.Highscore;
using CustomBeatmaps.Util;
using HarmonyLib;
using UnityEngine;

namespace CustomBeatmaps.Patches
{
    public static class HighScoreScreenPatch
    {
        [HarmonyPatch(typeof(HighScoreScreen), "Start")]
        [HarmonyPostfix]
        private static void OnHighScoreScreenOpened()
        {
            // SEND SCORE
    
            bool useCustomBeatmap = CustomBeatmapLoadingOverridePatch.CustomBeatmapSet();
            string beatmapScoreKey;
            if (useCustomBeatmap)
            {
                // custom map
                beatmapScoreKey = CustomBeatmaps.ServerHighScoreManager.CanSendScoreForCurrentMap?
                    CustomBeatmaps.ServerHighScoreManager.CurrentBeatmapKey
                    : null;
            }
            else
            {
                // white label map
                beatmapScoreKey =
                    UserServerHelper.GetHighScoreBeatmapKeyFromUnbeatableBeatmap(JeffBezosController.rhythmProgression
                        .GetBeatmapPath());
            }

            bool canHaveHighScore = beatmapScoreKey != null;
            if (!UnbeatableHelper.UsingHighScoreProhibitedAssists() && canHaveHighScore)
            {
                if (JeffBezosController.prevFail)
                {
                    Debug.Log("(High Score: Failed, no update)");
                }
                else
                {
                    int score = JeffBezosController.prevScore;
                    float accuracy = JeffBezosController.prevAccuracy;
                    bool noMiss = JeffBezosController.prevMiss == 0;
                    bool fc = noMiss && JeffBezosController.prevBarely == 0;

                    // beatmap key is set in UnbeatableHelper, from the server UI
                    // this is a _bit_ of spaghetti, but I'm nearing the limit of how complex this project will be so it's good enough.
                    CustomBeatmaps.ServerHighScoreManager.SendScore(
                        beatmapScoreKey, score, accuracy, noMiss, fc);
                }
            }
            else
            {
                if (UnbeatableHelper.UsingHighScoreProhibitedAssists())
                {
                    Debug.Log("(High Score: Using assists, won't send)");
                }
                else
                {
                    Debug.Log("(High Score: No custom beatmap high score key given or found!)");
                }
            }
            
            // EXTRA SERVER HIGH SCORE UI

            if (canHaveHighScore)
            {
                var highScoreUI = new GameObject().AddComponent<HighScoreUIBehaviour>();
                highScoreUI.Init(false, () => beatmapScoreKey);
                highScoreUI.Open();
            }
        }
    }
}
