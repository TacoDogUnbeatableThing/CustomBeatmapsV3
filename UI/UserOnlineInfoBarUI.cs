using System.Threading.Tasks;
using System.Timers;
using CustomBeatmaps.UISystem;
using CustomBeatmaps.Util;
using DG.Tweening;
using UnityEngine;

namespace CustomBeatmaps.UI
{
    public static class UserOnlineInfoBarUI
    {

        private static readonly Timer ShakeTimer = new Timer(6000);

        public static void Render()
        {
            var (shakeAmount, setShakeAmount) = Reacc.UseState(Vector3.zero);

            Reacc.UseEffect(() =>
            {
                ShakeTimer.AutoReset = true;
                ShakeTimer.Start();
                ShakeTimer.Elapsed += (sender, args) =>
                {
                    ScheduleHelper.SafeInvoke(() =>
                    {
                        // Shake!
                        DOTween.Shake(() => shakeAmount, val => setShakeAmount(val), 0.6f, Vector3.one * 8, 14);
                    });
                };
            });

            var topStyle = new GUIStyle(GUI.skin.window);
            GUILayout.BeginHorizontal(topStyle);

            var session = CustomBeatmaps.UserSession;
            if (session.LoggedIn)
            {
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                GUILayout.Label($"Logged in as {session.Username}");
                // Option to sync high scores to server lel
                var desyncedScores = CustomBeatmaps.ServerHighScoreManager.DesyncedHighScores;
                if (desyncedScores.Length != 0)
                {
                    GUILayout.Label($"(<b>{desyncedScores.Length} high scores not synced with server!</b>)");
                    if (GUILayout.Button($"Sync Scores!"))
                    {
                        CustomBeatmaps.ServerHighScoreManager.SyncUpHighScoresFromLocal();
                    }
                }

                NewVersionAvailableUI.Render();

                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Check out our Discord!", GUILayout.ExpandWidth(false)))
                {
                    Application.OpenURL(Config.Backend.DiscordInviteLink);
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                if (!session.LocalSessionExists())
                {
                    // Shake effect
                    var prevMat = GUI.matrix;
                    shakeAmount.z = 0;
                    GUI.matrix = Matrix4x4.Translate(shakeAmount) * GUI.matrix;

                    RenderRegisterScreen();

                    GUI.matrix = prevMat;
                } else if (session.LoginFailed)
                {
                    // Retry login?
                    if (GUILayout.Button("Retry Login"))
                    {
                        Task.Run(session.AttemptLogin);
                    }
                }
                // Show status (logging in/login failed/registration failed because XYZ) 
                GUILayout.Label(session.LoginStatus);
                NewVersionAvailableUI.Render();
            }

            GUILayout.EndHorizontal();
        }

        private static void RenderRegisterScreen()
        {
            var (username, setUsername) = Reacc.UseState("");

            GUILayout.Label("Submit Leaderboards by registering a unique username!", GUILayout.ExpandWidth(false));
            setUsername(GUILayout.TextField(username, GUILayout.Width(200)));

            if (GUILayout.Button("REGISTER", GUILayout.ExpandWidth(false)))
            {
                Task.Run(() => CustomBeatmaps.UserSession.RegisterNewUserSession(username));
            }
        }
    }
}
