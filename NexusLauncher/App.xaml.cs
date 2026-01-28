using Game_Launcher.Core;
using System.Configuration;
using System.Data;
using System.Windows;

namespace NexusLauncher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            SettingsManager.Load();
            LauncherUpdateManager.CheckForUpdate();
        }

    }

}
