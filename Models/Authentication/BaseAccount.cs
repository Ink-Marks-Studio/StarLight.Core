namespace StarLight_Core.Models.Authentication;

public class BaseAccount
{
    public string Type { get; set; }
    
    public string Name { get; set; }
    
    public string Uuid { get; set; }
    
    public string AccessToken { get; set; }
    
    public string ClientToken { get; set;}
    
    public enum AccountType
    {
        Offline,
        Microsoft,
        UnifiedPass,
        Yggdrasil
    }
}