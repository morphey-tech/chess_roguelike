using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace LiteUI.Common.Utils
{
    [UsedImplicitly(ImplicitUseKindFlags.Access, ImplicitUseTargetFlags.WithMembers | ImplicitUseTargetFlags.Members)]
    public static class FileUtils
    {
        [UsedImplicitly]
        private static string? _statPath;
        [UsedImplicitly]
        private static AndroidJavaClass? _version;


        public static bool ClearOldFiles(string directory, int keepFileCount, long fineFreeSpace)
        {
            Directory.CreateDirectory(directory);

            List<string> files = Directory.GetFiles(directory).OrderByDescending(File.GetLastWriteTime).ToList();
            if (GetFreeSpace() < fineFreeSpace) {
                foreach (string file in files.Skip(keepFileCount)) {
                    File.Delete(file);
                }
            }
            // ReSharper disable once InvertIf
            if (GetFreeSpace() < fineFreeSpace) {
                foreach (string file in files.Take(keepFileCount)) {
                    File.Delete(file);
                }
            }
            return GetFreeSpace() >= fineFreeSpace;
        }


        public static long GetFreeSpace()
        {
#if UNITY_EDITOR
            return DriveInfo.GetDrives()
                            .FirstOrDefault(d => d.DriveType == DriveType.Fixed && d.IsReady
                                                 && d.Name.StartsWith("c", true, CultureInfo.InvariantCulture))
                            ?.TotalFreeSpace ?? 0;
#elif UNITY_IOS
            return _GetFreeDiskSpace();
#elif UNITY_ANDROID
            return CalcAndroidDiscSpace();
#elif UNITY_STANDALONE
            return DriveInfo.GetDrives()
                            .FirstOrDefault(d => d.DriveType == DriveType.Fixed && d.IsReady 
                                            && d.Name.StartsWith("c", true, CultureInfo.InvariantCulture))?
                            .TotalFreeSpace ?? 0;
#else
            throw new System.NotImplementedException("Unsupported platform");
#endif
        }

#if UNITY_ANDROID
        private static long CalcAndroidDiscSpace()
        {
            if (_statPath == null) {
                AndroidJavaClass jc = new("android.os.Environment");
                AndroidJavaObject file = jc.CallStatic<AndroidJavaObject>("getDataDirectory");
                _statPath = file.Call<string>("getAbsolutePath");
            }
            if (_version == null) {
                _version = new AndroidJavaClass("android.os.Build$VERSION");
            }
            AndroidJavaObject stat = new("android.os.StatFs", _statPath);

            long blocks;
            long blockSize;
            if (_version.GetStatic<int>("SDK_INT") < 18) {
                blocks = stat.Call<int>("getAvailableBlocks");
                blockSize = stat.Call<int>("getBlockSize");
            } else {
                blocks = stat.Call<long>("getAvailableBlocksLong");
                blockSize = stat.Call<long>("getBlockSizeLong");
            }

            return blocks * blockSize;
        }
#endif

#if UNITY_IOS
        [DllImport("__Internal")]
        private static extern long _GetFreeDiskSpace();
#endif
    }
}
