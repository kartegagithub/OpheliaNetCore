﻿using System;
using System.Security.Cryptography;
using System.Text;

namespace Ophelia.Cryptography
{
    public static class CryptoManager
    {
        public static Encoding Encoding { get; set; }
        public static string Encrypt(string chipperText, string encryptionKey = "")
        {
            try
            {
                if (CryptoManager.Encoding == null)
                    CryptoManager.Encoding = System.Text.Encoding.UTF8;
                if (string.IsNullOrEmpty(encryptionKey))
                    encryptionKey = "";
                if (string.IsNullOrEmpty(chipperText))
                    return chipperText;
                var DES = TripleDES.Create();
                var hashMD5 = MD5.Create();
                DES.Key = hashMD5.ComputeHash(CryptoManager.Encoding.GetBytes(encryptionKey));
                DES.Mode = CipherMode.ECB;
                ICryptoTransform Encryptor = DES.CreateEncryptor();
                byte[] Buffer = CryptoManager.Encoding.GetBytes(chipperText);
                return Convert.ToBase64String(Encryptor.TransformFinalBlock(Buffer, 0, Buffer.Length));
            }
            catch
            {
                return chipperText;
            }
        }
        public static string Decrypt(string richText, string decryptionKey = "")
        {
            try
            {
                if (CryptoManager.Encoding == null)
                    CryptoManager.Encoding = System.Text.Encoding.UTF8;
                if (string.IsNullOrEmpty(decryptionKey))
                    decryptionKey = "";
                if (string.IsNullOrEmpty(richText))
                    return richText;
                var DES = TripleDES.Create();
                var hashMD5 = MD5.Create();
                DES.Key = hashMD5.ComputeHash(CryptoManager.Encoding.GetBytes(decryptionKey));
                DES.Mode = CipherMode.ECB;
                ICryptoTransform Decryptor = DES.CreateDecryptor();
                byte[] Buffer = Convert.FromBase64String(richText);
                return CryptoManager.Encoding.GetString(Decryptor.TransformFinalBlock(Buffer, 0, Buffer.Length));
            }
            catch
            {
                return richText;
            }
        }
        public static string ComputeHash(string plainText, string saltText = "", HashAlgorithms algorithm = HashAlgorithms.SHA1, Encoding encoding = null)
        {
            try
            {
                if (string.IsNullOrEmpty(plainText)) return plainText;
                if (encoding == null) encoding = Encoding.UTF8;
                HashAlgorithm hashProvider = null;
                switch (algorithm)
                {
                    default:
                    case HashAlgorithms.SHA1:
                        hashProvider = SHA1.Create(); break;
                    case HashAlgorithms.SHA256:
                        hashProvider = SHA256.Create(); break;
                    case HashAlgorithms.SHA384:
                        hashProvider = SHA384.Create(); break;
                    case HashAlgorithms.SHA512:
                        hashProvider = SHA512.Create(); break;
                    case HashAlgorithms.MD5:
                        hashProvider = MD5.Create(); break;
                }

                string hashedText = plainText + saltText;
                byte[] hashbytes = encoding.GetBytes(hashedText);
                byte[] inputbytes = hashProvider.ComputeHash(hashbytes);
                hashProvider.Clear();
                return CryptoHelper.GetHexaDecimal(inputbytes);
            }
            catch { return plainText; }
        }
        public static string GetMd5Hash(string plainText, string saltText = "")
        {
            return ComputeHash(plainText, saltText, HashAlgorithms.MD5);
        }
        public static string GetSHA512Hash(string plainText, string saltText = "")
        {
            return ComputeHash(plainText, saltText, HashAlgorithms.SHA512);
        }
    }
}
