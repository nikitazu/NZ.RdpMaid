using System.IO;
using System.IO.Compression;

namespace NZ.RdpMaid.Updater.Services;

internal static class PackageManager
{
    public static void ExtractPackage(string zipPath, string outputDirPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(zipPath, nameof(zipPath));
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirPath, nameof(outputDirPath));

        if (Directory.Exists(outputDirPath))
        {
            Directory.Delete(outputDirPath, true);
        }

        Directory.CreateDirectory(outputDirPath);
        ZipFile.ExtractToDirectory(zipPath, outputDirPath, overwriteFiles: false);
    }

    public static void BackupApplicationInstallationDirectory(string outputFilePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputFilePath, nameof(outputFilePath));

        var currentDirPath = Path.GetDirectoryName(typeof(MainWindowViewModel).Assembly.Location)!;
        var currentDir = new DirectoryInfo(currentDirPath);

        if (File.Exists(outputFilePath))
        {
            File.Delete(outputFilePath);
        }

        ZipFile.CreateFromDirectory(currentDir.FullName, outputFilePath);
    }

    public static async Task BackupUserDataDirectory(
        string userDataDirPath,
        string outputFilePath,
        string[] excludeFileNames,
        CancellationToken ct
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userDataDirPath, nameof(userDataDirPath));
        ArgumentException.ThrowIfNullOrWhiteSpace(outputFilePath, nameof(outputFilePath));
        ArgumentNullException.ThrowIfNull(excludeFileNames, nameof(excludeFileNames));

        if (File.Exists(outputFilePath))
        {
            File.Delete(outputFilePath);
        }

        using var archive = ZipFile.Open(outputFilePath, ZipArchiveMode.Create);

        foreach (var file in new DirectoryInfo(userDataDirPath).GetFiles())
        {
            if (!excludeFileNames.Contains(file.Name))
            {
                var entry = archive.CreateEntry(file.Name);
                using var entryStream = entry.Open();
                using var fileStream = file.OpenRead();
                await fileStream.CopyToAsync(entryStream, ct);
            }
        }
    }
}