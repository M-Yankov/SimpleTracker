using System.IO;
using System;
using System.Security.Cryptography;
using System.Text;

using Android.Content;
using Android.Content.PM;

namespace SimpleTracker.Common
{
    public static class Utilities
    {
        /// <summary>
        /// Returns whether an application is installed or not.
        /// For newer android versions the manifest file must contains defined <paramref name="packageName"/> to return <see langword="true"/>.
        /// </summary>
        /// <param name="packageName">Examples: com.google.android.youtube, com.facebook, com.viber</param>
        /// <returns></returns>
        public static bool IsPackageInstalled(ContextWrapper ctx, string packageName)
        {
            try
            {
                const int PackageInfoFlags = 0;
                ApplicationInfo appInfo = ctx.PackageManager.GetApplicationInfo(packageName, PackageInfoFlags);
                return appInfo != null;
            }
            catch
            {
                return false;
            }
        }

        public static bool ShouldRefreshAccessToken(DateTime tokenExpirationDate) =>
            DateTime.UtcNow.Subtract(tokenExpirationDate).TotalMinutes > 30;

        public static string EncryptValue(string value)
        {
            byte[] textBytes = Encoding.Unicode.GetBytes(value);
            byte[] encryptedBytes = GetCryptoBytes(textBytes);

            string encryptedValue = Convert.ToBase64String(encryptedBytes);
            return encryptedValue;
        }

        public static string DecryptValue(string value)
        {
            byte[] textBytes = Convert.FromBase64String(value);

            byte[] decryptedBytes = GetCryptoBytes(textBytes, encrypt: false);

            string decryptedValue = Encoding.Unicode.GetString(decryptedBytes);
            return decryptedValue;
        }

        private static byte[] GetCryptoBytes(byte[] sourceBytes, bool encrypt = true)
        {
            using Aes encryptor = Aes.Create();
            byte[] salt = new byte[]
            {
                    0x49, 0x76, 0x61,
                    0x6e, 0x20, 0x4d,
                    0x65, 0x64, 0x76,
                    0x65, 0x64, 0x65,
                    0x76
            };

            Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(ApplicationSecrets.SimpleEncryptionValue, salt);
            encryptor.Key = pdb.GetBytes(32);
            encryptor.IV = pdb.GetBytes(16);

            using MemoryStream ms = new MemoryStream();
            using (CryptoStream cs = new CryptoStream(
                ms,
                encrypt ? encryptor.CreateEncryptor() : encryptor.CreateDecryptor(),
                CryptoStreamMode.Write))
            {
                cs.Write(sourceBytes, 0, sourceBytes.Length);
                cs.Close();
            }

            byte[] cryptoBytes = ms.ToArray();
            return cryptoBytes;
        }
    }
}