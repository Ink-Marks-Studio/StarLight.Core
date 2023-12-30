using StarLight_Core.Enum;

namespace StarLight_Core.Models.Authentication;

public class OfflineAccount : BaseAccount
{
    public override AuthType Type => AuthType.Offline;
}