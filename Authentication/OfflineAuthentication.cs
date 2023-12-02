using System.Text.RegularExpressions;
using StarLight_Core.Enum;

namespace StarLight_Core.Authentication
{
    public static class OfflineAuthentication
    {
        public static string AccessToken { get; set; }
        public static string ClientToken { get; set; }
        public static string Name { get; set; }
        public static string Uuid { get; set; }
        public static async Task<Dictionary<string, string>> Authenticate(string username,string uuid = default)
        {
            AccessToken = Guid.NewGuid().ToString("N");
            ClientToken = Guid.NewGuid().ToString("N");
            Name = username;
            
            if (uuid == default) {
                Uuid = Guid.NewGuid().ToString();
            }
            else if (IsValidUuid(uuid)) {
                Uuid = uuid;
            }
            else
            {
                Uuid = Guid.NewGuid().ToString();
            }
            
            Dictionary<string, string> result = new Dictionary<string, string>
            {
                { "AuthType", AuthType.Offline.ToString() },
                { "accessToken", AccessToken },
                { "clientToken", ClientToken },
                { "id", Uuid },
                { "name", username }
            };

            return result;
        }
        
        // 验证Uuid是否合法
        public static bool IsValidUuid(string uuid)
        {
            string pattern = @"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$";
            Regex regex = new Regex(pattern);

            return regex.IsMatch(uuid);
        }
    }
}
