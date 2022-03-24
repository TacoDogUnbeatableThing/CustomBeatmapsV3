using CustomBeatmaps.Util;

namespace CustomBeatmaps.CustomPackages
{
    public class UserSession
    {
        public string UniqueId { get; private set; } = null;
        public bool LoggedIn => UniqueId != null;

        public void LoadUserSession(string localPath)
        {
            if (UserServerHelper.TryLoadUserUniqueId(localPath, out string id))
            {
                UniqueId = id;
            }
        }
    }
}
