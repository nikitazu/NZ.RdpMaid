using System;

namespace NZ.RdpMaid.App.Core.Services.HttpClients
{
    public record UpdateModel(
        DateTimeOffset Updated,
        string Link,
        Version Version,
        string Content,
        string Author);
}