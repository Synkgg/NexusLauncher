using Game_Launcher.Core;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace Game_Launcher.Views
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            InstallPathTextBox.Text = SettingsManager.Settings.GamesInstallPath;
            UpdateFreeSpace();
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select game installation folder",
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = "Select Folder"
            };

            if (dialog.ShowDialog() == true)
            {
                string folderPath = Path.GetDirectoryName(dialog.FileName);

                SettingsManager.Settings.GamesInstallPath = folderPath;
                SettingsManager.Save();

                InstallPathTextBox.Text = folderPath;
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void UpdateFreeSpace()
        {
            long free = DiskUtils.GetFreeBytes(SettingsManager.Settings.GamesInstallPath);
            DiskSpaceLabel.Text = $"Available space: {DiskUtils.FormatBytes(free)}";
        }

    }
}
