using System;
using CustomBeatmaps.Util;
using UnityEngine;

namespace CustomBeatmaps.UI
{
    public static class NewVersionAvailableUI
    {
        private static Version _newVersion = null;
        private static bool _closed = false;

        static NewVersionAvailableUI()
        {
            var modVersion = VersionHelper.GetModVersion();
            VersionHelper.GetOnlineLatestReleaseVersion().ContinueWith(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    var onlineVersion = task.Result;
                    ScheduleHelper.SafeLog($"FOUND ONLINE VERSION: {onlineVersion}");
                    if (onlineVersion > modVersion)
                    {
                        _newVersion = onlineVersion;
                    }
                } else if (task.Exception != null)
                {
                    EventBus.ExceptionThrown?.Invoke(task.Exception);
                }
            });
        }
        public static void Render()
        {
            if (_newVersion != null && !_closed)
            {
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                if (GUILayout.Button($"<color=lime><b>NEW VERSION AVAILABLE!</b> ({_newVersion})</color>", GUILayout.ExpandWidth(false)))
                {
                    Application.OpenURL(Config.Backend.DownloadLatestReleaseLink);
                }

                /*
                if (GUILayout.Button("x", GUILayout.ExpandWidth(false)))
                {
                    _closed = true;
                }
                */

                GUILayout.EndHorizontal();
            }
        }
    }
}