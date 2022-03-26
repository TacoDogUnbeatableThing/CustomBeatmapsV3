using System;
using System.Linq;
using System.Threading.Tasks;
using BepInEx;
using Newtonsoft.Json.Linq;

namespace CustomBeatmaps.Util
{
    public static class VersionHelper
    {
        public static Version GetModVersion()
        {
            if (typeof(CustomBeatmaps).GetCustomAttributes(
                    typeof(BepInPlugin), true
                ).FirstOrDefault() is BepInPlugin dnAttribute)
            {
                return dnAttribute.Version;
            }
            return null;
        }

        public static async Task<Version> GetOnlineLatestReleaseVersion()
        {
            var tags = await FetchHelper.GetJSON<JArray>(Config.Backend.RepoLatestTagsURL);
            if (tags.Count == 0)
                return null;
            var tag = tags[0];
            var versionString = tag["name"];
            if (versionString == null)
            {
                return null;
            }
            return new Version(versionString.ToString());
        }
    }
}