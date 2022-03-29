using System;
using CustomBeatmaps.Util;
using UnityEngine;

namespace CustomBeatmaps.UI.Highscore
{
    public class HighScoreUIBehaviour : MonoBehaviour
    {
        private bool _showLoginScreen;
        private Func<string> _getBeatmapKey;

        private Rect _windowRect;

        private static readonly int DEFAULT_WIDTH = 480;
        private static readonly int DEFAULT_HEIGHT = 512;

        private bool _opened;
        private bool _closing;

        public void Open()
        {
            if (_opened)
                return;
            ScheduleHelper.SafeLog("OPENED");
            _opened = true;
            _closing = false;
        }

        public void Close()
        {
            if (!_opened || _closing)
                return;
            // TODO: Zooom and set _opened (and _closing) to false then
            _opened = false;
            _closing = true;
        }

        public void Init(bool showLoginScreen, Func<string> getBeatmapKey)
        {
            _showLoginScreen = showLoginScreen;
            _getBeatmapKey = getBeatmapKey;
        }
        
        private void Start()
        {
            int p = 8;
            _windowRect = new Rect(Screen.width - DEFAULT_WIDTH - p, Screen.height - DEFAULT_HEIGHT - p, DEFAULT_WIDTH,
                DEFAULT_HEIGHT);
        }

        private void OnGUI()
        {
            if (!_opened)
                return;

            if (CustomBeatmaps.ServerHighScoreManager.Loaded)
            {
                _windowRect = GUI.Window(0, _windowRect, windowId =>
                {
                    // Make a very long rect that is 20 pixels tall.
                    // super long to allow for arbitrary size
                    GUI.DragWindow(new Rect(0, 0, 10000, 20));

                    GUILayout.BeginVertical();

                    if (_showLoginScreen)
                    {
                        UserOnlineInfoBarUI.Render();
                    }

                    string beatmapScoreKey = _getBeatmapKey != null? _getBeatmapKey.Invoke() : null;
                    if (beatmapScoreKey == null)
                    {
                        GUILayout.Label("(No score key found, something went wrong)");
                    }
                    else
                    {
                        HighScoreListUI.Render(beatmapScoreKey);
                    }

                    GUILayout.EndVertical();

                }, "Online High Scores");
            }
        }
    }
}