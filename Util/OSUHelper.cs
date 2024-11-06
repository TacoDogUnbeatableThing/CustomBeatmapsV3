using System;
using System.Collections.Generic;
using System.IO;
using CustomBeatmaps.CustomPackages;

using File = Pri.LongPath.File;
using Path = Pri.LongPath.Path;
using Directory = Pri.LongPath.Directory;

namespace CustomBeatmaps.Util
{
    public static class OSUHelper
    {
        public static CustomBeatmapInfo[] LoadOsuBeatmaps(string path, out string failMessage)
        {
            failMessage = "";
            path = GetOsuPath(path);
            if (Directory.Exists(path))
            {
                List<CustomBeatmapInfo> beatmaps = new List<CustomBeatmapInfo>();
                foreach (string osuProjectDir in Directory.EnumerateDirectories(path))
                {
                    foreach (string file in Directory.EnumerateFiles(osuProjectDir))
                    {
                        if (file.EndsWith(".osu"))
                        {
                            try
                            {
                                var b = CustomPackageHelper.LoadLocalBeatmap(file);
                                beatmaps.Add(b);
                            }
                            catch (Exception e)
                            {
                                failMessage += e.Message + "\n";
                            }
                        }
                    }
                }

                double TimeSinceLastWrite(string filename)
                {
                    return (DateTime.Now - File.GetLastWriteTime(filename)).TotalSeconds;
                }

                // Sort by newest access
                beatmaps.Sort((left, right) => Math.Sign(TimeSinceLastWrite(left.OsuPath) - TimeSinceLastWrite(right.OsuPath)));

                return beatmaps.ToArray();
            }
            return null;
        }

        public static string GetOsuPath(string overridePath)
        {
            if (string.IsNullOrEmpty(overridePath))
            {
                return Path.GetFullPath(Path.Combine(Environment.GetFolderPath(
                    Environment.SpecialFolder.ApplicationData).Replace('\\', '/'), "../Local/osu!/Songs"));
            }
            return overridePath;
        }

        private static string LoadPackageNameFromOsu(string osuPath)
        {
            string text = File.ReadAllText(osuPath);
            return CustomPackageHelper.GetBeatmapProp(text, "Title", osuPath);
        }

        public static string CreateExportZipFile(string osuPath, string temporaryFolderLocation)
        {
            if (Directory.Exists(temporaryFolderLocation))
            {
                Directory.Delete(temporaryFolderLocation, true);
            }
            Directory.CreateDirectory(temporaryFolderLocation);
            string packageName = LoadPackageNameFromOsu(osuPath);
            string filesLocation = temporaryFolderLocation;
            Directory.CreateDirectory(filesLocation);

            // Copy over the files
            string osuFullPath = Path.GetFullPath(osuPath);
            int lastSlash = osuFullPath.LastIndexOf("\\", StringComparison.Ordinal);
            string osuParentDir = lastSlash != -1 ? osuFullPath.Substring(0, lastSlash) : "";
            foreach (string fpath in Directory.EnumerateFiles(osuParentDir))
            {
                string fname = Path.GetFileName(fpath);
                File.Copy(fpath, $"{filesLocation}/{fname}");
            }

            // Zip
            
            string zipTarget = $"{packageName}.zip";
            // Remove LOCAL_ just... to make it a bit more neat.
            if (zipTarget.StartsWith("LOCAL_")) zipTarget = zipTarget.Substring("LOCAL_".Length);

            ZipHelper.CreateFromDirectory(temporaryFolderLocation, zipTarget);

            // Delete temporary directory afterwards
            Directory.Delete(temporaryFolderLocation, true);

            return zipTarget;
        }
    }
}