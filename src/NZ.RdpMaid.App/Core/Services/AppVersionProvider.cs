using System;

namespace NZ.RdpMaid.App.Core.Services
{
    internal class AppVersionProvider
    {
        private Version? _version;

        public Version Current
        {
            get
            {
                if (_version is not null)
                {
                    return _version;
                }

                var assemblyVersion = typeof(MainWindowViewModel).Assembly.GetName().Version;

                _version = assemblyVersion is null
                    ? new Version(0, 0)
                    : new Version(assemblyVersion.Major, assemblyVersion.Minor, assemblyVersion.Build);

                return _version;
            }
        }
    }
}