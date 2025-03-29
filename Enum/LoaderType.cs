using System.Text;

namespace StarLight_Core.Enum;

/// <summary>
/// 加载器类型枚举
/// </summary>
[Flags]
public enum LoaderType
{
    /// <summary>
    /// 原版
    /// </summary>
    Vanilla=0,
    /// <summary>
    /// Fabric 加载器
    /// </summary>
    Fabric=1,
    /// <summary>
    /// Forge 加载器
    /// </summary>
    Forge=2,
    /// <summary>
    /// 较早的 Forge 加载器
    /// </summary>
    ForgeLegacy=4,
    /// <summary>
    /// Quilt 加载器
    /// </summary>
    Quilt=8,
    /// <summary>
    /// 高清修复
    /// </summary>
    OptiFine=16,
    /// <summary>
    /// LiteLoader 加载器
    /// </summary>
    LiteLoader=32,
    /// <summary>
    /// NeoForge加载器
    /// </summary>
    NeoForge=64
    
}


/// <summary>
/// 提供用于处理LoaderType枚举的方法
/// </summary>
public static class LoaderTypeHelp
{
    static readonly LoaderType[] _allTypes =
    [
        LoaderType.Fabric, LoaderType.Forge, LoaderType.ForgeLegacy, LoaderType.Quilt, LoaderType.OptiFine,
        LoaderType.LiteLoader, LoaderType.NeoForge
    ];
    public static IEnumerable<LoaderType> GetAll(this LoaderType thisObj)
    {
        List<LoaderType> list = [];
        list.AddRange(_allTypes.Where(item => thisObj.HasFlag(item)));
        return list;
    }
    /// <summary>
    /// 改了名字的hasFlag
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="thisObj">当前对象</param>
    /// <returns></returns>
    public static bool Contain(this LoaderType thisObj,LoaderType obj)
    {
        return thisObj.HasFlag(obj);
    }
    
    /// <summary>
    /// 重写ToString?
    /// </summary>
    /// <returns></returns>
    public static string ToNormalString(this LoaderType obj)
    {
        StringBuilder str = new();
        foreach (var item in GetAll(obj))
        {
            str.Append(item + " ");
        }
        return str.ToString();
    }
}