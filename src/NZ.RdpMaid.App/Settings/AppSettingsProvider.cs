using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using NZ.RdpMaid.App.Exceptions;

namespace NZ.RdpMaid.App.Settings
{
    internal class AppSettingsProvider
    {
        private AppSettings? _settings = null;

        public AppSettings Settings =>
            _settings ??= InitAppSettings();

        private static AppSettings InitAppSettings()
        {
            var currentDir = Directory.GetCurrentDirectory();
            var appDataDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "NZ.RdpMaid"
            );

            var appSettingsDir =
                File.Exists(Path.Combine(currentDir, "appsettings.json"))
                    ? currentDir
                    : appDataDir;

            if (!File.Exists(Path.Combine(appSettingsDir, "appsettings.json")))
            {
                throw new AppSettingsNotFoundException(
                    message: "Невозможно запустить программу, т.к. отсутствует файл настроек",
                    hint: "Убедитесь что файл appsettings.json присутствует в одном из следующих путей:\n"
                    + $"<{currentDir}>\n"
                    + $"<{appDataDir}>"
                );
            }

            var builder = new ConfigurationBuilder()
                .SetBasePath(appSettingsDir)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);

            var config = builder.Build();
            var settings = new AppSettings();

            config.Bind(settings);

            return settings;
        }
    }
}
