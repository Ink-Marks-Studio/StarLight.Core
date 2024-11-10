using StarLight_Core.Models.Authentication;

namespace StarLight_Core.Authentication;

public class OfflineAuthentication : BaseAuthentication
{
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

    private string AccessToken { get; }
    private string Name { get; }
    private string Uuid { get; set; }

    public OfflineAccount OfflineAuth()
    {
        if (!IsValidUuid(Uuid)) Uuid = Guid.NewGuid().ToString("N");

        var result = new OfflineAccount
        {
            Name = Name,
            Uuid = Uuid,
            AccessToken = AccessToken,
            ClientToken = ClientToken
        };

        return result;
    }
}