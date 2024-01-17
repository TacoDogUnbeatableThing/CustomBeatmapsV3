using System;
using System.IO;

namespace CustomBeatmaps.Util
{
    /// <summary>
    /// Queries files or folders for "changes" (writes/renames/deletes etc.)
    /// </summary>
    public static class FileWatchHelper
    {
        public static FileSystemWatcher WatchFile(string filePath, Action onChange)
        {
            var fileWatcher = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(filePath),
                NotifyFilter = NotifyFilters.DirectoryName |
                               NotifyFilters.FileName
                               | NotifyFilters.LastWrite,
                EnableRaisingEvents = true,
                Filter = Path.GetFullPath(filePath),
                IncludeSubdirectories = false
            };

            void Do(object sender, FileSystemEventArgs args)
            {
                if (Path.GetFullPath(filePath) == Path.GetFullPath(args.FullPath))
                {
                    onChange.Invoke();
                }
            }

            fileWatcher.Changed += Do;
            fileWatcher.Created += Do;
            fileWatcher.Deleted += Do;
            fileWatcher.Renamed += (sender, args) => Do(null, args);

            return fileWatcher;
        }

        public static FileSystemWatcher WatchFolder(string dirPath, bool recursive, Action<FileSystemEventArgs> onChange)
        {
            var fileWatcher = new FileSystemWatcher
            {
                Path = dirPath,
                NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.DirectoryName |
                               NotifyFilters.FileName
                               | NotifyFilters.LastWrite,
                EnableRaisingEvents = true,
                Filter = "*",
                IncludeSubdirectories = recursive
            };

            void Do(object sender, FileSystemEventArgs args)
            {
                onChange.Invoke(args);
            }

            fileWatcher.Changed += Do;
            fileWatcher.Created += Do;
            fileWatcher.Deleted += Do;
            fileWatcher.Renamed += Do;

            return fileWatcher;
        }

        public static void WatchFileForModifications(string fpath, Action onWriteChange)
        {
            WatchFile(fpath, () =>
            {
                if (File.Exists(fpath))
                {
                    onWriteChange.Invoke();
                }
            });
        }
    }
}