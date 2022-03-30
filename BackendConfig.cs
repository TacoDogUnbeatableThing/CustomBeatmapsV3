namespace CustomBeatmaps
{
    /// <summary>
    /// Backend config file, not for users to mess with unless they're testing their own backend.
    /// </summary>
    public class BackendConfig
    {
        /// Grabs list of all package files
        public string ServerPackageList = "http://64.225.60.116:8080/packages.json";
        /// Grabs list of all submissions currently pending
        public string ServerSubmissionList = "http://64.225.60.116:8080/submissions.json";
        /// Grabs all high scores
        public string ServerHighScores = "http://64.225.60.116:8080/highscores.json";
        public string ServerLowScores = "http://64.225.60.116:8080/lowscores.json";
        /// The directory for all server data
        public string ServerStorageURL = "http://64.225.60.116:8080";
        /// The root folder within the server directory
        public string ServerPackageRoot = "packages";
        /// Where we grab user data from
        public string ServerUserURL = "http://64.225.60.116:8081";
        // Grabs the latest project tag from GitHub
        public string RepoLatestTagsURL = "https://api.github.com/repos/TacoDogUnbeatableThing/CustomBeatmapsV3/tags?per_page=1";
        public string DownloadLatestReleaseLink = "https://github.com/TacoDogUnbeatableThing/CustomBeatmapsV3/releases";
        public string DiscordInviteLink = "https://discord.gg/TfZF7Vxv8S";
    }
}