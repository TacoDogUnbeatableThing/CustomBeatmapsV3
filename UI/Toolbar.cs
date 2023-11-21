using System.Linq;
using UnityEngine;

namespace CustomBeatmaps.UI
{
    public static class Toolbar
    {
        public static int Render(int selected, GUIContent[] contents)
        {
            for (int i = 0; i < contents.Length; ++i)
            {
                var s = new GUIStyle(GUI.skin.button);
                if (selected == i)
                {
                    var selmode = s.active;
                    // selmode.textColor = Color.yellow;
                    // selmode.background = Texture2D.grayTexture;
                    s.normal = selmode;
                    s.onNormal = selmode;
                    s.focused = selmode;
                    s.onFocused = selmode;
                    s.hover = selmode;
                    s.onHover = selmode;
                }
                if (GUILayout.Button(contents[i], s))
                {
                    return i;
                }
            }
            return -1;
        }
        public static int Render(int selected, string[] contents)
        {
            return Render(selected, contents.Select(c => new GUIContent(c)).ToArray());
        }
    }
}