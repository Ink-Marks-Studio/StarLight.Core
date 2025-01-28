using System.IO.Compression;

namespace StarLight_Core.Utilities;

/// <summary>
/// 解压工具类
/// </summary>
public static class ZipUtil
{
    /// <summary>
    /// 解压工具
    /// </summary>
    /// <param name="zipFilePath"></param>
    /// <param name="targetDirectoryPath"></param>
    /// <exception cref="Exception"></exception>
    public static async Task DecompressZipFileAsync(string zipFilePath, string targetDirectoryPath)
    {
        try
        {
            FileUtil.IsDirectory(targetDirectoryPath, true);

            using var zipArchive = ZipFile.OpenRead(zipFilePath);
            foreach (var entry in zipArchive.Entries)
            {
                var completeFileName = Path.Combine(targetDirectoryPath, entry.FullName);
                var directoryPath = Path.GetDirectoryName(completeFileName);
                if (directoryPath != null && !Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

                await using var fileStream = new FileStream(completeFileName, FileMode.Create, FileAccess.Write,
                    FileShare.None, 4096, true);
                await entry.Open().CopyToAsync(fileStream);
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"解压 ZIP 文件时发生错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 解压 Natives
    /// </summary>
    /// <param name="zipFile"></param>
    /// <param name="targetDirectory"></param>
    /// <exception cref="Exception"></exception>
    public static async Task ExtractNativesFilesAsync(string zipFile, string targetDirectory)
    {
        try
        {
            using var zipArchive = ZipFile.OpenRead(zipFile);
            foreach (var entry in zipArchive.Entries)
                try
                {
                    var fileExtension = Path.GetExtension(entry.Name);
                    if (!fileExtension.Contains(".dll")) continue;
                    var completeFileName = Path.Combine(targetDirectory, entry.Name);
                    await using var fileStream = new FileStream(completeFileName, FileMode.Create, FileAccess.Write,
                        FileShare.None, 4096, true);
                    await entry.Open().CopyToAsync(fileStream);
                }
                catch (UnauthorizedAccessException)
                {
                    throw new Exception($"无权限访问 Natives 文件: {entry.FullName}");
                }
        }
        catch (Exception e)
        {
            throw new Exception($"无法解压 Natives 文件: {e}");
        }
    }
}