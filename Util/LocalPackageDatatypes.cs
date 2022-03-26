using System.IO;
using CustomBeatmaps.CustomPackages;
using HarmonyLib;

namespace CustomBeatmaps.Util
{
    public struct CustomLocalPackage
    {
        public string FolderName;
        public CustomBeatmapInfo[] Beatmaps;
        
        public override string ToString()
        {
            return $"{{{Path.GetFileName(FolderName)}: [{Beatmaps.Join()}]}}";
        }

    }
}