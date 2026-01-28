using System.IO;
using System.Net;

namespace Game_Launcher.Core
{
    public static class DiskUtils
    {
        public static long GetFreeBytes(string path)
        {
            var drive = new DriveInfo(Path.GetPathRoot(path));
            return drive.AvailableFreeSpace;
        }

        public static string FormatBytes(long bytes)
        {
            double size = bytes;
            string[] units = { "B", "KB", "MB", "GB", "TB" };
            int unit = 0;

            while (size >= 1024 && unit < units.Length - 1)
            {
                size /= 1024;
                unit++;
            }

            return $"{size:0.##} {units[unit]}";
        }

        public static long GetRemoteFileSize(string url)
        {
            var request = WebRequest.Create(url);
            request.Method = "HEAD";

            using var response = request.GetResponse();
            return response.ContentLength;
        }

        public static void CopyDirectory(string sourceDir, string targetDir)
        {
            Directory.CreateDirectory(targetDir);

            // Copy files
            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var destFile = Path.Combine(targetDir, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }

            // Copy subdirectories
            foreach (var dir in Directory.GetDirectories(sourceDir))
            {
                var destDir = Path.Combine(targetDir, Path.GetFileName(dir));
                CopyDirectory(dir, destDir);
            }
        }

    }

}
