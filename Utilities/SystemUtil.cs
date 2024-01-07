using Microsoft.Win32;

namespace StarLight_Core.Utilities
{
    public class SystemUtil
    {
        public static bool IsOperatingSystemGreaterThanWin10()
        {
            const string subkey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";
            using (var key = Registry.LocalMachine.OpenSubKey(subkey))
            {
                if (key != null)
                {
                    object currentValue = key.GetValue("CurrentMajorVersionNumber");
                    if (currentValue is int majorVersion)
                    {
                        return majorVersion >= 10;
                    }
                }
            }
            return false;
        }
    }
}