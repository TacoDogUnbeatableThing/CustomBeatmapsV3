using System;
using CustomBeatmaps.UISystem;
using UnityEngine;

namespace CustomBeatmaps.UI.PackageList
{
    public static class PackageInfoUI
    {
        public static void Render(Action onRenderTop, Action onRenderScroll, Action onRenderBottom)
        {
            var (scrollPos, setScrollPos) = Reacc.UseState(Vector2.zero);

            GUILayout.BeginVertical(GUILayout.MinWidth(400));
                onRenderTop();
                var newScrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.ExpandHeight(true));
                    onRenderScroll();
                GUILayout.EndScrollView();
                onRenderBottom();
                setScrollPos(newScrollPos);
            GUILayout.EndVertical();
        }
    }
}
