using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NZ.RdpMaid.App.Settings;

namespace NZ.RdpMaid.App.Core.Services
{
    internal class FileStorage(AppSettingsProvider settingsProvider)
    {
        public string ResolveFilePath(string name)
        {
            var dir = EnsureDataDirectoryExists();
            return Path.Combine(dir.FullName, name);
        }

        public bool FileExists(string name)
        {
            var path = ResolveFilePath(name);
            return File.Exists(path);
        }

        public void CreateTextFile(string name, string content)
        {
            var path = ResolveFilePath(name);
            File.WriteAllText(path, content);
        }

        public string ReadTextFile(string name)
        {
            var path = ResolveFilePath(name);
            return File.ReadAllText(path);
        }

        public void CreateBinaryFile(string name, byte[] content)
        {
            var path = ResolveFilePath(name);
            File.WriteAllBytes(path, content);
        }

        public async Task CreateBinaryFileAsync(string name, byte[] content, CancellationToken ct = default)
        {
            var path = ResolveFilePath(name);
            await File.WriteAllBytesAsync(path, content, ct);
        }

        public byte[] ReadBinaryFile(string name)
        {
            var path = ResolveFilePath(name);
            return File.ReadAllBytes(path);
        }

        public DirectoryInfo EnsureDataDirectoryExists()
        {
            if (settingsProvider.Settings.UseAppDataDirectoryForStorage)
            {
                var appDataDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "NZ.RdpMaid"
                );

                if (!Directory.Exists(appDataDir))
                {
                    Directory.CreateDirectory(appDataDir);
                }

                return new DirectoryInfo(appDataDir);
            }

            return new DirectoryInfo(Directory.GetCurrentDirectory());
        }
    }
}