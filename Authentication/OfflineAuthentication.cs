using System.Text.RegularExpressions;
using StarLight_Core.Enum;
using StarLight_Core.Models.Authentication;

namespace StarLight_Core.Authentication
{
    public class OfflineAuthentication : BaseAuthentication
    {
        private string AccessToken { get; set; }
        private string Name { get; set; }
        private string Uuid { get; set; }
        
        public OfflineAuthentication(string username)
        {
            AccessToken = Guid.NewGuid().ToString("N");
            ClientToken = Guid.NewGuid().ToString("N");
            Name = username;
            Uuid = Guid.NewGuid().ToString();
        }
        
        public OfflineAuthentication(string username, string uuid)
        {
            AccessToken = Guid.NewGuid().ToString("N");
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
    }
}
