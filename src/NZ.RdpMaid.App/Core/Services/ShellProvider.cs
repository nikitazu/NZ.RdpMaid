using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace NZ.RdpMaid.App.Core.Services;

internal class ShellProvider
{
    public void OpenUrl(string url)
    {
        if (!string.IsNullOrEmpty(url))
        {
            OpenExplorer(url);
        }
    }

    public void OpenFile(string path)
    {
        if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
        {
            OpenExplorer(path);
        }
    }

    public async Task<bool> RunUpdater((string, string)[]? env)
    {
        var updaterPath = Path.Combine("__updater", "NZ.RdpMaid.Updater.exe");
        var start = new ProcessStartInfo
        {
            FileName = updaterPath,
        };

        if (env is not null)
        {
            foreach (var (key, value) in env)
            {
                start.Environment[key] = value;
            }
        }

        var process = Process.Start(start);

        await Task.Delay(200);

        return process is not null && !process.HasExited;
    }

    private static void OpenExplorer(string args)
    {
        Process.Start("explorer.exe", args);
    }
}