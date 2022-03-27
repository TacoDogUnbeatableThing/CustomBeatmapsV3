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
            //OsuEditorPatch.SetEditMode(false);
            LevelManager.LoadLevel(PLAY_SCENE_NAME);
            // We can kind of leave this empty
            JeffBezosController.rhythmProgression = (IProgression) new DefaultProgression(beatmap.OsuPath, PLAY_SCENE_NAME);
        }
        
        public static void PlayBeatmap(CustomBeatmapInfo beatmap, string onlinePackageUrl=null)
        {
            OsuEditorPatch.SetEditMode(false);
            PlayBeatmapinternal(beatmap);
        }

        public static void PlayBeatmapEdit(CustomBeatmapInfo beatmap, bool enableCountdown=false)
        {
            OsuEditorPatch.SetEditMode(true, enableCountdown, beatmap.OsuPath);
            PlayBeatmapinternal(beatmap);
            /*
            CustomBeatmapLoadingOverridePatch.SetOverrideBeatmap(beatmap);
            OsuEditorPatch.SetEditMode(true, path);
            LevelManager.LoadLevel(PLAY_SCENE_NAME);
            */
        }
    }
}