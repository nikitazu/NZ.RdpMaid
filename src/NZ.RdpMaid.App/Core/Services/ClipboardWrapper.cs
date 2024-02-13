using System.Windows;

namespace NZ.RdpMaid.App.Core.Services;

internal class ClipboardWrapper
{
    public string GetText()
    {
        // try/catch здесь и ниже нужен, т.к. иногда доступ к буферу обмена падает без особых явных причин
        try
        {
            return Clipboard.GetText() ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    public void SetText(string text)
    {
        try
        {
            Clipboard.SetText(text ?? string.Empty);
        }
        catch { }
    }

    public void ClearText()
    {
        SetText(string.Empty);
    }
}