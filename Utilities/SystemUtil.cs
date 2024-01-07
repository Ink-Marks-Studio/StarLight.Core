using System.Runtime.InteropServices;

namespace StarLight_Core.Utilities
{
    public class SystemUtil
    {
        public static bool IsOperatingSystemGreaterThanWin10()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var osDescription = RuntimeInformation.OSDescription;
                if (osDescription.Contains("Windows"))
                {
                    var versionString = osDescription.Split(" ")[1];
                    var versionParts = versionString.Split('.');

                    if (versionParts.Length >= 2)
                    {
                        if (int.TryParse(versionParts[0], out int majorVersion) && int.TryParse(versionParts[1], out int minorVersion))
                        {
                            return majorVersion > 10 || (majorVersion == 10 && minorVersion > 0);
                        }
                    }
                }
            }
            return false;
        }
    }
}