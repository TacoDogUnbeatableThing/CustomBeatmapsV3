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
            if (!UnbeatableHelper.UsingHighScoreProhibitedAssists() && CustomBeatmaps.ServerHighScoreManager.CanSendScoreForCurrentMap)
            {
                if (JeffBezosController.prevFail)
                {
                    Debug.Log("(High Score: Failed, no update)");
                    return;
                }
                int score = JeffBezosController.prevScore;
                float accuracy = JeffBezosController.prevAccuracy;
                bool noMiss = JeffBezosController.prevMiss == 0;
                bool fc = noMiss && JeffBezosController.prevBarely == 0;

                // beatmap key is set in UnbeatableHelper, from the server UI
                // this is a _bit_ of spaghetti, but I'm nearing the limit of how complex this project will be so it's good enough.
                CustomBeatmaps.ServerHighScoreManager.SendScore(CustomBeatmaps.ServerHighScoreManager.CurrentBeatmapKey, score, accuracy, noMiss, fc);
            }
            else
            {
                if (UnbeatableHelper.UsingHighScoreProhibitedAssists())
                {
                    Debug.Log("(High Score: Using assists, won't send)");
                }
                else
                {
                    Debug.Log("(High Score: Server High Score Manager can't send)");
                }
            }
        }
    }
}
