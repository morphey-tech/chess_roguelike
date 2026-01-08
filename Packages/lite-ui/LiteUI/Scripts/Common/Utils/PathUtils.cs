using System;
using System.IO;
using JetBrains.Annotations;

namespace LiteUI.Common.Utils
{
    [PublicAPI]
    public static class PathUtils
    {
        public const long BYTES_IN_MEGABYTES = 1024L * 1024L;

        public static string NormalizePath(string path)
        {
            if (path.Length == 0) {
                return "";
            }
            string sourcePath = path;
            if (!Path.IsPathRooted(path)) {
                string curDirectory = Directory.GetCurrentDirectory();
                sourcePath = Path.Combine(curDirectory, path);
            }
            string localPath = new Uri(sourcePath).LocalPath;
            string fullPath = Path.GetFullPath(localPath);
            return fullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).ToLowerInvariant();
        }
    }
}
