using System.Xml.Linq;

namespace NZ.RdpMaid.App.SerializationModels.AtomRss;

internal static class Schema
{
    public static readonly XNamespace Ns = "http://www.w3.org/2005/Atom";
    public static readonly XName Entry = Ns + "entry";
    public static readonly XName Updated = Ns + "updated";
    public static readonly XName Title = Ns + "title";
    public static readonly XName Link = Ns + "link";
    public static readonly XName Content = Ns + "content";
    public static readonly XName Author = Ns + "author";
}