using System.Collections.Generic;
using CustomBeatmaps.CustomPackages;
using CustomBeatmaps.UI;

namespace CustomBeatmaps.Util
{
    public static class UIConversionHelper
    {
        public static BeatmapHeader CustomBeatmapInfoToBeatmapHeader(CustomBeatmapInfo bmap)
        {
            return new BeatmapHeader(
                bmap.SongName,
                bmap.Artist,
                bmap.BeatmapCreator,
                bmap.Difficulty,
                null
            );
        }
        public static List<BeatmapHeader> CustomBeatmapInfosToBeatmapHeaders(List<CustomBeatmapInfo> customBeatmaps)
        {
            List<BeatmapHeader> headers = new List<BeatmapHeader>(customBeatmaps.Count);
            foreach (var bmap in customBeatmaps)
            {
                headers.Add(CustomBeatmapInfoToBeatmapHeader(bmap));
            }

            return headers;
        }
    }
}