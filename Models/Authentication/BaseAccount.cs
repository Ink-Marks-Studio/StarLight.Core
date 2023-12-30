using StarLight_Core.Enum;

namespace StarLight_Core.Models.Authentication;

public class BaseAccount
{
    public string Name { get; set; }
    
    public string Uuid { get; set; }
    
    public string AccessToken { get; set; }
    
    public string ClientToken { get; set;}
    
    public virtual AuthType Type { get; set;}
}