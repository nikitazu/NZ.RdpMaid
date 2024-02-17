using System.Xml.Linq;

namespace NZ.RdpMaid.App.Core.Utils;

internal static class XDocumentUtil
{
    public static bool TryParse(string input, out XDocument xml)
    {
        try
        {
            xml = XDocument.Parse(input);
        }
        catch
        {
            xml = null!;
        }

        return xml is not null;
    }
}