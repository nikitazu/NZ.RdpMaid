using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using NZ.RdpMaid.App.SerializationModels.AtomRss;

namespace NZ.RdpMaid.App.Core.Utils;

internal static class AtomRssParser
{
    public static IEnumerable<AtomEntry> ParseFeed(XElement xfeed)
    {
        ArgumentNullException.ThrowIfNull(xfeed, nameof(xfeed));

        return xfeed
            .Elements(Schema.Entry)
            .Select(ParseEntry);
    }

    public static AtomEntry ParseEntry(XElement xentry)
    {
        ArgumentNullException.ThrowIfNull(xentry, nameof(xentry));

        var xupdated = xentry.Element(Schema.Updated) ?? throw new FormatException("Отсутствует элемент <updated>");
        var xlink = xentry.Element(Schema.Link) ?? throw new FormatException("Отсутствует элемент <link>");
        var xtitle = xentry.Element(Schema.Title) ?? throw new FormatException("Отсутствует элемент <title>");
        var xauthor = xentry.Element(Schema.Author) ?? throw new FormatException("Отсутствует элемент <author>");
        var xcontent = xentry.Element(Schema.Content) ?? throw new FormatException("Отсутствует элемент <content>");

        var updated = DateTimeOffset.Parse(xupdated.Value);
        var link = xlink.Attribute("href")?.Value ?? string.Empty;
        var title = xtitle.Value ?? throw new FormatException("Элемент <title> пуст");
        var author = xauthor.Value ?? throw new FormatException("Элемент <author> пуст");
        var content = xcontent.Value ?? string.Empty;

        return new AtomEntry(
            Updated: updated,
            Link: link,
            Title: title,
            Content: content,
            Author: author);
    }
}