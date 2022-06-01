using System;
using System.IO;

namespace CustomBeatmaps.CustomPackages
{
    public class BeatmapException : Exception
    {
        public readonly string BeatmapPath;

        private string _message;
        public override string Message => _message;

        public BeatmapException(string message, string beatmapPath) : base(message)
        {
            BeatmapPath = Path.GetFullPath(beatmapPath);
            _message = message + " in beatmap " + BeatmapPath;
        }

        public override string ToString()
        {
            return base.ToString() + " in beatmap " + BeatmapPath;
        }
    }
}
