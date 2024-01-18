using System.IO.Compression;

namespace StarLight_Core.Utilities;

public class ZipUtil
{
    // 解压工具
    public static async Task DecompressZipFileAsync(string zipFilePath, string targetDirectoryPath)
    {
        try
        {
            FileUtil.IsDirectory(targetDirectoryPath, true);
        
            using ZipArchive zipArchive = ZipFile.OpenRead(zipFilePath);
            foreach (ZipArchiveEntry entry in zipArchive.Entries)
            {
                string completeFileName = Path.Combine(targetDirectoryPath, entry.FullName);
            
                string directoryPath = Path.GetDirectoryName(completeFileName);
                if (directoryPath != null && !Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
            
                using (var fileStream = new FileStream(completeFileName, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
                {
                    await entry.Open().CopyToAsync(fileStream);
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"[SL]解压 ZIP 文件时发生错误: {ex.Message}");
        }
    }
    
    // 解压 Natives
    public static async Task ExtractNativesFilesAsync(string zipFile, string targetDirectory)
    {
        try
        {
            using ZipArchive zipArchive = ZipFile.OpenRead(zipFile);
            foreach (ZipArchiveEntry entry in zipArchive.Entries)
            {
                try
                {
                    string fileExtension = Path.GetExtension(entry.Name);
                    if (fileExtension.Contains(".dll"))
                    {
                        string completeFileName = Path.Combine(targetDirectory, entry.Name);
                        using (var fileStream = new FileStream(completeFileName, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
                        {
                            await entry.Open().CopyToAsync(fileStream);
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    throw new Exception($"[SL]无权限访问 Natives 文件: {entry.FullName}");
                }
            }
        }
        catch (Exception e)
        {
            throw new Exception($"[SL]无法解压 Natives 文件: {e}");
        }
    }
}