namespace Game_Launcher.Core
{
    public class LauncherSettings
    {
        public string GamesInstallPath { get; set; }
        public Dictionary<string, string> LocatedGames { get; set; } = new Dictionary<string, string>();
    }
}
