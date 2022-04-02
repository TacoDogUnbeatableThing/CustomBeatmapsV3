using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BepInEx;
using Newtonsoft.Json.Linq;

namespace CustomBeatmaps.Util
{
    public static class VersionHelper
    {
        private static Version _modVersion = null;
        public static Version GetModVersion()
        {
            if (_modVersion == null)
            {
                if (typeof(CustomBeatmaps).GetCustomAttributes(
                        typeof(BepInPlugin), true
                    ).FirstOrDefault() is BepInPlugin dnAttribute)
                {
                    _modVersion = dnAttribute.Version;
                }
                
            }
            return _modVersion != null? _modVersion : new Version();
        }

        public static async Task<Version> GetOnlineLatestReleaseVersion()
        {
            var headers = new Dictionary<string, string>();
            headers["User-Agent"] = "request";
            var tags = await FetchHelper.GetJSON<JArray>(Config.Backend.RepoLatestTagsURL, headers);
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