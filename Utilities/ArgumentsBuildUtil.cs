using StarLight_Core.Models.Utilities;
using System.Text.Json;

namespace StarLight_Core.Utilities;

public class ArgumentsBuildUtil
{
    public string BuildLibrariesArgs(string versionId, string root)
    {
        GameCoreInfo coreInfo = GameCoreUtil.GetGameCore(versionId, root);
        string versionPath = root + "\\" + versionId + versionId + ".json";
        if (coreInfo.InheritsFrom != null!)
        {
            
        }
        else
        {
            var libraries = JsonSerializer.Deserialize<List<ArgsBuildLibraryJson>>(versionPath);
        }

        return "";
    }
}