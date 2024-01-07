using System.Text.RegularExpressions;
using StarLight_Core.Enum;
using StarLight_Core.Models.Authentication;

namespace StarLight_Core.Authentication
{
    public class OfflineAuthentication
    {
        public string AccessToken { get; set; }
        public string ClientToken { get; set; }
        public string Name { get; set; }
        public string Uuid { get; set; }
        
        public OfflineAuthentication(string username)
        {
            AccessToken = AccessToken = Guid.NewGuid().ToString("N");
            ClientToken = Guid.NewGuid().ToString("N");
            Name = username;
            Uuid = Guid.NewGuid().ToString();
        }
        
        public OfflineAuthentication(string username, string uuid)
        {
            AccessToken = AccessToken = Guid.NewGuid().ToString("N");
            ClientToken = Guid.NewGuid().ToString("N");
            Name = username;
            Uuid = uuid;
        }
        
        public OfflineAuthentication(string username, string uuid, string accessToken, string clientToken)
        {
            AccessToken = accessToken;
            ClientToken = clientToken;
            Name = username;
            Uuid = uuid;
        }
        
        public BaseAccount OfflineAuth()
        {
            if (!IsValidUuid(Uuid)) {
                Uuid = Guid.NewGuid().ToString("N");
            }
            
            OfflineAccount result = new OfflineAccount()
            {
                Name = Name,
                Uuid = Uuid,
                AccessToken = AccessToken,
                ClientToken = ClientToken,
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
