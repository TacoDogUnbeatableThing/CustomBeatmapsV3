using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using CustomBeatmaps.Util;

namespace CustomBeatmaps.CustomPackages
{
    public class UserSession
    {
        public string UniqueId { get; private set; } = null;
        public bool LoggedIn => UniqueId != null && Username != null;
        public string Username { get; private set; } = null;
        public string LoginStatus = "(log in not attempted)";

        public bool LoginFailed;

        public async Task AttemptLogin()
        {
            LoginFailed = false;
            if (UserServerHelper.LocalUserSessionExists(Config.Mod.UserUniqueIdFile))
            {
                try
                {
                    LoginStatus = "logging in...";
                    UniqueId = UserServerHelper.LoadUserSession(Config.Mod.UserUniqueIdFile);
                    UserInfo info = await UserServerHelper.GetUserInfo(Config.Backend.ServerUserURL, UniqueId);
                    Username = info.Name;
                    LoginStatus = "logged in!";

                    // High scores will want to know about this.
                    CustomBeatmaps.ServerHighScoreManager.CheckDesyncedHighScores(Username);
                }
                catch (Exception e)
                {
                    LoginFailed = true;
                    if (e is SocketException)
                    {
                        LoginStatus = "Failed to connect! Shout at TacoTechnica to fix this.";
                    }
                    else
                    {
                        LoginStatus = "login failed: " + e.Message;
                    }
                }
            }
            else
            {
                LoginStatus = "no user session found at " + Config.Mod.UserUniqueIdFile;
            }
        }

        public async Task RegisterNewUserSession(string username)
        {
            LoginStatus = "registering...";
            try
            {
                // Get the user ID, set it and save it locally.
                var newUserInfo = await UserServerHelper.RegisterUser(Config.Backend.ServerUserURL, username);
                Username = username;
                UniqueId = newUserInfo.UniqueId;
                UserServerHelper.SaveUserSession(Config.Mod.UserUniqueIdFile, UniqueId);
                LoginStatus = "registered!";
                
                // We registered, so check for unregistered high scores
                CustomBeatmaps.ServerHighScoreManager.CheckDesyncedHighScores(Username);
            }
            catch (Exception e)
            {
                EventBus.ExceptionThrown?.Invoke(e);
                if (e is SocketException)
                {
                    LoginStatus = "Failed to connect! Shout at TacoTechnica to fix this.";
                }
                else
                {
                    LoginStatus = "register failed: " + e.Message;
                }
            }
        }

        public bool LocalSessionExists()
        {
            return UserServerHelper.LocalUserSessionExists(Config.Mod.UserUniqueIdFile);
        }
    }
}
