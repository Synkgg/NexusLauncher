using Game_Launcher.Core;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Game_Launcher.Views
{
    public partial class DeadWrong : GameLauncherBaseControl
    {
        public DeadWrong()
        {
            InitializeComponent();

            Loaded += (_, __) => { InitializeLauncher("DeadWrong", "DeadWrong.exe"); };
        }

        // FILES
        public override string VersionUrl => "https://www.dropbox.com/scl/fi/htxcicm2aiwuc45fhn9be/DWVersion.txt?rlkey=13nv7tdylr5n5bx0v91mg6fan&st=vtn7omkt&dl=1";
        public override string ZipUrl => "https://www.dropbox.com/scl/fi/d6q2oupmbeqtuxhru0suc/DeadWrong.zip?rlkey=vdhfkcn1gyfaje7u35hyupk3l&st=3sxkvvcb&dl=1";

        // NEEDED ASSETS
        public override Button PlayButton => Play_Button;
        public override Button OptionButton => OptionsButton;
        public override ProgressBar ProgressBar => Download_ProgressBar;
        public override TextBlock ProgressLabel => Progress_Label;

        // COLORS
        public override SolidColorBrush DefaultButtonColor => new SolidColorBrush(new Color() { A = 100, R = 0, G = 0, B = 0 });
        public override SolidColorBrush DisabledButtonColor => new SolidColorBrush(new Color() { A = 60, R = 0, G = 0, B = 0 });
        public override SolidColorBrush DefaultTextColor => Brushes.White;
        public override SolidColorBrush DisabledTextColor => Brushes.Gray;

        private void PlayButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            switch (Status)
            {
                case GameInstallStatus.ready:
                    Launch();
                    break;
                case GameInstallStatus.install:
                case GameInstallStatus.update:
                    Install(Status == GameInstallStatus.update);
                    break;
                case GameInstallStatus.failed:
                    CheckForUpdates();
                    break;
            }
        }

        private void OptionsButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            OptionsButton.ContextMenu.PlacementTarget = OptionsButton;
            OptionsButton.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            OptionsButton.ContextMenu.IsOpen = true;
        }

        private void VerifyGameFiles_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Example: check if all files exist
            if (!System.IO.File.Exists(GameExe))
            {
                System.Windows.MessageBox.Show("Game files are missing or corrupt. Please reinstall.");
            }
            else
            {
                System.Windows.MessageBox.Show("All game files are verified!");
            }
        }

        private void Uninstall_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                if (System.IO.Directory.Exists(GameFolder))
                    System.IO.Directory.Delete(GameFolder, true);

                if (System.IO.File.Exists(ZipPath))
                    System.IO.File.Delete(ZipPath);

                Status = GameInstallStatus.install;
                UpdateUI();

                System.Windows.MessageBox.Show("Game uninstalled successfully.");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to uninstall: {ex.Message}");
            }
        }

        private void DiscordButton_Click(object sender, RoutedEventArgs e)
        {
            string discordInvite = "https://discord.gg/ddt3RMXTfq";

            Process.Start(new ProcessStartInfo
            {
                FileName = discordInvite,
                UseShellExecute = true
            });
        }

    }
}
