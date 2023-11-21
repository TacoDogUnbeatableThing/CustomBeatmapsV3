using System;
using UnityEngine;

namespace CustomBeatmaps.UI
{
    public static class Searchbar
    {
        public static void Render(string text, Action<string> setText)
        {
            var newText = GUILayout.TextArea(text);
            if (newText != text)
            {
                setText?.Invoke(newText);
            }
        }
    }
}