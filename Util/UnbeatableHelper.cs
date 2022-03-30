using System;
using System.Linq;
using CustomBeatmaps.CustomPackages;
using CustomBeatmaps.Patches;
using Rhythm;

namespace CustomBeatmaps.Util
{
    public static class UnbeatableHelper
    {
        public static readonly CustomBeatmapRoom[] Rooms = {
            new CustomBeatmapRoom("Default", "TrainStationRhythm"),
            new CustomBeatmapRoom("Practice Room", "PracticeRoomRhythm"),
            new CustomBeatmapRoom("NSR", "NSR_Stage"),
            // TODO: Make Tutorial screen viable
            // 1) disable tutorial fail
            // 2) bring back regular pause screen
            //new CustomBeatmapRoom("Tutorial", "Tutorial"),
            new CustomBeatmapRoom("Offset Wizard", "OffsetWizard")
        };
        private static readonly string DEFAULT_BEATMAP_SCENE = "TrainStationRhythm";

        public static string GetSceneNameByIndex(int index)
        {
            if (index < 0 || index >= Rooms.Length)
            {
                return DEFAULT_BEATMAP_SCENE;
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
            PlayBeatmapinternal(beatmap, DEFAULT_BEATMAP_SCENE);
        }

        public static bool UsingHighScoreProhibitedAssists()
        {
            return JeffBezosController.useAssistMode || JeffBezosController.songSpeed < 1 || JeffBezosController.noFail;
        }

        public static HighScoreList LoadWhiteLabelHighscores()
        {
           return HighScoreScreen.LoadHighScores("wl-highscores");
        }

        /// <returns> whether <code>potentialSongPath</code> is in the format "[UNBEATABLE Song]/[DIFFICULTY] </returns>
        public static bool IsValidSongPath(string potentialSongPath)
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
