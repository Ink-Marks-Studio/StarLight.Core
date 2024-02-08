using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace StarLight_Core.Utilities
{
    // 哈希工具
    public static class HashUtil
    {
        public static string CalculateSha512(string input)
        {
            using (SHA512 sha512 = SHA512.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha512.ComputeHash(bytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        public static string CalculateSha1(string input)
        {
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha1.ComputeHash(bytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        public static string CalculateMd5(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(bytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
        
        public static string CalculateFileHash(string filePath, HashAlgorithm algorithm)
        {
            using (FileStream stream = File.OpenRead(filePath))
            {
                byte[] hashBytes = algorithm.ComputeHash(stream);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        public static bool VerifyFileHash(string filePath, string hash, HashAlgorithm algorithm)
        {
            string newHash = CalculateFileHash(filePath, algorithm);
            return newHash.Equals(hash, StringComparison.OrdinalIgnoreCase);
        }

        public static bool VerifySha512(string input, string hash)
        {
            string newHash = CalculateSha512(input);
            return newHash.Equals(hash, StringComparison.OrdinalIgnoreCase);
        }

        public static bool VerifySha1(string input, string hash)
        {
            string newHash = CalculateSha1(input);
            return newHash.Equals(hash, StringComparison.OrdinalIgnoreCase);
        }

        public static bool VerifyMd5(string input, string hash)
        {
            string newHash = CalculateMd5(input);
            return newHash.Equals(hash, StringComparison.OrdinalIgnoreCase);
        }
    }
}