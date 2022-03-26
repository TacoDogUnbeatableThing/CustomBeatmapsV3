using System;
using System.Collections.Generic;
using CustomBeatmaps.UISystem;
using UnityEngine;

namespace CustomBeatmaps.UI
{
    public static class ErrorUI
    {
        public static void Render(List<Exception> exceptions)
        {
            var (windowRect, setWindowRect) = Reacc.UseState(() => new Rect(Screen.width/2 - 20, Screen.height/2 - 150, 400, 300));
            var (scrollPos, setScrollPos) = Reacc.UseState(Vector2.zero);
            var (open, setOpen) = Reacc.UseState(false);

            if (exceptions.Count != 0)
            {
                if (open)
                {
                    setWindowRect(GUI.Window(Reacc.GetUniqueId(), windowRect, windowId =>
                    {
                        // Make a very long rect that is 20 pixels tall.
                        // super long to allow for arbitrary size
                        GUI.DragWindow(new Rect(0, 0, 10000, 20));

                        setScrollPos(GUILayout.BeginScrollView(scrollPos));

                        foreach (var exception in exceptions)
                        {
                            GUILayout.TextField(exception.Message);
                        }

                        GUILayout.EndScrollView();

                    }, $" {exceptions.Count} + Errors"));
                }

                // Bottom left, Errors section
                string errorLabel = "";
                if (exceptions.Count != 0)
                {
                    errorLabel = $"⚠ x{exceptions.Count}";
                }
                if (GUI.Button(new Rect(4, 4, 64, 64), errorLabel))
                {
                    setOpen(!open);
                }
            }
        }
    }
}
