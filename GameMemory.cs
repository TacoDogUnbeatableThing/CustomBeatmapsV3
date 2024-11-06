using System.IO;
using CustomBeatmaps.UI;
using CustomBeatmaps.Util;

using File = Pri.LongPath.File;
using Path = Pri.LongPath.Path;
using Directory = Pri.LongPath.Directory;

namespace CustomBeatmaps
{
    public class GameMemory
    {
        public int SelectedRoom = 0;
        public Tab SelectedTab = Tab.Online;
        public bool OpeningDisclaimerDisabled = false;
        // Extra modes for fun!
        public bool OneLifeMode = false;
        public bool FlipMode = false;

        public static GameMemory Load(string path)
        {
            if (File.Exists(path))
                return SerializeHelper.LoadJSON<GameMemory>(path);
            return new GameMemory();
        }

        public static void Save(string path, GameMemory gameMemory)
        {
            SerializeHelper.SaveJSON(path, gameMemory);
        }
    }
}