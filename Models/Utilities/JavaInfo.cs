namespace StarLight_Core.Models.Utilities;

public class JavaInfo
{
    // Java 版本
    public string JavaVersion { get; set; }

    // Java 路径
    public string JavaPath { get; set; }
    
    // Java 目录
    public string JavaLibraryPath { get; set; }

    // Java 版本缩写
    public int JavaSlugVersion { get; set; }

    // Java 位数
    public bool Is64Bit { get; set; }
}