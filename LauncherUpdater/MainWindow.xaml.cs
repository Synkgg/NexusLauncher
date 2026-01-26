using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using System.Windows;

namespace LauncherUpdater
{
    public partial class MainWindow : Window
    {
        private const string UpdateZipUrl =
            "https://www.dropbox.com/scl/fi/5slcwwnd7ni8k72f5aq6e/NexusLauncher.zip?rlkey=40fis5afyyskyc1rr7fa8bzb4&st=4irw3eq2&dl=1";

        public MainWindow()
        {
            InitializeComponent();
            Loaded += async (_, __) => await RunUpdateAsync();
        }

        private async Task RunUpdateAsync()
        {
            try
            {
                // Ensure launcher is closed
                foreach (var p in Process.GetProcessesByName("Launcher"))
                    p.WaitForExit();

                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string zipPath = Path.Combine(baseDir, "LauncherUpdate.zip");
                string launcherExe = Path.Combine(baseDir, "NexusLauncher.exe");

                StatusText.Text = "Downloading update...";

                using (var wc = new WebClient())
                {
                    wc.DownloadProgressChanged += (_, e) =>
                        ProgressBar.Value = e.ProgressPercentage;

                    await wc.DownloadFileTaskAsync(
                        new Uri(UpdateZipUrl),
                        zipPath
                    );
                }

                StatusText.Text = "Applying update...";
                ProgressBar.IsIndeterminate = true;

                ZipFile.ExtractToDirectory(zipPath, baseDir, true);
                File.Delete(zipPath);

                StatusText.Text = "Restarting launcher...";
                Process.Start(launcherExe);

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Launcher update failed:\n\n" + ex.Message,
                    "Update Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                Close();
            }
        }
    }
}
