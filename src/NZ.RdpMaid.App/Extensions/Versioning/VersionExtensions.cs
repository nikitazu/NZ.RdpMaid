using System;

namespace NZ.RdpMaid.App.Extensions.Versioning;

internal static class VersionExtensions
{
    public static string ToXyzString(this Version version)
    {
        ArgumentNullException.ThrowIfNull(version, nameof(version));

        return $"{version.Major}.{version.Minor}.{version.Build}";
    }
}