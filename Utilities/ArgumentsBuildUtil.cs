using StarLight_Core.Models.Utilities;

namespace StarLight_Core.Utilities;

public class ArgumentsBuildUtil
{
    public string BuildLibrariesArgs(string versionId, string root)
    {
        GameCoreUtil.GetGameCore(versionId, root);
        return versionId;
    }
}