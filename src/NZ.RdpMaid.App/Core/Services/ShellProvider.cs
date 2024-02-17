using System.Diagnostics;
using System.IO;

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

    private static void OpenExplorer(string args)
    {
        Process.Start("explorer.exe", args);
    }
}