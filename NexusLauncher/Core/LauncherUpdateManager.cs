using System;
using System.Diagnostics;
using System.Net;

namespace Game_Launcher.Core
{
    public static class LauncherUpdateManager
    {
        // 🔧 Change this when you release a new launcher
        public static readonly Version CurrentVersion =
            new Version("1.0.0");

        // 🔗 Server version file
        private const string VersionUrl =
            "https://www.dropbox.com/scl/fi/lysl6oaum1jnfyb6xalfn/Launcher_Version.txt?rlkey=19tn1jcxg7ksnl360v9sn8bx1&st=txzoxryj&dl=1";

        // 🌍 Global state (read anywhere)
        public static bool UpdateAvailable { get; private set; }
        public static Version RemoteVersion { get; private set; }

        /// <summary>
        /// Call ONCE when the app starts
        /// </summary>
        public static void CheckForUpdate()
        {
            try
            {
                var text = new WebClient()
                    .DownloadString(VersionUrl)
                    .Trim();

                RemoteVersion = new Version(text);
                UpdateAvailable = RemoteVersion > CurrentVersion;
            }
            catch
            {
                UpdateAvailable = false;
            }
        }

        /// <summary>
        /// User-clicked update
        /// </summary>
        public static void StartUpdate()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "LauncherUpdater.exe",
                UseShellExecute = true
            });
        }
    }
}
