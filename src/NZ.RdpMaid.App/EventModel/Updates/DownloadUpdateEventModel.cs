using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NZ.RdpMaid.App.Core.Services;
using NZ.RdpMaid.App.Core.Services.HttpClients;
using NZ.RdpMaid.App.Views;

namespace NZ.RdpMaid.App.EventModel.Updates
{
    internal record DownloadUpdateEventModel : INotification;

    internal class DownloadUpdateEventConsumer(
        UpdateViewModel model,
        GithubUpdateClient updateClient,
        FileStorage fileStorage,
        ShellProvider shell
    )
        : INotificationHandler<DownloadUpdateEventModel>
    {
        private readonly UpdateViewModel _model = model ?? throw new ArgumentNullException(nameof(model));
        private readonly GithubUpdateClient _updateClient = updateClient ?? throw new ArgumentNullException(nameof(updateClient));
        private readonly FileStorage _fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
        private readonly ShellProvider _shell = shell ?? throw new ArgumentNullException(nameof(shell));

        public async Task Handle(DownloadUpdateEventModel notification, CancellationToken ct)
        {
            if (_model.CurrentStatus != UpdateViewModel.Status.WaitingForDownloadOrder)
            {
                return;
            }

            if (_model.PendingUpdate is null)
            {
                _model.AddLog("Что-то пошло не так! Обновление не инициализировано!");
                return;
            }

            _model.CurrentStatus = UpdateViewModel.Status.Downloading;
            _model.AddLog($"Качаю обновление: {_model.PendingUpdate!.Version}");

            var progress = new Progress<float>();
            progress.ProgressChanged += (_, value) =>
            {
                _model.DownloadProgressValue = (int)(value * 100);
            };

            _model.DownloadProgressValue = 0;
            var response = await _updateClient.DownloadUpdate(_model.PendingUpdate, progress, ct);
            _model.DownloadProgressValue = 100;

            if (response.Status == GithubUpdateClient.DownloadStatus.Failed)
            {
                _model.AddLogIfNotEmpty(response.Error);
                _model.AddLog("Не удалось скачать обновление, попробуйте ещё раз");
                _model.CurrentStatus = UpdateViewModel.Status.WaitingForDownloadOrder;

                return;
            }

            _model.AddLog($"Скачал {response.Data.Length} байт");
            _fileStorage.EnsureDataDirectoryExists();

            await _fileStorage.CreateBinaryFileAsync("update.zip", response.Data, ct);
            var path = _fileStorage.ResolveFilePath("update.zip");

            _model.AddLog($"Записал обновление на диск: {path}");
            _shell.OpenFile(path);
            _model.CurrentStatus = UpdateViewModel.Status.WaitingForInstallOrder;
        }
    }
}