<div align="center">

<img Height="300" Width="300" src="Assets/logo-256.png"/>

# StarLight.Core

中文 / English

#### 一个高效,模块化,全能的我的世界启动器核心


![Star](https://img.shields.io/github/stars/Ink-Marks-Studio/StarLight.Core?logo=github&label=Star&style=for-the-badge)
![Forks](https://img.shields.io/github/forks/Ink-Marks-Studio/StarLight.Core?logo=github&label=Forks&style=for-the-badge)
![NugetVersion](https://img.shields.io/nuget/v/StarLight_Core?logo=nuget&label=Nuget包版本&style=for-the-badge)
![NugetDownload](https://img.shields.io/nuget/dt/StarLight_Core?logo=nuget&label=Nuget包下载量&style=for-the-badge)
![Issues](https://img.shields.io/github/issues-closed/Ink-Marks-Studio/StarLight.Core?logo=github&label=Issues&style=for-the-badge)
![PR](https://img.shields.io/github/issues-pr-closed/Ink-Marks-Studio/StarLight.Core?logo=github&label=Pull%20requests&style=for-the-badge)
![License](https://img.shields.io/github/license/Ink-Marks-Studio/StarLight.Core?logo=github&label=开源协议&style=for-the-badge&color=ff7a35)


![Alt](https://repobeats.axiom.co/api/embed/ba6e9977d1c23baebac22caa8629dc6f2ae14dd9.svg "Repobeats analytics image")

</div>

<br></br>

## ✨特点
- 🚀全功能:
  除了启动外, 还支持多种登录方式, 原版与其他加载器的安装, 整合包解析还有很多封装好的小工具

- 📦模块化:
  主要有启动, 安装, 验证与工具集, 模块化的核心使您的使用更加简单

- 📖全开源:
  您可以在 Github 上查看完全开放的源代码, 使用 MIT 协议, 您可以参考学习等

## 📜支持列表

> [!TIP]
> StarLight.Core 仍在积极开发中, 但由于开发者学业繁重, 我们非常欢迎您参与到项目的开发

✅: 已支持的功能

☑️: 开发中的功能, 或已完成但为进行测试

❌: 还未开始开发, 将在未来进行支持的功能

🧱: 未在表中的内容, 未计划支持功能或不会支持的功能

| 功能            | 状态 |
|---------------|----|
| 启动游戏          | ✅  |
| 游戏搜寻器         | ✅  |
| Java 搜寻器      | ✅  |
| 离线验证器         | ✅  |
| 微软验证器         | ✅  |
| 外置验证器         | ✅  |
| 统一通行证         | ✅  |
| 原版游戏安装        | ✅  |
| Fabric 安装器    | ✅  |
| Forge 安装器     | ☑️ |
| NeoForge 安装器  | ❌  |
| Optifine 安装器  | ❌  |
| 多线程下载器        | ✅  |
| 皮肤处理器         | ✅  |
| 模组处理器         | ☑️ |
| 整合包处理器        | ❌  |
| CurseForge 下载 | ❌  |
| Modrinth 下载   | ❌  |
| 错误分析器         | ❌  |

## 📘文档
[StarLight_Core 使用文档与使用帮助](https://mohen.wiki/)

## 🗒️快速开始

### 1.先决条件

> [!IMPORTANT]
> 若您需要使用 Native AOT 发布, 则必须使用 C# .NET 8.0

> [!TIP]
> 使用较新的 C# .NET 版本可以获得较好的 Json 解析性能

1.你的项目必须是使用C# .NET 6.0及以上.

2.你的项目是在 Windows 平台为目标进行开发, 跨平台将在日后支持

### 2.下载
a. 通过任意包管理器搜索 `StarLight_Core` 进行安装

b. 通过命令行进行安装
```shell
dotnet add package StarLight_Core
```

### 2-2.手动下载
a. 在 [Nuget](https://www.nuget.org/packages/StarLight_Core) 中下载

b. 在 [Github Packages](https://github.com/orgs/Ink-Marks-Studio/packages?repo_name=StarLight.Core) 中下载

### 3.添加需要的引用
```csharp
using StarLight_Core.Utilities;
using StarLight_Core.Authentication;
using StarLight_Core.Launch;
using StarLight_Core.Models.Launch;
```

> [!TIP]
> 部分的 IDE 支持引用的自动添加

### 4.获取已安装的游戏
```csharp
var gameCore = GameCoreUtil.GetGameCores();
```

### 5.添加账户
```csharp
var account = new OfflineAuthentication("Steve").OfflineAuth();
```
> [!NOTE]
> 更多验证器请查看 [文档](https://mohen.wiki/)-验证器 部分

### 6.启动游戏
```csharp
LaunchConfig args = new() // 配置启动参数
{
    Account = new()
    {
        BaseAccount = account // 账户
    },
    GameCoreConfig = new()
    {
        Root = ".minecraft", // 游戏根目录
        Version = "1.18.2" // 启动的版本
    },
    JavaConfig = new()
    {
        JavaPath = "C:\\Program Files\\Java\\jdk-18.0.2.1\\bin\\javaw.exe", // Java 路径
        MaxMemory = 4096 // 最大内存
    }
};
var launch = new MinecraftLauncher(args); // 实例化启动器
var la = await launch.LaunchAsync(ReportProgress); // 启动
```
> [!NOTE]
> 更多启动配置以及错误处理请查看 [文档](https://mohen.wiki/)-启动器 部分

### 7.更多组件
在文档 [StarLight_Core 使用文档与使用帮助](https://mohen.wiki/)
中查看更多组件的使用教程

## 🌐讨论
欢迎加入Q群: [971192670](https://qm.qq.com/q/FcmJDYRoDQ)

[StarLight-启动器开发交流群](https://qm.qq.com/q/FcmJDYRoDQ)

如有问题或新功能建议, 请提交 [Issue](https://github.com/Ink-Marks-Studio/StarLight.Core/issues)

有其他需要可以加入Q群或发送邮箱至
[StarLight@InkMarks.Studio](mailto:starlight@inkmarks.studio)
