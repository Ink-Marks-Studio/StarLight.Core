using StarLight_Core.Enum;

namespace StarLight_Core.Models.Authentication;

public class YggdrasilAccount : BaseAccount
{
    public override AuthType Type => AuthType.Yggdrasil;

    public string ServerUrl { get; set; }

    public string Email { get; set; }

    public string Password { get; set; }
}