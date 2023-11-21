using System;
using UnityEngine;

namespace CustomBeatmaps.UI
{
    public class OpeningDisclaimerUIBehaviour : MonoBehaviour
    {
        private static readonly string Message = "<size=64>PLEASE PLEASE READ</size>\n\n" +
                                                 "<size=18>CustomBeatmapsV3 is an <b>UNOFFICIAL MOD</b> that adds extra features to the game.\n" +
                                                 "It is NOT endorsed or recognized by D-Cell Games.\n" +
                                                 "\n\n" +
                                                 "Please AVOID posting screenshots of the mod/high scores table in the official server." +
                                                 "\n\n" +
                                                 "If you need help or have found a bug with this mod, please do NOT bug D-Cell and ask in our Discord instead.\n\n" +
                                                 "Also, if you want to report UNBEATABLE [white label] related bugs/feedback in the D-Cell discord,\n" +
                                                 "please make sure it is NOT the mod " +
                                                 "that is causing this problem, else you will be wasting D-Cell's time.</size>";

        private bool _acceptNoModBugReport;
        private bool _acceptNoHighScorePosting;

        public Action OnSelect;

        private void OnGUI()
        {
            // Black background
            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), Texture2D.blackTexture);

            float p = 64;
            GUILayout.BeginArea(new Rect(p, p, Screen.width - p*2, Screen.height - p*2));
                GUILayout.Label(Message, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
                DiscordButtonUI.Render("Visit our discord here for help");
                GUILayout.FlexibleSpace();
                _acceptNoModBugReport = GUILayout.Toggle(_acceptNoModBugReport,
                    "I will NOT submit any bug reports to D-Cell while using this mod");
                _acceptNoHighScorePosting = GUILayout.Toggle(_acceptNoHighScorePosting,
                    "I will AVOID posting screenshots with high scores in the D-Cell discord");
                if (_acceptNoModBugReport && _acceptNoHighScorePosting)
                {
                    if (GUILayout.Button("<size=16>PROCEED</size>", GUILayout.ExpandWidth(false)))
                    {
                        OnSelect?.Invoke();
                    }
                }
            GUILayout.EndArea();

        }
    }
}
