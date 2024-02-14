using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using NZ.RdpMaid.App.Core.Services;
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
        private static SingleInstanceProvider _appInstance = new();

        internal IServiceProvider? ServiceProvider { get; private set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (_appInstance.IsRunning)
            {
                Shutdown();
            }
            else
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

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            _appInstance?.Dispose();
            _appInstance = null!;
        }
    }
}