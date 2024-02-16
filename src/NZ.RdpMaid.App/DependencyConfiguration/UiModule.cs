using Microsoft.Extensions.DependencyInjection;
using NZ.RdpMaid.App.UiServices;
using NZ.RdpMaid.App.Views;

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
                .AddSingleton<UpdateViewModel>()
                .AddSingleton<MainWindowViewModel>();
        }
    }
}