namespace CustomBeatmaps.Util
{
    /// <summary>
    /// Static access for our config data.
    ///
    /// I prefer this to referencing CustomBeatmaps all the time.
    /// </summary>
    public static class Config
    {
        public static ModConfig Mod => CustomBeatmaps.ModConfig;
        public static BackendConfig Backend => CustomBeatmaps.BackendConfig;
    }
}