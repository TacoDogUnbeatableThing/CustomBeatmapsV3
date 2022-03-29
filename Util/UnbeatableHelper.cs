using System;
using System.Linq;
using CustomBeatmaps.CustomPackages;
using CustomBeatmaps.Patches;
using Rhythm;

namespace CustomBeatmaps.Util
{
    public static class UnbeatableHelper
    {
        private static readonly string PLAY_SCENE_NAME = "TrainStationRhythm";

        private static void PlayBeatmapinternal(CustomBeatmapInfo beatmap)
        {
            CustomBeatmapLoadingOverridePatch.SetOverrideBeatmap(beatmap);
            LevelManager.LoadLevel(PLAY_SCENE_NAME);
            JeffBezosController.rhythmProgression = new DefaultProgression(beatmap.OsuPath, PLAY_SCENE_NAME);
        }
        
        public static void PlayBeatmap(CustomBeatmapInfo beatmap, bool registerHighScores)
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
            PlayBeatmapinternal(beatmap);
        }

        public static void PlayBeatmapEdit(CustomBeatmapInfo beatmap, bool enableCountdown=false)
        {
            OsuEditorPatch.SetEditMode(true, enableCountdown, beatmap.OsuPath);
            PlayBeatmapinternal(beatmap);
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

    }
}
