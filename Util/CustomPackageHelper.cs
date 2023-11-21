using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CustomBeatmaps.CustomPackages;
using HarmonyLib;
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

            // Remove "USER_BEATMAPS/" (V1 compatability)
            if (audioFile.StartsWith("USER_BEATMAPS/"))
                audioFile = audioFile.Substring("USER_BEATMAPS/".Length);

            var audioFolder = Path.GetDirectoryName(bmapPath);
            var trueAudioPath = Path.Combine(audioFolder,  audioFile); // Path.Join fails.

            return new CustomBeatmapInfo(new TextAsset(text), artist, beatmapCreator,
                songName, difficulty, trueAudioPath, bmapPath);
        }

        public static string GetBeatmapProp(string beatmapText, string prop, string beatmapPath)
        {
            var match = Regex.Match(beatmapText, $"{prop}: *(.+?)\r?\n");
            if (match.Groups.Count > 1)
            {
                return match.Groups[1].Value;
            }
            throw new BeatmapException($"{prop} property not found.", beatmapPath);
        }

        private static bool IsBeatmapFile(string beatmapPath)
        {
            return beatmapPath.ToLower().EndsWith(".osu");
        }

        public static bool TryLoadLocalPackage(string packageFolder, string outerFolderPath, out CustomLocalPackage package, bool recursive = false,
            Action<BeatmapException> onBeatmapFail = null)
        {
            packageFolder = Path.GetFullPath(packageFolder);
            outerFolderPath = Path.GetFullPath(outerFolderPath);

            // We can't do Path.GetRelativePath, Path.GetPathRoot, or string.Split so this works instead.
            string relative = Path.GetFullPath(packageFolder).Substring(outerFolderPath.Length + 1); // + 1 removes the start slash
            // We also only want the stub (lowest directory)
            string rootSubFolder = Path.Combine(outerFolderPath, StupidMissingTypesHelper.GetPathRoot(relative));
            package.FolderName = rootSubFolder;
            ScheduleHelper.SafeLog($"{packageFolder.Substring(AppDomain.CurrentDomain.BaseDirectory.Length)}");


            List<CustomBeatmapInfo> bmaps = new List<CustomBeatmapInfo>();
            foreach (string packageSubFile in recursive? Directory.EnumerateFiles(packageFolder, "*.*", SearchOption.AllDirectories) : Directory.EnumerateFiles(packageFolder))
            {
                ScheduleHelper.SafeLog($"    {packageSubFile.Substring(packageFolder.Length)}");
                if (IsBeatmapFile(packageSubFile))
                {
                    try
                    {
                        var customBmap = LoadLocalBeatmap(packageSubFile);
                        bmaps.Add(customBmap);
                        //ScheduleHelper.SafeLog("          (OSU!)");
                    }
                    catch (BeatmapException e)
                    {
                        ScheduleHelper.SafeLog($"    BEATMAP FAIL: {e.Message}");
                        onBeatmapFail?.Invoke(e);
                    }
                }
            }

            // This folder has some beatmaps!
            if (bmaps.Count != 0)
            {
                package.Beatmaps = bmaps.ToArray();
                return true;
            }

            // Empty
            package = new CustomLocalPackage();
            return false;
        }

        public static CustomLocalPackage[] LoadLocalPackages(string folderPath, Action<BeatmapException> onBeatmapFail=null)
        {
            folderPath = Path.GetFullPath(folderPath);

            List<CustomLocalPackage> result = new List<CustomLocalPackage>();

            // Folders = packages
            foreach (string subDir in Directory.EnumerateDirectories(folderPath, "*.*", SearchOption.AllDirectories))
            {
                CustomLocalPackage potentialNewPackage;
                if (TryLoadLocalPackage(subDir, folderPath, out potentialNewPackage, false, onBeatmapFail))
                {
                    result.Add(potentialNewPackage);
                }
            }

            // Files = packages too! For compatibility with V1 (cause why not)
            foreach (string subFile in Directory.GetFiles(folderPath))
            {
                if (IsBeatmapFile(subFile))
                {
                    try
                    {
                        var customBmap = LoadLocalBeatmap(subFile);
                        var newPackage = new CustomLocalPackage();
                        newPackage.Beatmaps = new[] {customBmap};
                        result.Add(newPackage);
                    }
                    catch (BeatmapException e)
                    {
                        onBeatmapFail?.Invoke(e);
                    }
                }
            }

            ScheduleHelper.SafeLog($"LOADED {result.Count} PACKAGES");
            // ScheduleHelper.SafeLog($"####### FULL PACKAGES LIST: #######\n{result.Join(delimiter:"\n")}");
            
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
            return await FetchHelper.GetJSON<CustomServerPackageList>(url);
        }

        public static async Task<Dictionary<string, ServerSubmissionPackage>> FetchServerSubmissions(string url)
        {
            return await FetchHelper.GetJSON<Dictionary<string, ServerSubmissionPackage>>(url);
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

        private static bool _dealingWithTempFile;

        private static async Task DownloadPackageInner(string downloadURL, string targetFolder)
        {
            ScheduleHelper.SafeLog($"Downloading package from {downloadURL} to {targetFolder}");
            
            string tempDownloadFilePath = ".TEMP.zip";

            // Impromptu mutex, as per usual.
            // Only let one download handle the temporary file at a time.
            while (_dealingWithTempFile)
            {
                Thread.Sleep(200);
            }

            _dealingWithTempFile = true;
            try
            {
                await FetchHelper.DownloadFile(downloadURL, tempDownloadFilePath);

                // Extract
                ZipFile.ExtractToDirectory(tempDownloadFilePath, targetFolder, true);
                // Delete old
                File.Delete(tempDownloadFilePath);
            }
            catch (Exception)
            {
                _dealingWithTempFile = false;
                throw;
            }
            _dealingWithTempFile = false;
        }
        
        /// <summary>
        /// Downloads a package from a server URL locally
        /// </summary>
        /// <param name="packageDownloadURL"> Hosted Directory above the package location ex. http://64.225.60.116:8080  </param>
        /// <param name="serverPackageRoot"> Hosted Directory within above directory ex. packages, creating http://64.225.60.116:8080/packages) </param>
        /// <param name="localServerPackageDirectory"> Local directory to save packages ex. SERVER_PACKAGES </param>
        /// <param name="serverPackageURL">The url from the server (https or "packages/{something}.zip"</param>
        /// <param name="callback"> Returns the local path of the downloaded file </param>
        public static async Task DownloadPackage(string packageDownloadURL, string serverPackageRoot, string localServerPackageDirectory, string serverPackageURL)
        {
            string serverDownloadURL = GetURLFromServerPackageURL(packageDownloadURL, serverPackageRoot, serverPackageURL);
            string localDownloadExtractPath =
                GetLocalFolderFromServerPackageURL(localServerPackageDirectory, serverPackageURL);

            await DownloadPackageInner(serverDownloadURL, localDownloadExtractPath);
        }

        public static async Task DownloadTemporarySubmissionPackage(string downloadURL, string tempSubmissionFolder)
        {
            try
            {
                if (Directory.Exists(tempSubmissionFolder))
                    Directory.Delete(tempSubmissionFolder, true);
            }
            catch (Exception e)
            {
                EventBus.ExceptionThrown?.Invoke(e);
            }
            await DownloadPackageInner(downloadURL, tempSubmissionFolder);
        }
    }
}
