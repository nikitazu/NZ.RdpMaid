using System;

namespace NZ.RdpMaid.App.SerializationModels.AtomRss;

internal record AtomEntry(
    DateTimeOffset Updated,
    string Link,
    string Title,
    string Author,
    string Content);