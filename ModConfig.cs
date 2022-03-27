namespace CustomBeatmaps
{
    /// <summary>
    /// Base config file for entire mod
    /// </summary>
    public class ModConfig
    {
        // Directory for user (local) packages
        public string UserPackagesDir = "USER_PACKAGES";
        /// Directory for server/downloaded packages
        public string ServerPackagesDir = "CustomBeatmapsV3-Data/SERVER_PACKAGES";
        /// Songs directory for your OSU install for the mod to access & test
        public string OsuSongsOverrideDirectory = null;
        /// Directory (relative to UNBEATABLE) where your OSU file packages will export
        public string OsuExportDirectory = ".";
        /// Temporary folder used to load + play a user submission
        public string TemporarySubmissionPackageFolder = "CustomBeatmapsV3-Data/.SUBMISSION_PACKAGE.temp";
        /// The local user "key" for high score submissions
        public string UserUniqueIdFile = "CustomBeatmapsV3-Data/.USER_ID";
        /// A line separated list of all beatmaps we've tried playing
        public string PlayedBeatmapList = "CustomBeatmapsV3-Data/.played_beatmaps";
    }
}
