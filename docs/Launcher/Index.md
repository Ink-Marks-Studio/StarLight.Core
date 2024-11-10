# 启动器

用于启动游戏，支持原版核心以及各种 Mod 加载器

支持自动解压游戏文件，补全游戏文件

## 构造函数

```csharp
public MinecraftLauncher(LaunchConfig launchConfig)
{
    GameWindowConfig = launchConfig.GameWindowConfig;
    GameCoreConfig = launchConfig.GameCoreConfig;
    JavaConfig = launchConfig.JavaConfig;
    BaseAccount = launchConfig.Account.BaseAccount;
}
```

| 参数           | 类型           | 描述   |
|--------------|--------------|------|
| launchConfig | LaunchConfig | 启动参数 |

<br>

### 详细 `LaunchConfig` 定义

```csharp
public class LaunchConfig
{
    public Account Account { get; set; } // 账户类型
    
    public GameWindowConfig GameWindowConfig { get; set; } // 游戏窗口配置
    
    public GameCoreConfig GameCoreConfig { get; set; } // 游戏核心配置
    
    public JavaConfig JavaConfig { get; set; } // Java 配置
}
```

| 参数               | 类型               | 描述      |
|------------------|------------------|---------|
| Account          | Account          | 账户类型    |
| GameWindowConfig | GameWindowConfig | 游戏窗口配置  |
| GameCoreConfig   | GameCoreConfig   | 游戏核心配置  |
| JavaConfig       | JavaConfig       | Java 配置 |

<br>

``GameWindowConfig`` 游戏窗口配置

```csharp
public class GameWindowConfig
{
    public int Height { get; set; } = 480; // 窗口高度

    public int Width { get; set; } = 854; // 窗口宽度
        
    public bool IsFullScreen { get; set; } = false; // 启用全屏,自动忽略上面的参数
}
```

<br>

``GameCoreConfig`` 游戏核心配置

```csharp
public class GameCoreConfig
{
    public string Root { get; set; } = ".minecraft"; // 游戏目录(可以是绝对的也可以是相对的,自动判断)
    
    public string Version { get; set; } // 启动的版本
    
    public bool IsVersionIsolation { get; set; } = true; // 版本隔离
    
    public string Ip { get; set; } // 自动进入服务器的 IP
    
    public string Port { get; set; } // 自动进入服务器的端口
    
    public string UnifiedPassServerId { get; set; } // 统一通行证服务器 ID 只有统一通行证需要
    
    public string Nide8authPath { get; set; } // 统一通行证 Nide8auth 路径 只有统一通行证需要
    
    public IEnumerable<string> GameArguments { get; set; } // 附加启动参数
}
```

<br>

``JavaConfig`` Java 配置

```csharp
public class JavaConfig
{
    public string JavaPath { get; set; }  // Java 路径 (完整路径到 Java 可执行文件)

    public int MaxMemory { get; set; } = 2048; // 最大内存

    public int MinMemory { get; set; } = 256; // 最小内存
    
    public bool DisabledOptimizationAdvancedArgs { get; set; } = false; // 启用默认优化参数
    
    public bool DisabledOptimizationGcArgs { get; set; } = false; // 启用默认 GC 优化参数
    
    public IEnumerable<string> AdvancedArguments { get; set; } // 附加 Java 参数
    
    public IEnumerable<string> GCArguments { get; set; } // 附加 GC 参数
}
```