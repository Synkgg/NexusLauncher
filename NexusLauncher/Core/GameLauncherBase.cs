using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Game_Launcher.Core
{
    public abstract class GameLauncherBase : UserControl
    {
        public abstract Button PlayButton { get; }
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

            GamesPath = Path.Combine(RootPath, "Games");
            GameFolder = Path.Combine(GamesPath, GameName);
            VersionFile = Path.Combine(GameFolder, "Version.txt");
            ZipPath = Path.Combine(RootPath, $"{GameName}.zip");
            GameExe = Path.Combine(GameFolder, ExeRelativePath);

            Directory.CreateDirectory(GamesPath);

            CheckForUpdates();

            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(2);
            timer.Tick += (_, __) => UpdateUI();
            timer.Start();
        }

        protected void CheckForUpdates()
        {
            if (!File.Exists(GameExe))
            {
                Status = GameInstallStatus.install;
                UpdateUI();
                return;
            }

            try
            {
                var local = File.Exists(VersionFile)
                    ? new Version(File.ReadAllText(VersionFile))
                    : new Version(0, 0, 0);

                var remoteText = new WebClient().DownloadString($"{VersionUrl}");
                var remote = new Version(remoteText);

                OnlineVersion = remote;

                Status = remote > local
                    ? GameInstallStatus.update
                    : GameInstallStatus.ready;
            }
            catch
            {
                Status = GameInstallStatus.failed;
            }

            UpdateUI();
        }

        protected void Install(bool isUpdate)
        {
            Client = new WebClient();

            Status = isUpdate
                ? GameInstallStatus.downloadingUpdate
                : GameInstallStatus.downloadingGame;

            UpdateUI();

            Client.DownloadProgressChanged += (_, e) =>
            {
                ProgressBar.Value = e.ProgressPercentage;
                ProgressLabel.Text = $"{e.ProgressPercentage}%";
            };

            Client.DownloadFileCompleted += (_, e) =>
            {
                if (Directory.Exists(GameFolder))
                    Directory.Delete(GameFolder, true);

                ZipFile.ExtractToDirectory(ZipPath, GamesPath);
                File.WriteAllText(VersionFile, OnlineVersion.ToString());
                File.Delete(ZipPath);

                CheckForUpdates();
            };

            Client.DownloadFileAsync(new Uri(ZipUrl), ZipPath);
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
                    PlayButton.Content = isRunning ? "RUNNING" : "INSTALL";
                    OptionButton.Visibility = Visibility.Hidden;
                    ProgressBar.Visibility = Visibility.Hidden;
                    ProgressLabel.Visibility = Visibility.Hidden;
                    PlayButton.IsEnabled = !isRunning;
                    break;

                case GameInstallStatus.ready:
                    PlayButton.Background = DefaultButtonColor;
                    PlayButton.Foreground = DefaultTextColor;
                    PlayButton.Content = isRunning ? "RUNNING" : "PLAY";
                    OptionButton.Visibility = Visibility.Visible;
                    ProgressBar.Visibility = Visibility.Hidden;
                    ProgressLabel.Visibility = Visibility.Hidden;
                    PlayButton.IsEnabled = !isRunning;
                    break;

                case GameInstallStatus.failed:
                    PlayButton.Background = DefaultButtonColor;
                    PlayButton.Foreground = DefaultTextColor;
                    PlayButton.Content = isRunning ? "RUNNING" : "RETRY";
                    OptionButton.Visibility = Visibility.Hidden;
                    ProgressBar.Visibility = Visibility.Hidden;
                    ProgressLabel.Visibility = Visibility.Hidden;
                    PlayButton.IsEnabled = !isRunning;
                    break;

                case GameInstallStatus.update:
                    PlayButton.Background = DefaultButtonColor;
                    PlayButton.Foreground = DefaultTextColor;
                    PlayButton.Content = isRunning ? "RUNNING" : "UPDATE";
                    OptionButton.Visibility = Visibility.Hidden;
                    PlayButton.IsEnabled = !isRunning;
                    break;

                case GameInstallStatus.downloadingGame:
                    PlayButton.Background = DisabledButtonColor;
                    PlayButton.Foreground = DisabledTextColor;
                    PlayButton.Content = "DOWNLOADING";
                    OptionButton.Visibility = Visibility.Hidden;
                    ProgressBar.Visibility = Visibility.Visible;
                    ProgressLabel.Visibility = Visibility.Visible;
                    PlayButton.IsEnabled = false;
                    break;

                case GameInstallStatus.downloadingUpdate:
                    PlayButton.Background = DisabledButtonColor;
                    PlayButton.Foreground = DisabledTextColor;
                    PlayButton.Content = "UPDATING";
                    OptionButton.Visibility = Visibility.Hidden;
                    ProgressBar.Visibility = Visibility.Visible;
                    ProgressLabel.Visibility = Visibility.Visible;
                    PlayButton.IsEnabled = false;
                    break;
            }
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