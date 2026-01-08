using System.IO;
using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;

namespace LiteUI.Common.Utils
{
    [PublicAPI]
    public static class HashUtils
    {
        public static string MakeMd5(string s)
        {
            using (MD5 md5 = MD5.Create()) {
                byte[] bytes = new UTF8Encoding().GetBytes(s);
                byte[] bytesHash = md5.ComputeHash(bytes);
                StringBuilder sBuilder = new();
                foreach (byte b in bytesHash) {
                    sBuilder.Append(b.ToString("x2"));
                }

                return sBuilder.ToString();
            }
        }

        public static string MakeFileSha1(string filePath)
        {
            using (SHA1 sha1 = SHA1.Create()) {
                using (FileStream stream = File.OpenRead(filePath)) {
                    string hash = Encoding.Default.GetString(sha1.ComputeHash(stream));
                    return hash;
                }
            }
        }

        public static string MakeFileMd5(string filePath)
        {
            using (MD5 md5 = MD5.Create()) {
                using (FileStream stream = File.OpenRead(filePath)) {
                    byte[] bytesHash = md5.ComputeHash(stream);
                    StringBuilder sBuilder = new();
                    foreach (byte b in bytesHash) {
                        sBuilder.Append(b.ToString("x2"));
                    }
                    string hash = sBuilder.ToString();
                    return hash;
                }
            }
        }
    }
}
