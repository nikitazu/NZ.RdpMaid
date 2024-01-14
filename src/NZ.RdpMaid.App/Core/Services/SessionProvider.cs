using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using NZ.RdpMaid.App.Exceptions;
using NZ.RdpMaid.App.Settings;

namespace NZ.RdpMaid.App.Core.Services
{
    internal class SessionProvider(AppSettingsProvider settings, FileStorage fileStorage)
    {
        private const string _sessionResourceName = "TemplateSession.rdp";
        private readonly AppSettingsProvider _settingsProvider = settings ?? throw new ArgumentNullException(nameof(settings));
        private readonly FileStorage _fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));

        public (string Error, string Hint)? TestCanCreateSession()
        {
            try
            {
                CreateSessionFile("1337");
                return null;
            }
            catch (AppSettingsNotFoundException ex)
            {
                return (Error: ex.Message, ex.Hint);
            }
            catch (Exception ex)
            {
                return (
                    Error: ex.Message,
                    Hint:
                        "Настройте доступ на запись в каталог программы"
                        + " или переместите её в пользовательский каталог"
                        + " или поменяйте настройку UseAppDataDirectoryForStorage на значение true"
                        + " в файле appsettings.json и перезапустите программу."
                );
            }
        }

        public void CreateSession(string code)
        {
            CreateSessionFile(code);
            Process.Start("explorer.exe", _fileStorage.ResolveFilePath("session.rdp"));
        }

        private void CreateSessionFile(string code)
        {
            string template;

            if (_fileStorage.FileExists(_sessionResourceName))
            {
                template = _fileStorage.ReadTextFile(_sessionResourceName);
            }
            else
            {
                var assemblyPath = Assembly.GetExecutingAssembly().Location;
                var assemblyDir = Path.GetDirectoryName(assemblyPath)
                    ?? throw new InvalidOperationException("Не удалось найти каталог приложения");

                var sessionTemplatePath = Path.Combine(assemblyDir, "Resources", "Sessions", _sessionResourceName);
                template = File.ReadAllText(sessionTemplatePath);

                _fileStorage.CreateTextFile(_sessionResourceName, template);
            }

            var content = template
                .Replace("__GATEWAYHOSTNAME__", _settingsProvider.Settings.GatewayHostname)
                .Replace("__LOGIN__", _settingsProvider.Settings.Login)
                .Replace("__IMAPINCODE__", code)
                .Replace("__DOMAIN__", _settingsProvider.Settings.Domain)
                .Replace("__ADDRESS__", _settingsProvider.Settings.Address);

            _fileStorage.CreateTextFile("session.rdp", content);
        }
    }
}