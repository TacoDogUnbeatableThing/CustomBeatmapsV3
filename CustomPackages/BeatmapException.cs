using System;
using System.IO;

namespace CustomBeatmaps.CustomPackages
{
    public class BeatmapException : Exception
    {
        public readonly string BeatmapPath;

        public BeatmapException(string message, string beatmapPath) : base(message)
        {
            BeatmapPath = Path.GetFullPath(beatmapPath);
        }

        public override string ToString()
        {
            return base.ToString() + " in beatmap " + BeatmapPath;
        }
    }
}
