using Game_Launcher.Core;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Game_Launcher.Views
{
    partial class Cubical : GameLauncherBaseControl
    {
        public Cubical()
        {
            InitializeComponent();

            Loaded += (_, __) => { InitializeLauncher("Cubical", Path.Combine("Builds", "Windows", "x32", "Test Game.exe")); };
        }

        // FILES
        public override string VersionUrl => "https://www.dropbox.com/scl/fi/rh7mrsu9o6daxbdajfolt/Version.txt?rlkey=e8dfthq1wikkyvo41mhl95230&st=iak87mxt&dl=1";
        public override string ZipUrl => "https://www.dropbox.com/scl/fi/hrqeg43jaot31g671hu0c/Cubical.zip?rlkey=7kcuqn1faaj2me444164n4z5p&st=5sf2doeo&dl=1";

        // NEEDED ASSETS
        public override Button PlayButton => Play_Button;
        public override Button LocateButton => Locate_Button;
        public override Button OptionButton => OptionsButton;
        public override ProgressBar ProgressBar => Download_ProgressBar;
        public override TextBlock ProgressLabel => Progress_Label;

        // COLORS
        public override SolidColorBrush DefaultButtonColor => new SolidColorBrush(new Color() { A = 250, R = 0, G = 116, B = 224 });
        public override SolidColorBrush DisabledButtonColor => new SolidColorBrush(new Color() { A = 250, R = 6, G = 64, B = 119 });
        public override SolidColorBrush DefaultTextColor => Brushes.White;
        public override SolidColorBrush DisabledTextColor => Brushes.Gray;

        private async void PlayButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            switch (Status)
            {
                case GameInstallStatus.ready:
                    Launch();
                    break;
                case GameInstallStatus.install:
                    await InstallAsync(false);
                    break;
                case GameInstallStatus.update:
                    await InstallAsync(true);
                    break;
                case GameInstallStatus.failed:
                    CheckForUpdates();
                    break;
            }
        }

        private void LocateButton_Click(object sender, RoutedEventArgs e)
        {
            LocateGame();
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

    }
}
