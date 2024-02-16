using System.Diagnostics;

namespace NZ.RdpMaid.App.Core.Services;

internal class BrowserProvider
{
    public void OpenUrl(string url)
    {
        if (!string.IsNullOrEmpty(url))
        {
            Process.Start("explorer.exe", url);
        }
    }
}