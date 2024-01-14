using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using NZ.RdpMaid.App.DependencyConfiguration;
using NZ.RdpMaid.App.Settings;
using NZ.RdpMaid.App.UiServices;

namespace NZ.RdpMaid.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        internal IServiceProvider? ServiceProvider { get; private set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Иньекция
            //

            var services = new ServiceCollection();

            // Регистрация служб
            //

            services
                .AddSingleton<AppSettingsProvider>()
                .AddCoreModule()
                .AddUiModule()
                .AddEventModelModule();

            // Инициализация
            //

            ServiceProvider = services.BuildServiceProvider();
            ServiceProvider.GetRequiredService<ThemeLoader>().LoadTheme();
            ServiceProvider.GetRequiredService<MainWindowFactory>().CreateMainWindow();
        }
    }
}