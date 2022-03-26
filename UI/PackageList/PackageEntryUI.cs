using System;
using CustomBeatmaps.CustomPackages;
using UnityEngine;

namespace CustomBeatmaps.UI.PackageList
{
    public static class PackageEntryUI
    {
        private static readonly float MapCountRightPad = 64;
        private static readonly float SongCountRightPad = 128;
        private static readonly float NewTextRightPad = 128 + 64;
        private static readonly float DownloadStatusRightPad = 128 + 128 + 32;

        private static string GetCount(int count, string unitSingular)
        {
            bool singular = count == 1;
            return singular ? $"{count} {unitSingular}" : $"{count} {unitSingular}s";
        }

        public static void Render(PackageHeader header, bool selected, Action onSelect, BeatmapDownloadStatus status=BeatmapDownloadStatus.Downloaded)
        {
            string label = $"{header.Name}";
            if (selected)
            {
                label = $"<b><color=#fbff8fff>{label}</color></b>";
            }

            label += $" <i>by {header.Creator}</i>";

            /*
            GUIStyle withRichText = GUI.skin.button;
            withRichText.padding.left = 8; // Left side
            withRichText.richText = true;
            //withRichText.fixedHeight = 32;
            */

            if (GUILayout.Button("", /*withRichText,*/ GUILayout.ExpandWidth(true), GUILayout.Height(32)))
            {
                onSelect?.Invoke();
            }

            // button rect for Text + Map + Song + New count
            Rect br = GUILayoutUtility.GetLastRect();

            GUI.Label(new Rect(br.min + Vector2.right * 16, br.size), label);
            GUI.Label(new Rect(br.xMax - MapCountRightPad, br.y, MapCountRightPad, br.height), $"{GetCount(header.MapCount, "Map")}");
            GUI.Label(new Rect(br.xMax - SongCountRightPad, br.y, SongCountRightPad, br.height), $"{GetCount(header.SongCount, "Song")}");
            if (header.New)
            {
                GUI.Label(new Rect(br.xMax - NewTextRightPad, br.y, NewTextRightPad, br.height), "<color=#ffff00ff><i>NEW!</i></color>");
            }

            if (status != BeatmapDownloadStatus.Downloaded)
            {
                string statusLabel = "";
                switch (status)
                {
                    case BeatmapDownloadStatus.Queued:
                        statusLabel = "<color=orange>QUEUED</color>";
                        break;
                    case BeatmapDownloadStatus.CurrentlyDownloading:
                        statusLabel = "<color=blue>Downloading...</color>";
                        break;
                    case BeatmapDownloadStatus.NotDownloaded:
                        statusLabel = "<color=gray><i>Online</i></color>";
                        break;
                }
                GUI.Label(new Rect(br.xMax - DownloadStatusRightPad, br.y, DownloadStatusRightPad, br.height), statusLabel);
            }
        }
    }
}