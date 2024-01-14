using Microsoft.Extensions.DependencyInjection;
using NZ.RdpMaid.App.UiServices;

namespace NZ.RdpMaid.App.DependencyConfiguration
{
    internal static class UiModule
    {
        public static IServiceCollection AddUiModule(this IServiceCollection services)
        {
            return services

                // UI Services
                //

                .AddSingleton<DialogProvider>()
                .AddSingleton<MainWindowFactory>()
                .AddSingleton<ThemeLoader>()

                // Commands
                //

                .AddSingleton<PublishCommand>()

                // View Models
                //

                .AddSingleton<PasswordPromptWindowViewModel>()
                .AddSingleton<PinCodeSourcePromptWindowViewModel>()
                .AddSingleton<MainWindowViewModel>();
        }
    }
}