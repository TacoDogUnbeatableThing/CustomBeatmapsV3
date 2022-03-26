
using System;

namespace CustomBeatmaps.Util
{
    /// <summary>
    /// We can't use:
    ///
    /// Path.GetRelativePath
    /// Path.GetPathRoot
    /// string.Split
    /// </summary>
    public static class StupidMissingTypesHelper
    {
        public static string GetPathRoot(string path)
        {
            int firstSlash = path.IndexOf("\\", StringComparison.Ordinal);
            return firstSlash < 0? path : path.Substring(0, firstSlash);
        }
    }
}