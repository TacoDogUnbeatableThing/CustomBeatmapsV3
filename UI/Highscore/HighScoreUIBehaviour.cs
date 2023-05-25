﻿using System;
using CustomBeatmaps.Util;
using UnityEngine;

namespace CustomBeatmaps.UI.Highscore
{
    public class HighScoreUIBehaviour : MonoBehaviour
    {
        public static bool Opened { get; private set; }
        private static bool Hide;

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
            Opened = true;
            _opened = true;
            _closing = false;
        }

        public void Close()
        {
            if (!_opened || _closing)
                return;
            Opened = false;
            // TODO: Zooom and set _opened (and _closing) to false then
            _opened = false;
            _closing = true;
        }

        public void Init(bool showLoginScreen, Func<string> getBeatmapKey)
        {
            _showLoginScreen = showLoginScreen;
            _getBeatmapKey = getBeatmapKey;
        }
        
        private void Awake()
        {
            int p = 8;
            _windowRect = new Rect(Screen.width - DEFAULT_WIDTH - p, Screen.height - DEFAULT_HEIGHT - p, DEFAULT_WIDTH,
                DEFAULT_HEIGHT);
        }
        private void OnDestroy()
        {
            Opened = false;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F5))
            {
                Hide = !Hide;
            }
        }

        private void OnGUI()
        {
            if (!_opened || Hide)
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

                    // Hide mode
                    GUILayout.Label("<size=12><color=gray>F5: Hide</color></size>");

                    GUILayout.EndVertical();

                }, "Online High Scores");
            }
        }
    }
}