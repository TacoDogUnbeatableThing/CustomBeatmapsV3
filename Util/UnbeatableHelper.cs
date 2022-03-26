using CustomBeatmaps.CustomPackages;
using CustomBeatmaps.Patches;
using Rhythm;

namespace CustomBeatmaps.Util
{
    public static class UnbeatableHelper
    {
        private static readonly string PLAY_SCENE_NAME = "TrainStationRhythm";

        public static void PlayBeatmap(CustomBeatmapInfo beatmap, string onlinePackageUrl=null)
        {
            CustomBeatmapLoadingOverridePatch.SetOverrideBeatmap(beatmap);
            //OsuEditorPatch.SetEditMode(false);
            LevelManager.LoadLevel(PLAY_SCENE_NAME);
            // We can kind of leave this empty
            JeffBezosController.rhythmProgression = (IProgression) new DefaultProgression("", PLAY_SCENE_NAME);
        }

        public static void PlayBeatmapEdit(CustomBeatmapInfo beatmap, string path)
        {
            /*
            CustomBeatmapLoadingOverridePatch.SetOverrideBeatmap(beatmap);
            OsuEditorPatch.SetEditMode(true, path);
            LevelManager.LoadLevel(PLAY_SCENE_NAME);
            */
        }
    }
}