using StarLight_Core.Enum;

namespace StarLight_Core.Models.Mod;

public static class ModSourceConfig
{
    public static int ModSource = 0;
    public static void SwitchModSource(ModDownloadSource source)
    {
        switch (source)
        {
            case ModDownloadSource.Official:
                ModSource = 0;
                break;
            case ModDownloadSource.MCIM:
                ModSource = 1;
                break;
        }
    }
}