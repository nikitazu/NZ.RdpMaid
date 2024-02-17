using System;
using NZ.RdpMaid.App.Core.Services.HttpClients;
using NZ.RdpMaid.App.SerializationModels.AtomRss;

namespace NZ.RdpMaid.App.Core.Utils;

internal static class UpdateModelFactory
{
    public static UpdateModel CreateFromAtomRss(AtomEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry, nameof(entry));

        var content = entry.Content
            .Replace("<h2>", string.Empty)
            .Replace("</h2>", string.Empty)
            .Replace("<h3>", string.Empty)
            .Replace("</h3>", string.Empty)
            .Replace("<ul>", string.Empty)
            .Replace("</ul>", string.Empty)
            .Replace("<li>", string.Empty)
            .Replace("</li>", string.Empty);

        var version = Version.Parse(entry.Title);

        return new UpdateModel(
            Updated: entry.Updated,
            Link: entry.Link,
            Version: version,
            Content: content,
            Author: entry.Author);
    }
}