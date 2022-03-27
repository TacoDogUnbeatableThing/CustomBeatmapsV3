using System;
using System.Collections.Generic;
using System.IO;
using CustomBeatmaps.CustomPackages;

namespace CustomBeatmaps.Util
{
    public static class OSUHelper
    {
        public static CustomBeatmapInfo[] GetOsuBeatmaps(string path)
        {
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
                            beatmaps.Add(CustomPackageHelper.LoadLocalBeatmap(file));
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

        private static string GetPackageNameFromOsu(string osuPath)
        {
            string title = "(undefined)";
            foreach (string fpath in Directory.EnumerateFiles(osuPath))
            {
                if (fpath.EndsWith(".osu"))
                {
                    string text = File.ReadAllText(fpath);
                    title = CustomPackageHelper.GetBeatmapProp(text, "Title", fpath);
                }
            }
            return $"LOCAL_{title}";
        }

        public static string CreateExportZipFile(string osuPath, string temporaryFolderLocation)
        {
            if (Directory.Exists(temporaryFolderLocation))
            {
                Directory.Delete(temporaryFolderLocation, true);
            }
            Directory.CreateDirectory(temporaryFolderLocation);
            string packageName = GetPackageNameFromOsu(osuPath);
            string filesLocation = $"{temporaryFolderLocation}/{packageName}";
            Directory.CreateDirectory(filesLocation);

            List<string> beatmaps = new List<string>();
            // Copy over the files
            foreach (string fpath in Directory.EnumerateFiles(osuPath))
            {
                string fname = Path.GetFileName(fpath);
                File.Copy(fpath, $"{filesLocation}/{fname}");
                if (fpath.EndsWith(".osu"))
                {
                    beatmaps.Add(fpath);
                }
            }

            // Zip
            string zipTarget = $"{packageName}.zip";
            // Remove LOCAL_ just... to make it a bit more neat.
            if (zipTarget.StartsWith("LOCAL_")) zipTarget = zipTarget.Substring("LOCAL_".Length);
            System.IO.Compression.ZipFile.CreateFromDirectory(temporaryFolderLocation, zipTarget);

            // Delete temporary directory afterwards
            Directory.Delete(temporaryFolderLocation, true);

            return zipTarget;
        }
    }
}