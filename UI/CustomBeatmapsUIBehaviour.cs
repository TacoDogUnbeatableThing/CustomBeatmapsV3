﻿using System;
using System.Collections.Generic;
using CustomBeatmaps.Patches;
using CustomBeatmaps.UISystem;
using CustomBeatmaps.Util;
using DG.Tweening;
using UnityEngine;

namespace CustomBeatmaps.UI
{
    public class CustomBeatmapsUIBehaviour : MonoBehaviour
    {
        private readonly ReaccStore _store = new ReaccStore();
        private Vector2 _windowOffset;

        private static readonly float WindowPadding = 8;

        private int _releaseInputTimer;
        private bool _open;
        private readonly List<Exception> _errors = new List<Exception>();

        private void Awake()
        {
            EventBus.ExceptionThrown += e =>
            {
                _errors.Add(e);
            };
        }

        public void Open()
        {
            GUIHelper.AvoidInputOneFrame();
            _releaseInputTimer = 3;
            _open = true;
            DOTween.Kill(this);
            DOTween.To(() => _windowOffset, value => _windowOffset = value, Vector2.zero, 0.2f)
                .SetEase(Ease.OutBounce)
                .SetId(this);

            WhiteLabelMainMenuPatch.DisableBGM();
        }
        public void Close()
        {
            _releaseInputTimer = -1;
            DOTween.Kill(this);
            DOTween.To(() => _windowOffset, value => _windowOffset = value,
                new Vector2(-Screen.width, -1f * (float)Screen.height / 4f) // Looks better if we move more to the left when exiting
                , 0.5f)
                .SetEase(Ease.OutExpo)
                .SetId(this)
                .OnComplete(() =>
                {
                    _open = false;
                    WhiteLabelMainMenuPatch.EnableBGM();
                    WhiteLabelMainMenuPatch.StopSongPreview();
                    _windowOffset = new Vector2(-1 * Screen.width, -1 * Screen.height);
                });

        }

        private void Update()
        {
            // We do this to prevent first frame input from switching to this UI.
            // Janky ish but it's the easiest/most concise fix
            if (--_releaseInputTimer == 0)
            {
                GUIHelper.FreeInput();
                _releaseInputTimer = -1;
            }
        }

        private void OnGUI()
        {
            if (!_open)
                return;

            Reacc.SetStore(_store);

            float p = WindowPadding;
            float w = Screen.width - p * 2,
                h = Screen.height - p * 2;
            GUILayout.Window(Reacc.GetUniqueId(), new Rect(p + _windowOffset.x, p + _windowOffset.y, w, h), id =>
            {
                try
                {
                    // Main UI
                    CustomBeatmapsUI.Render();
                }
                catch (ArgumentException e)
                {
                    // Skip if we're just doing a "Getting Control" exception.
                    if (!e.Message.Contains("Getting control"))
                    {
                        _errors.Add(e);
                        throw;
                    }
                }

                // Extra Error UI
                ErrorUI.Render(_errors);
            }, "Custom Beatmaps v3");
        }
    }
}
