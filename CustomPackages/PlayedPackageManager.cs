using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CustomBeatmaps.CustomPackages
{
    public class PlayedPackageManager
    {
        private readonly HashSet<string> _played = new HashSet<string>();
        private readonly string _filePath;

        public PlayedPackageManager(string fileToRead)
        {
            _filePath = fileToRead;

            if (File.Exists(fileToRead))
            {
                foreach (string file in File.ReadAllLines(fileToRead))
                {
                    _played.Add(Path.GetFullPath(file));
                }
            }
        }

        public bool HasPlayed(string path)
        {
            return _played.Contains(Path.GetFullPath(path));
        }

        public void RegisterPlay(string path)
        {
            if (!_played.Contains(path))
            {
                _played.Add(path);
                Save();
            }
        }

        private void Save()
        {
            File.WriteAllLines(_filePath, _played.AsEnumerable());
        }
    }
}
