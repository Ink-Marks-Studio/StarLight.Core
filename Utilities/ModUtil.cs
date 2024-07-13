using StarLight_Core.Models.Utilities;

namespace StarLight_Core.Utilities
{
    // 模组工具
    public class ModUtil
    {
        public static IEnumerable<ModInfo> GetModsInfo(string root)
        {
            FileUtil.IsDirectory(root, true);
            
            if (!FileUtil.IsAbsolutePath(root))
            {
                root = FileUtil.GetCurrentExecutingDirectory() + root;
            }
            
            return null;
        }
    }
}