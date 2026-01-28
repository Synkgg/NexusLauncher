using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;

namespace Game_Launcher.Core
{
    public abstract class GameLauncherBase : UserControl
    {
        public abstract Button PlayButton { get; }
        public abstract Button LocateButton { get; }
        public abstract Button OptionButton { get; }
        public abstract ProgressBar ProgressBar { get; }
        public abstract TextBlock ProgressLabel { get; }

        public abstract string VersionUrl { get; }
        public abstract string ZipUrl { get; }

        public abstract SolidColorBrush DefaultButtonColor { get; }
        public abstract SolidColorBrush DisabledButtonColor { get; }
        public abstract SolidColorBrush DefaultTextColor { get; }
        public abstract SolidColorBrush DisabledTextColor { get; }

        protected string GameName;
        protected string ExeRelativePath;

        protected string RootPath;
        protected string GamesPath;
        protected string GameFolder;
        protected string VersionFile;
        protected string ZipPath;
        protected string GameExe;

        protected Version OnlineVersion;

        protected WebClient Client;


        public GameInstallStatus Status { get; protected set; }

        protected void InitializeLauncher(string gameName, string exeRelativePath)
        {
            GameName = gameName;
            ExeRelativePath = exeRelativePath;

            RootPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "NexusLauncher"
            );

            GamesPath = SettingsManager.Settings.GamesInstallPath;
            GameFolder = Path.Combine(GamesPath, GameName);
            VersionFile = Path.Combine(GameFolder, "Version.txt");
            ZipPath = Path.Combine(RootPath, $"{GameName}.zip");
            GameExe = Path.Combine(GameFolder, ExeRelativePath);

            Directory.CreateDirectory(GamesPath);

            CheckForUpdates();
        }

        protected void CheckForUpdates()
        {
            if (!File.Exists(GameExe))
            {
                SetStatus(GameInstallStatus.install);
                return;
            }

            try
            {
                Version local;

                if (File.Exists(VersionFile))
                    local = new Version(File.ReadAllText(VersionFile));
                else
                    local = new Version(0, 0, 0); // or maybe force install/update?

                var remoteText = new WebClient().DownloadString(VersionUrl);
                var remote = new Version(remoteText);

                OnlineVersion = remote;

                if (remote > local)
                    SetStatus(GameInstallStatus.update);
                else
                    SetStatus(GameInstallStatus.ready);
            }
            catch
            {
                SetStatus(GameInstallStatus.failed);
            }
        }

        protected async Task InstallAsync(bool isUpdate)
        {
            try
            {
                Status = isUpdate ? GameInstallStatus.downloadingUpdate : GameInstallStatus.downloadingGame;
                UpdateUI();

                // Fetch online version if needed
                if (!isUpdate || OnlineVersion == null)
                {
                    try
                    {
                        using var versionClient = new WebClient();
                        var remoteText = await versionClient.DownloadStringTaskAsync(VersionUrl);
                        OnlineVersion = new Version(remoteText.Trim());
                    }
                    catch
                    {
                        MessageBox.Show("Failed to fetch version information.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        SetStatus(GameInstallStatus.failed);
                        return;
                    }
                }

                // Ensure the game folder exists
                Directory.CreateDirectory(GameFolder);

                // Path to download ZIP directly into game folder
                var zipPath = Path.Combine(GameFolder, $"{GameName}.zip");

                using (var client = new WebClient())
                {
                    client.DownloadProgressChanged += (_, e) =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            ProgressBar.Value = e.ProgressPercentage;
                            ProgressLabel.Text = $"{e.ProgressPercentage}%";
                        });
                    };

                    await client.DownloadFileTaskAsync(new Uri(ZipUrl), zipPath);
                }

                // Extract ZIP and flatten top-level folder if needed
                using (var archive = ZipFile.OpenRead(zipPath))
                {
                    foreach (var entry in archive.Entries)
                    {
                        if (string.IsNullOrEmpty(entry.Name)) continue; // skip folders

                        // Remove the first folder segment if ZIP has a top-level folder
                        var parts = entry.FullName.Split(new[] { '/', '\\' }, 2);
                        var relativePath = parts.Length == 2 ? parts[1] : parts[0];

                        var destPath = Path.Combine(GameFolder, relativePath);
                        Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);
                        entry.ExtractToFile(destPath, overwrite: true);
                    }
                }

                // Clean up ZIP
                File.Delete(zipPath);

                // Write version file
                File.WriteAllText(VersionFile, OnlineVersion.ToString());

                // Update UI status
                CheckForUpdates();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Install Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                SetStatus(GameInstallStatus.failed);
            }
        }

        protected void Launch()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = GameExe,
                WorkingDirectory = Path.GetDirectoryName(GameExe),
                UseShellExecute = true
            });
        }

        protected void UpdateUI()
        {
            bool isRunning = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(GameExe)).Length > 0;

            switch (Status)
            {
                case GameInstallStatus.install:
                    PlayButton.Background = DefaultButtonColor;
                    PlayButton.Foreground = DefaultTextColor;
                    PlayButton.Content = "INSTALL";
                    LocateButton.Visibility = Visibility.Visible;
                    OptionButton.Visibility = Visibility.Hidden;
                    ProgressBar.Visibility = Visibility.Hidden;
                    ProgressLabel.Visibility = Visibility.Hidden;
                    PlayButton.IsEnabled = !isRunning;
                    break;

                case GameInstallStatus.ready:
                    PlayButton.Background = DefaultButtonColor;
                    PlayButton.Foreground = DefaultTextColor;
                    PlayButton.Content = isRunning ? "RUNNING" : "PLAY";
                    LocateButton.Visibility = Visibility.Collapsed;
                    OptionButton.Visibility = Visibility.Visible;
                    ProgressBar.Visibility = Visibility.Hidden;
                    ProgressLabel.Visibility = Visibility.Hidden;
                    PlayButton.IsEnabled = !isRunning;
                    break;

                case GameInstallStatus.failed:
                    PlayButton.Background = DefaultButtonColor;
                    PlayButton.Foreground = DefaultTextColor;
                    PlayButton.Content = "RETRY";
                    LocateButton.Visibility = Visibility.Collapsed;
                    OptionButton.Visibility = Visibility.Hidden;
                    ProgressBar.Visibility = Visibility.Hidden;
                    ProgressLabel.Visibility = Visibility.Hidden;
                    PlayButton.IsEnabled = !isRunning;
                    break;

                case GameInstallStatus.update:
                    PlayButton.Background = DefaultButtonColor;
                    PlayButton.Foreground = DefaultTextColor;
                    PlayButton.Content = "UPDATE";
                    LocateButton.Visibility = Visibility.Collapsed;
                    OptionButton.Visibility = Visibility.Hidden;
                    PlayButton.IsEnabled = !isRunning;
                    break;

                case GameInstallStatus.downloadingGame:
                    PlayButton.Background = DisabledButtonColor;
                    PlayButton.Foreground = DisabledTextColor;
                    PlayButton.Content = "DOWNLOADING";
                    LocateButton.Visibility = Visibility.Collapsed;
                    OptionButton.Visibility = Visibility.Hidden;
                    ProgressBar.Visibility = Visibility.Visible;
                    ProgressLabel.Visibility = Visibility.Visible;
                    PlayButton.IsEnabled = false;
                    break;

                case GameInstallStatus.downloadingUpdate:
                    PlayButton.Background = DisabledButtonColor;
                    PlayButton.Foreground = DisabledTextColor;
                    PlayButton.Content = "UPDATING";
                    LocateButton.Visibility = Visibility.Collapsed;
                    OptionButton.Visibility = Visibility.Hidden;
                    ProgressBar.Visibility = Visibility.Visible;
                    ProgressLabel.Visibility = Visibility.Visible;
                    PlayButton.IsEnabled = false;
                    break;
            }
        }

        protected void LocateGame()
        {
            var dialog = new OpenFileDialog
            {
                Title = $"Locate {GameName}",
                Filter = "Executable (*.exe)|*.exe",
                CheckFileExists = true
            };

            if (dialog.ShowDialog() != true)
                return;

            var selectedExe = dialog.FileName;

            // 1️⃣ Validate EXE name
            var expectedExeName = Path.GetFileName(ExeRelativePath);

            if (!Path.GetFileName(selectedExe)
                .Equals(expectedExeName, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(
                    $"Please select {expectedExeName}",
                    "Invalid Executable",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // 2️⃣ Validate folder layout
            var selectedFolder = Path.GetDirectoryName(selectedExe)!;
            var expectedExePath = Path.Combine(selectedFolder, ExeRelativePath);

            if (!File.Exists(expectedExePath))
            {
                MessageBox.Show(
                    "Invalid game folder structure.",
                    "Invalid Game",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // ✅ Accept
            GameFolder = selectedFolder;
            GameExe = expectedExePath;
            VersionFile = Path.Combine(GameFolder, "Version.txt");

            if (!File.Exists(VersionFile))
                File.WriteAllText(VersionFile, "0.0.0");

            SettingsManager.Settings.LocatedGames[GameName] = GameExe;
            SettingsManager.Save();

            SetStatus(GameInstallStatus.ready);
        }

        protected void SetStatus(GameInstallStatus status)
        {
            Status = status;
            UpdateUI();
        }


    }

    public enum GameInstallStatus
    {
        install,
        ready,
        failed,
        update,
        downloadingGame,
        downloadingUpdate
    }
    
}