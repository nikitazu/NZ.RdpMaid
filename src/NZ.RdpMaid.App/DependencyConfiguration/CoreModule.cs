using Microsoft.Extensions.DependencyInjection;
using NZ.RdpMaid.App.Core.Services;
using NZ.RdpMaid.App.Core.Services.HttpClients;

namespace NZ.RdpMaid.App.DependencyConfiguration
{
    internal static class CoreModule
    {
        public static IServiceCollection AddCoreModule(this IServiceCollection services) => services
            .AddHttpClientServices()
            .AddSingleton<AppVersionProvider>()
            .AddSingleton<ClipboardWrapper>()
            .AddSingleton<DebouncingTriggerService>()
            .AddSingleton<FileStorage>()
            .AddSingleton<PinCodeProvider>()
            .AddSingleton<PinCodeSourceImporter>()
            .AddSingleton<SafeStorage>()
            .AddSingleton<SensitiveDataProvider>()
            .AddSingleton<SessionProvider>()
            .AddSingleton<StateStorage>();

        private static IServiceCollection AddHttpClientServices(this IServiceCollection services)
        {
            services.AddHttpClient<GithubUpdateClient>();
            return services;
        }
    }
}