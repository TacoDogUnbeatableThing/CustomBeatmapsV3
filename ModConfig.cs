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
        public string ServerPackagesDir = "SERVER_PACKAGES";
        /// Songs directory for your OSU install for the mod to access & test
        // TODO: General means of getting the user directory so we have a good default for OSU
        public string OsuSongsDirectory = null;
        /// Directory (relative to UNBEATABLE) where your OSU file packages will export
        public string OsuExportDirectory = ".";
        /// Temporary folder used to load + play a user submission
        public string TemporarySubmissionPackageFolder = ".SUBMISSION_PACKAGE.temp";
        /// The local user "key" for high score submissions
        public string UserUniqueIdFile = ".USER_ID";
        /// Grabs list of all package files
        public string ServerPackageList = "http://64.225.60.116:8080/packages.json";
        /// Grabs list of all submissions currently pending
        public string ServerSubmissionList = "http://64.225.60.116:8080/submissions.json";
        /// The directory for all server data
        public string ServerStorageURL = "http://64.225.60.116:8080";
        /// The root folder within the server directory
        public string ServerPackageRoot = "packages";
        /// Where we grab user data from
        public string ServerUserURL = "http://64.225.60.116:8081";
    }
}
