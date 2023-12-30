using StarLight_Core.Enum;

namespace StarLight_Core.Models.Authentication;

public class UnifiedPassAccount : BaseAccount
{
    public override AuthType Type => AuthType.UnifiedPass;
    public string ServerId { get; set; }
}