using System.Security.Cryptography;
using System.Text;

namespace StarLight_Core.Utilities;

// 哈希工具
public static class HashUtil
{
    public static string CalculateSha512(string input)
    {
        using (var sha512 = SHA512.Create())
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = sha512.ComputeHash(bytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }

    public static string CalculateSha1(string input)
    {
        using (var sha1 = SHA1.Create())
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = sha1.ComputeHash(bytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }

    public static string CalculateMd5(string input)
    {
        using (var md5 = MD5.Create())
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = md5.ComputeHash(bytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }

    public static string CalculateFileHash(string filePath, HashAlgorithm algorithm)
    {
        using (var stream = File.OpenRead(filePath))
        {
            var hashBytes = algorithm.ComputeHash(stream);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }

    public static bool VerifyFileHash(string filePath, string hash, HashAlgorithm algorithm)
    {
        var newHash = CalculateFileHash(filePath, algorithm);
        return newHash.Equals(hash, StringComparison.OrdinalIgnoreCase);
    }

    public static bool VerifySha512(string input, string hash)
    {
        var newHash = CalculateSha512(input);
        return newHash.Equals(hash, StringComparison.OrdinalIgnoreCase);
    }

    public static bool VerifySha1(string input, string hash)
    {
        var newHash = CalculateSha1(input);
        return newHash.Equals(hash, StringComparison.OrdinalIgnoreCase);
    }

    public static bool VerifyMd5(string input, string hash)
    {
        var newHash = CalculateMd5(input);
        return newHash.Equals(hash, StringComparison.OrdinalIgnoreCase);
    }
}