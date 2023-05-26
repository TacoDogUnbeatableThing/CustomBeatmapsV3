using System;
using System.Collections.Generic;
using CustomBeatmaps.UISystem;
using CustomBeatmaps.Util;
using Rewired;
using UnityEngine;

namespace CustomBeatmaps.UI.PackageList
{
    public static class PackageListUI
    {
        private static Player _rewired;

        public static void Render(string header, List<PackageHeader> packageHeaders, int selectedHeaderIndex, Action<int> onPackageHeaderSelect)
        {
            var (scrollPos, setScrollPos) = Reacc.UseState(Vector2.zero);
            var (prevSelectedHeaderIndex, setPrevSelectedHeaderIndex) = Reacc.UseState(-1);

            Reacc.UseEffect(() =>
            {
                _rewired = ReInput.players.GetPlayer(0);
                // The game does not use/set up these parameters so I set them up here
                _rewired.controllers.maps.GetInputBehavior(0).buttonRepeatDelay = 0.5f;
                _rewired.controllers.maps.GetInputBehavior(0).buttonRepeatRate = 8;
            });

            GUILayout.Label(header);
            var curScrollPos = GUILayout.BeginScrollView(scrollPos);
                bool anySelected = false;
                Rect selectedRect = Rect.zero;
                Rect firstRect = Rect.zero;
                for (int i = 0; i < packageHeaders.Count; ++i)
                {
                    var packageHeader = packageHeaders[i];
                    int indexToSelect = i;
                    bool selected = selectedHeaderIndex == i;
                    Rect r = PackageEntryUI.Render(packageHeader, selected, () =>
                    {
                        onPackageHeaderSelect?.Invoke(indexToSelect);
                    }, packageHeader.DownloadStatus);

                    if (i == 0)
                    {
                        firstRect = r;
                    }
                    if (selected)
                    {
                        anySelected = true;
                        selectedRect = r;
                    }
                }
            GUILayout.EndScrollView();
            Rect scrollViewRect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint)
            {
                // Make sure our scroll pos encapsulates the rect
                if (anySelected && prevSelectedHeaderIndex != selectedHeaderIndex)
                {
                    float viewMinY = firstRect.yMin + curScrollPos.y;
                    float viewMaxY = firstRect.yMin + scrollViewRect.height + curScrollPos.y;
                    float deltaY = 0;
                    if (selectedRect.yMax < viewMinY)
                    {
                        deltaY = selectedRect.yMin - viewMinY;
                    } else if (selectedRect.yMin > viewMaxY)
                    {
                        deltaY = selectedRect.yMax - viewMaxY;
                    }

                    curScrollPos.y += deltaY;
                }

                setPrevSelectedHeaderIndex(selectedHeaderIndex);   
            }
            setScrollPos(curScrollPos);


            // Keyboard Shortcuts: Navigate packages with arrow keys

            if (GUIHelper.CanDoInput() && packageHeaders.Count > 1)
            {
                if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S) || _rewired.GetNegativeButtonRepeating("Vertical"))
                {
                    onPackageHeaderSelect((selectedHeaderIndex + 1) % packageHeaders.Count);
                }
                else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) || _rewired.GetButtonRepeating("Vertical"))
                {
                    int ind = selectedHeaderIndex - 1;
                    if (ind < 0)
                        ind = packageHeaders.Count - 1;
                    onPackageHeaderSelect(ind);
                }
            }
        }
    }
}
