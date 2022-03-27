using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CustomBeatmaps.CustomPackages;
using CustomBeatmaps.UI;
using HarmonyLib;

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

        public static void SortServerPackages(List<CustomServerPackage> headers, SortMode sortMode)
        {
            headers.Sort((left, right) =>
            {
                switch (sortMode)
                {
                    case SortMode.New:
                        return DateTime.Compare(right.UploadTime, left.UploadTime);
                    case SortMode.Old:
                        return DateTime.Compare(left.UploadTime, right.UploadTime);
                    case SortMode.Title:
                        string nameL = Path.GetFileName(left.ServerURL),
                            nameR = Path.GetFileName(right.ServerURL);
                        return String.CompareOrdinal(nameL, nameR);
                    case SortMode.Artist:
                        string artistLeft = left.Beatmaps.Select(map => map.Artist).OrderBy(x => x).Join();
                        string artistRight = right.Beatmaps.Select(map => map.Artist).OrderBy(x => x).Join();
                        return String.CompareOrdinal(artistLeft, artistRight);
                    case SortMode.Creator:
                        string creatorLeft = left.Beatmaps.Select(map => map.Creator).OrderBy(x => x).Join();
                        string creatorRight = right.Beatmaps.Select(map => map.Creator).OrderBy(x => x).Join();
                        return String.CompareOrdinal(creatorLeft, creatorRight);
                    case SortMode.Downloaded:
                        bool downloadedLeft = CustomBeatmaps.LocalServerPackages.PackageExists(
                            CustomPackageHelper.GetLocalFolderFromServerPackageURL(Config.Mod.ServerPackagesDir,
                                left.ServerURL));
                        bool downloadedRight = CustomBeatmaps.LocalServerPackages.PackageExists(
                            CustomPackageHelper.GetLocalFolderFromServerPackageURL(Config.Mod.ServerPackagesDir,
                                right.ServerURL));
                        return (downloadedLeft ? 1 : 0).CompareTo(downloadedRight ? 1 : 0);
                    default:
                        throw new ArgumentOutOfRangeException(nameof(sortMode), sortMode, null);
                }
                ;
            });
        }
        public static void SortLocalPackages(List<CustomLocalPackage> packages, SortMode sortMode)
        {
            packages.Sort((left, right) =>
            {
                switch (sortMode)
                {
                    case SortMode.New:
                        return DateTime.Compare(Directory.GetLastWriteTime(right.FolderName), Directory.GetLastWriteTime(left.FolderName));
                    case SortMode.Old:
                        return DateTime.Compare(Directory.GetLastWriteTime(left.FolderName), Directory.GetLastWriteTime(right.FolderName));
                    case SortMode.Title:
                        string nameL = Path.GetFileName(left.FolderName),
                            nameR = Path.GetFileName(right.FolderName);
                        return String.CompareOrdinal(nameL, nameR);
                    case SortMode.Artist:
                        string artistLeft = left.Beatmaps.Select(map => map.Artist).OrderBy(x => x).Join();
                        string artistRight = right.Beatmaps.Select(map => map.Artist).OrderBy(x => x).Join();
                        return String.CompareOrdinal(artistLeft, artistRight);
                    case SortMode.Creator:
                        string creatorLeft = left.Beatmaps.Select(map => map.BeatmapCreator).OrderBy(x => x).Join();
                        string creatorRight = right.Beatmaps.Select(map => map.BeatmapCreator).OrderBy(x => x).Join();
                        return String.CompareOrdinal(creatorLeft, creatorRight);
                    case SortMode.Downloaded:
                        return 1.CompareTo(1); // um
                    default:
                        throw new ArgumentOutOfRangeException(nameof(sortMode), sortMode, null);
                }
                ;
            });
        }

    }
}