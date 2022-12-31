using System;
using System.Linq;
using System.Reflection;
using CustomBeatmaps.CustomPackages;
using CustomBeatmaps.Patches;
using Rhythm;
using TMPro;
using UnityEngine;

namespace CustomBeatmaps.Util
{
    public static class UnbeatableHelper
    {
        public static readonly CustomBeatmapRoom[] Rooms = {
            new CustomBeatmapRoom("Default", "TrainStationRhythm"),
            new CustomBeatmapRoom("Practice Room", "PracticeRoomRhythm"),
            new CustomBeatmapRoom("NSR", "NSR_Stage"),
            new CustomBeatmapRoom("Tutorial", "Tutorial")
            // This one would be interesting but we already have the tutorial screen
            //new CustomBeatmapRoom("Offset Wizard", "OffsetWizard")
        };
        private static readonly string DefaultBeatmapScene = "TrainStationRhythm";

        public static readonly string DefaultScoreScene = "ScoreScreenArcade";

        public static readonly string LevelSelectScene = "TrainLevelSelect";

        public static string GetSceneNameByIndex(int index)
        {
            if (index < 0 || index >= Rooms.Length)
            {
                return DefaultBeatmapScene;
            }

            return Rooms[index].SceneName;
        }
        
        private static void PlayBeatmapinternal(CustomBeatmapInfo beatmap, string sceneName)
        {
            CustomBeatmapLoadingOverridePatch.SetOverrideBeatmap(beatmap);
            LevelManager.LoadLevel(sceneName);
            JeffBezosController.rhythmProgression = new DefaultProgression(beatmap.OsuPath, sceneName);
        }
        
        public static void PlayBeatmap(CustomBeatmapInfo beatmap, bool registerHighScores, string sceneName)
        {
            if (registerHighScores)
            {
                string beatmapKey = UserServerHelper.GetHighScoreBeatmapKeyFromLocalBeatmap(Config.Mod.ServerPackagesDir, beatmap.OsuPath);
                CustomBeatmaps.ServerHighScoreManager.SetCurrentBeatmapKey(beatmapKey);
            }
            else
            {
                CustomBeatmaps.ServerHighScoreManager.ResetCurrentBeatmapKey();
            }
            OsuEditorPatch.SetEditMode(false);
            PlayBeatmapinternal(beatmap, sceneName);
        }

        public static void PlayBeatmapEdit(CustomBeatmapInfo beatmap, bool enableCountdown=false)
        {
            OsuEditorPatch.SetEditMode(true, enableCountdown, beatmap.OsuPath);
            PlayBeatmapinternal(beatmap, DefaultBeatmapScene);
        }

        public static bool UsingHighScoreProhibitedAssists()
        {
            // We include flip mode because _potentially_ it might be used to make high notes easier to hit?
            return (JeffBezosController.GetAssistMode() == 1) || GetSongSpeed(JeffBezosController.GetSongSpeed()) < 1 || (JeffBezosController.GetNoFail() == 1) || CustomBeatmaps.Memory.FlipMode;
        }

        public static float GetSongSpeed(int songSpeedIndex)
        {
            switch (songSpeedIndex)
            {
                case 0:
                    return 1f;
                case 1:
                    return 0.5f;
                case 2:
                    return 2f;
                default:
                    throw new InvalidOperationException($"Invalid song speed index: {songSpeedIndex}");
            }
        }

        public static HighScoreList LoadWhiteLabelHighscores()
        {
           return HighScoreScreen.LoadHighScores("wl-highscores");
        }

        /// <returns> whether <code>potentialSongPath</code> is in the format "[UNBEATABLE Song]/[DIFFICULTY] </returns>
        public static bool IsValidUnbeatableSongPath(string potentialSongPath)
        {
            var beatmapIndex = Rhythm.BeatmapIndex.defaultIndex;
            var whiteLabelSongs = beatmapIndex.SongNames;

            int lastDashIndex = potentialSongPath.LastIndexOf("/", StringComparison.Ordinal);
            if (lastDashIndex != -1)
            {
                // Also check to make sure it's a valid UNBEATABLE song
                string songName = potentialSongPath.Substring(0, lastDashIndex);
                return whiteLabelSongs.Contains(songName);
            }

            return false;
        }

        private static readonly Vector3 MenuOffsetPerNewOption = new Vector3( 0.17f - 0.21f, 0.015f, 0.82f - 0.75f);
        private static readonly FieldInfo UINavigatorRowField = typeof(UINavigator).GetField("row", BindingFlags.Instance | BindingFlags.NonPublic);
        public static UISelectionCycle InsertBeatmapOptionInMenu(BeatmapOptionsMenu menu, Func<int, int> indexGivenSize, string name)
        {
            var navigator = menu.navigator;
            var rows = navigator.columns[0].rows;

            int index = indexGivenSize.Invoke(rows.Count);

            // Find the thing to copy 
            var titleToCopy = menu.transform.GetChild(Math.Min(index*2, menu.transform.childCount - 2));
            var optionToCopy = menu.transform.GetChild(Math.Min(index*2, menu.transform.childCount - 2) + 1);
            var newTitle = UnityEngine.Object.Instantiate(titleToCopy, titleToCopy.parent, true);
            var newOption = UnityEngine.Object.Instantiate(optionToCopy, optionToCopy.parent, true);

            Vector3 offsetPerElement = Vector3.down * (0.1857f - 0.1272f);

            // MOVE cycles AFTER this one ahead
            for (int i = index; i < rows.Count; ++i)
            {
                var uiRow = rows[i];
                uiRow.transform.localPosition += offsetPerElement;
                // Also offset the title
                var uiTitleTransform = menu.transform.GetChild(uiRow.transform.GetSiblingIndex() - 1);
                uiTitleTransform.localPosition += offsetPerElement;
            }

            // If we're appending to the end, move us UP
            if (index == rows.Count)
            {
                newTitle.transform.localPosition += offsetPerElement;
                newOption.transform.localPosition += offsetPerElement;
            }

            // Prepend our option
            var newUICycle = newOption.GetComponent<UISelectionCycle>();
            rows.Insert(index, newUICycle);



            // Update our UI Row to work 
            var navRow = (WrapCounter) UINavigatorRowField.GetValue(navigator);
            UINavigatorRowField.SetValue(navigator, new WrapCounter(navRow.count + 1, navRow.value));

            newTitle.name = $"{name} Title";
            newOption.name = $"{name} Option";

            newTitle.GetComponent<TMP_Text>().text = name;

            // Move our entire menu back to fit the new option
            menu.transform.localPosition += MenuOffsetPerNewOption;
            
            return newUICycle;
        }

        public struct CustomBeatmapRoom
        {
            public string Name;
            public string SceneName;

            public CustomBeatmapRoom(string name, string sceneName)
            {
                Name = name;
                SceneName = sceneName;
            }
        }
    }
}
