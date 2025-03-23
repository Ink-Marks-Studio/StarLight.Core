namespace StarLight_Core.Models.Launch;

/// <summary>
/// 核心配置信息类
/// </summary>
public class GameCoreConfig
{
    /// <summary>
    /// 游戏根目录路径
    /// </summary>
    public string Root { get; set; } = ".minecraft";

    /// <summary>
    /// 游戏版本
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    /// 版本隔离
    /// </summary>
    public bool IsVersionIsolation { get; set; } = true;
    
    /// <summary>
    /// 自动加入服务器的地址
    /// </summary>
    public string Ip { get; set; }

    /// <summary>
    /// 自动加入服务器的端口
    /// </summary>
    public string Port { get; set; }

    /// <summary>
    /// 版本类型信息
    /// </summary>
    public string VersionTypeInfo { get; set; }

    /// <summary>
    /// 统一通行证服务器 ID
    /// </summary>
    public string UnifiedPassServerId { get; set; }

    /// <summary>
    /// Nide8auth 组件路径
    /// </summary>
    public string Nide8authPath { get; set; }
    
    /// <summary>
    /// authlib-injector 组件路径
    /// </summary>
    public string AuthlibPath { get; set; }
    
    /// <summary>
    /// 皮肤站服务器 URL
    /// </summary>
    public string AuthlibServerUrl { get; set; }

    /// <summary>
    /// 游戏语言
    /// 默认值为 GameLanguage.zh_cn（中文简体）
    /// </summary>
    public GameLanguage Language { get; set; } = GameLanguage.zh_cn;

    /// <summary>
    /// 附加游戏启动参数
    /// </summary>
    public IEnumerable<string> GameArguments { get; set; }
}