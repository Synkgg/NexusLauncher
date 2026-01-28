using System;
using System.IO;
using System.Text.Json;

namespace Game_Launcher.Core
{
    public static class SettingsManager
    {
        private static readonly string RootPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NexusLauncher");

        private static readonly string SettingsFile =
            Path.Combine(RootPath, "settings.json");

        public static LauncherSettings Settings { get; private set; }

        public static void Load()
        {
            Directory.CreateDirectory(RootPath);

            if (File.Exists(SettingsFile))
            {
                Settings = JsonSerializer.Deserialize<LauncherSettings>(
                    File.ReadAllText(SettingsFile)) ?? new LauncherSettings();
            }
            else
            {
                Settings = new LauncherSettings
                {
                    GamesInstallPath = Path.Combine(RootPath, "Games"),
                    LocatedGames = new Dictionary<string, string>()
                };
                Save();
            }
        }

        public static void Save()
        {
            File.WriteAllText(SettingsFile,
                JsonSerializer.Serialize(Settings, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
