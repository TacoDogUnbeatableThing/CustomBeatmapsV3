
using CustomBeatmaps.CustomPackages;

namespace CustomBeatmaps.UI
{

    public enum Tab
    {
        Online, Local, Submissions, Osu
    }

    public enum SortMode
    {
        New, Title, Artist, Creator, Downloaded
    }

    public struct PackageHeader
    {
        public string Name;
        public int SongCount;
        public int MapCount;
        public string Creator;
        public bool New;
        public BeatmapDownloadStatus DownloadStatus; // Kinda jank since this should only be for servers, but whatever.

        public PackageHeader(string name, int songCount, int mapCount, string creator, bool @new, BeatmapDownloadStatus downloadStatus)
        {
            Name = name;
            SongCount = songCount;
            MapCount = mapCount;
            Creator = creator;
            New = @new;
            DownloadStatus = downloadStatus;
        }
    }

    public struct BeatmapHeader
    {
        public string Name;
        public string Artist;
        public string Creator;
        public string Difficulty;
        public string IconURL;

        public BeatmapHeader(string name, string artist, string creator, string difficulty, string iconURL)
        {
            Name = name;
            Artist = artist;
            Creator = creator;
            Difficulty = difficulty;
            IconURL = iconURL;
        }
    }
}