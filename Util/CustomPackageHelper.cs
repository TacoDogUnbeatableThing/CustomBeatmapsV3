using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CustomBeatmaps.CustomPackages;
using UnityEngine;

namespace CustomBeatmaps.Util
{
    public static class CustomPackageHelper
    {
        public static CustomBeatmapInfo LoadLocalBeatmap(string bmapPath)
        {
            string text = File.ReadAllText(bmapPath);
            string songName = GetBeatmapProp(text, "Title", bmapPath);
            string difficulty = GetBeatmapProp(text, "Version", bmapPath);
            string artist = GetBeatmapProp(text, "Artist", bmapPath);
            string beatmapCreator = GetBeatmapProp(text, "Creator", bmapPath);
            string audioFile = GetBeatmapProp(text, "AudioFilename", bmapPath);

            var audioFolder = Path.GetDirectoryName(bmapPath);
            var trueAudioPath = Path.Combine(audioFolder,  audioFile); // Path.Join fails.

            return new CustomBeatmapInfo(new TextAsset(text), artist, beatmapCreator,
                songName, difficulty, trueAudioPath);
        }

        private static string GetBeatmapProp(string beatmapText, string prop, string beatmapPath)
        {
            var match = Regex.Match(beatmapText, $"{prop}: *(.+?)\r?\n");
            if (match.Groups.Count > 1)
            {
                return match.Groups[1].Value;
            }
            throw new BeatmapException($"{prop} property not found.", beatmapPath);
        }

        public static CustomBeatmapInfo[] LoadLocalBeatmaps(string folderPath, Action<ModError> onError)
        {
            List<CustomBeatmapInfo> result = new List<CustomBeatmapInfo>();

            foreach (string subPath in Directory.EnumerateFiles(folderPath))
            {
                if (Directory.Exists(subPath))
                {
                    // Parse directory with beatmaps
                }
            }

            return result.ToArray();
        }

        // Package IDs are just the ending zip file name.
        // If we want the server to hold on to external packages, we probably want a custom key system or something to prevent duplicate names... 
        private static string GetPackageFolderIdFromServerPackageURL(string serverPackageURL)
        {
            string endingName = Path.GetFileName(serverPackageURL);
            return endingName;
        }

        public static string GetLocalFolderFromServerPackageURL(string localServerPackageDirectory, string serverPackageURL)
        {
            string packageFolderId = GetPackageFolderIdFromServerPackageURL(serverPackageURL);
            return Path.Combine(localServerPackageDirectory, packageFolderId);
        }

        public static async Task<CustomServerPackageList> FetchServerPackageList(string url)
        {
            return await ServerHelper.GetJSON<CustomServerPackageList>(url);
        }

        private static string GetURLFromServerPackageURL(string serverDirectory, string serverPackageRoot, string serverPackageURL)
        {
            // In the form "packages/<something>/zip"
            if (serverPackageURL.StartsWith(serverPackageRoot))
            {
                return serverDirectory + "/" + serverPackageURL;
            }
            // Probably an absolute URL
            return serverPackageURL;
        }

        /// <summary>
        /// Downloads a package from a server URL locally
        /// </summary>
        /// <param name="serverDirectory"> Hosted Directory above the package location ex. http://64.225.60.116:8080  </param>
        /// <param name="serverPackageRoot"> Hosted Directory within above directory ex. packages, creating http://64.225.60.116:8080/packages) </param>
        /// <param name="localServerPackageDirectory"> Local directory to save packages ex. SERVER_PACKAGES </param>
        /// <param name="serverPackageURL">The url from the server (https or "packages/{something}.zip"</param>
        /// <param name="callback"> Returns the local path of the downloaded file </param>
        public static async Task<string> DownloadPackage(string serverDirectory, string serverPackageRoot, string localServerPackageDirectory, string serverPackageURL)
        {
            string serverDownloadURL = GetURLFromServerPackageURL(serverDirectory, serverPackageRoot, serverPackageURL);
            string localDownloadExtractPath =
                GetLocalFolderFromServerPackageURL(localServerPackageDirectory, serverPackageURL);

            string tempDownloadFilePath = ".TEMP.zip";

            await ServerHelper.DownloadFile(serverDownloadURL, tempDownloadFilePath);
            
            // Extract
            System.IO.Compression.ZipFile.ExtractToDirectory(tempDownloadFilePath, localDownloadExtractPath);
            // Delete old
            File.Delete(tempDownloadFilePath);

            return localDownloadExtractPath;
        }
    }
}
