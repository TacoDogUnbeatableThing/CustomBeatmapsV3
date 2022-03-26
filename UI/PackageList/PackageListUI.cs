using System;
using System.Collections.Generic;
using CustomBeatmaps.UISystem;
using CustomBeatmaps.Util;
using UnityEngine;

namespace CustomBeatmaps.UI.PackageList
{
    public static class PackageListUI
    {
        public static void Render(string header, List<PackageHeader> packageHeaders, int selectedPackageIndex, Action<int> onPackageSelect)
        {
            var (scrollPos, setScrollPos) = Reacc.UseState(Vector2.zero);

            GUILayout.Label(header);
            var curScrollPos = GUILayout.BeginScrollView(scrollPos);
                for (int i = 0; i < packageHeaders.Count; ++i)
                {
                    var packageHeader = packageHeaders[i];
                    int indexToSelect = i;
                    PackageEntryUI.Render(packageHeader, selectedPackageIndex == i, () =>
                    {
                        onPackageSelect?.Invoke(indexToSelect);
                    }, packageHeader.DownloadStatus);
                }
            GUILayout.EndScrollView();

            setScrollPos(curScrollPos);

            // Keyboard Shortcuts: Navigate packages with arrow keys

            if (GUIHelper.CanDoInput() && packageHeaders.Count > 1)
            {
                if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
                {
                    onPackageSelect((selectedPackageIndex + 1) % packageHeaders.Count);
                }
                else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
                {
                    int ind = selectedPackageIndex - 1;
                    if (ind < 0)
                        ind = packageHeaders.Count - 1;
                    onPackageSelect(ind);
                }
            }
        }
    }
}
