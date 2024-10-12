using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;

namespace StarLight_Core.Utilities
{
    /// <summary>
    /// 幸运值生成器
    /// </summary>
    public class LuckyValueGenerator
    {
        /// <summary>
        /// 生成幸运值
        /// </summary>
        /// <returns>幸运值</returns>
        public static int GenerateLuckyValue()
        {
            var machineName = Environment.MachineName;
            var userName = Environment.UserName;
            var currentDate = DateTime.Now.ToString("yyyyMMdd");
            var macAddress = GetMacAddress();
            var uniqueString = machineName + userName + macAddress + currentDate;
            var hash = GetHash(uniqueString);
            var random = new Random(hash);
            return random.Next(1, 101);
        }
        
        private static string GetMacAddress()
        {
            var macAddress = NetworkInterface
                .GetAllNetworkInterfaces()
                .FirstOrDefault(nic => nic is { OperationalStatus: OperationalStatus.Up, IsReceiveOnly: false })?
                .GetPhysicalAddress()
                .ToString();

            return macAddress ?? string.Empty;
        }

        private static int GetHash(string input)
        {
            var hashBytes = MD5.HashData(Encoding.UTF8.GetBytes(input));
            var hash = BitConverter.ToInt32(hashBytes, 0);
            return Math.Abs(hash);
        }
    }
}