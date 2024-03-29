﻿using System;
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
        FileStorage fileStorage
    )
        : INotificationHandler<DownloadUpdateEventModel>
    {
        private readonly UpdateViewModel _model = model ?? throw new ArgumentNullException(nameof(model));
        private readonly GithubUpdateClient _updateClient = updateClient ?? throw new ArgumentNullException(nameof(updateClient));
        private readonly FileStorage _fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));

        private readonly Progress<float> _progress = new();

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

            _model.DownloadProgressValue = 0.0F;
            _progress.ProgressChanged += (_, value) => _model.DownloadProgressValue = value;
            var response = await _updateClient.DownloadUpdate(_model.PendingUpdate, _progress, ct);
            _model.DownloadProgressValue = 1.0F;

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
            _model.CurrentStatus = UpdateViewModel.Status.WaitingForInstallOrder;
        }
    }
}