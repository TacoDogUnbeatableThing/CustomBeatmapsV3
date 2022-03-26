using System;
using System.IO;

namespace CustomBeatmaps.Util
{
    /// <summary>
    /// Queries files or folders for "changes" (writes/renames/deletes etc.)
    /// </summary>
    public static class FileWatchHelper
    {
        public static FileSystemWatcher WatchFile(string filePath, Action<string> onChange)
        {
            var fileWatcher = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(filePath),
                NotifyFilter = NotifyFilters.DirectoryName |
                               NotifyFilters.FileName
                               | NotifyFilters.LastWrite | NotifyFilters.Size,
                EnableRaisingEvents = true,
                Filter = Path.GetFullPath(filePath),
                IncludeSubdirectories = false
            };

            FileSystemEventHandler Do = (sender, args) =>
            {
                if (Path.GetFullPath(filePath) == Path.GetFullPath(args.FullPath))
                {
                    onChange.Invoke(Path.GetFullPath(filePath));
                }
            };

            fileWatcher.Changed += Do;
            fileWatcher.Created += Do;
            fileWatcher.Deleted += Do;
            fileWatcher.Renamed += (sender, args) => Do(null, args);

            return fileWatcher;
        }

        public static FileSystemWatcher WatchFolder(string dirPath, bool recursive, Action onChange)
        {
            var fileWatcher = new FileSystemWatcher
            {
                Path = dirPath,
                NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.DirectoryName |
                               NotifyFilters.FileName
                               | NotifyFilters.LastWrite | NotifyFilters.Security | NotifyFilters.Size,
                EnableRaisingEvents = true,
                Filter = "*",
                IncludeSubdirectories = recursive
            };
            fileWatcher.Changed += (sender, args) => onChange.Invoke();
            fileWatcher.Created += (sender, args) => onChange.Invoke();
            fileWatcher.Deleted += (sender, args) => onChange.Invoke();
            fileWatcher.Renamed += (sender, args) => onChange.Invoke();

            return fileWatcher;
        }

        public static void WatchFileForModifications(string fpath, Action onWriteChange)
        {
            WatchFile(fpath, path =>
            {
                if (File.Exists(path))
                {
                    onWriteChange.Invoke();
                }
            });
        }
    }
}