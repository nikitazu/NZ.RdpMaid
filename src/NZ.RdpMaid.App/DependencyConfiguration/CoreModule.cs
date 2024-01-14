using Microsoft.Extensions.DependencyInjection;
using NZ.RdpMaid.App.Core.Services;

namespace NZ.RdpMaid.App.DependencyConfiguration
{
    internal static class CoreModule
    {
        public static IServiceCollection AddCoreModule(this IServiceCollection services) => services
            .AddSingleton<FileStorage>()
            .AddSingleton<PinCodeProvider>()
            .AddSingleton<PinCodeSourceImporter>()
            .AddSingleton<SafeStorage>()
            .AddSingleton<SensitiveDataProvider>()
            .AddSingleton<SessionProvider>()
            .AddSingleton<StateStorage>();
    }
}