using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NZ.RdpMaid.App.Core.Services;
using NZ.RdpMaid.App.Core.Services.HttpClients;
using NZ.RdpMaid.App.Views;

namespace NZ.RdpMaid.App.EventModel.Updates
{
    internal record InstallUpdateEventModel : INotification;

    internal class InstallUpdateEventConsumer(
        UpdateViewModel model,
        GithubUpdateClient updateClient,
        FileStorage fileStorage,
        ShellProvider shell
    )
        : INotificationHandler<InstallUpdateEventModel>
    {
        private static class Keys
        {
            public const string InstallDirPath = "NZ_RDPMAID__INSTALL_DIR_PATH";
            public const string UpdateFilePath = "NZ_RDPMAID__UPDATE_FILE_PATH";
            public const string UserDataDirPath = "NZ_RDPMAID__USER_DATA_DIR_PATH";
        }

        private readonly UpdateViewModel _model = model ?? throw new ArgumentNullException(nameof(model));
        private readonly GithubUpdateClient _updateClient = updateClient ?? throw new ArgumentNullException(nameof(updateClient));
        private readonly FileStorage _fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
        private readonly ShellProvider _shell = shell ?? throw new ArgumentNullException(nameof(shell));

        private readonly Progress<float> _progress = new();

        public async Task Handle(InstallUpdateEventModel notification, CancellationToken ct)
        {
            if (_model.CurrentStatus != UpdateViewModel.Status.WaitingForInstallOrder)
            {
                return;
            }

            if (_model.PendingUpdate is null)
            {
                _model.AddLog("Что-то пошло не так! Обновление не инициализировано!");
                return;
            }

            _model.CurrentStatus = UpdateViewModel.Status.Installing;
            _model.AddLog($"Устанавливаю обновление: {_model.PendingUpdate!.Version}");

            var currentExeDirPath = Path.GetDirectoryName(typeof(InstallUpdateEventModel).Assembly.Location);

            if (string.IsNullOrEmpty(currentExeDirPath))
            {
                _model.AddLog("Не удалось определить каталог приложения");
                return;
            }

            var installPath = Path.GetDirectoryName(currentExeDirPath);

            if (string.IsNullOrEmpty(installPath))
            {
                _model.AddLog("Не удалось определить каталог установки");
                return;
            }

            var updateFilePath = _fileStorage.ResolveFilePath("update.zip");

            if (!File.Exists(updateFilePath))
            {
                _model.AddLog($"Не удалось найти файл обновления: {updateFilePath}");
                return;
            }

            var dataDir = _fileStorage.EnsureDataDirectoryExists();

            _model.AddLog($"Запускаю процесс установки");

            (string, string)[] env = [
                (Keys.InstallDirPath, installPath),
                (Keys.UpdateFilePath, updateFilePath),
                (Keys.UserDataDirPath, dataDir.FullName),
            ];

            _model.AddLog($"Настройка окружения");

            foreach (var (key, value) in env)
            {
                _model.AddLog($" - {key}: {value}");
            }

            bool ok = false;

            try
            {
                // ДЕЛА перед запуском нужно скопировать updater во временный каталог
                // ДЕЛА запустить нужно не сам updater а его копию из временного каталога

                var updaterPath = Path.Combine("__updater", "NZ.RdpMaid.Updater.exe");
                ok = await _shell.RunUpdater(updaterPath, env);
            }
            catch (Exception ex)
            {
                _model.AddLog($"Неожиданная ошибка запуска: {ex.Message}");
            }

            if (!ok)
            {
                _model.AddLog("Не удалось запустить процесс установки :(");
            }
        }
    }
}