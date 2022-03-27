namespace CustomBeatmaps.Patches
{
    /// <summary>
    /// Grant cheats when playing a beatmap (from OSU directory) for editing/testing purposes.
    /// </summary>
    public static class OsuEditorPatch
    {
        private static bool _editMode;
        private static string _editPath;
        public static void SetEditMode(bool editMode, string path=null)
        {
            _editMode = editMode;
            _editPath = path != null? path.Replace('\\', '/') : path;
        }
    }
}