using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NZ.RdpMaid.App.Core.Services;
using NZ.RdpMaid.App.Core.Services.HttpClients;
using NZ.RdpMaid.App.Views;

namespace NZ.RdpMaid.App.EventModel.Updates
{
    internal record LoadUpdateInfoEventModel : INotification
    {
        public static readonly LoadUpdateInfoEventModel Instance = new();
    }

    internal class LoadUpdateInfoEventConsumer(
        AppVersionProvider appVersion,
        UpdateViewModel model,
        GithubUpdateClient updateClient
    )
        : INotificationHandler<LoadUpdateInfoEventModel>
    {
        private readonly AppVersionProvider _appVersion = appVersion ?? throw new ArgumentNullException(nameof(appVersion));
        private readonly UpdateViewModel _model = model ?? throw new ArgumentNullException(nameof(model));
        private readonly GithubUpdateClient _updateClient = updateClient ?? throw new ArgumentNullException(nameof(updateClient));

        public async Task Handle(LoadUpdateInfoEventModel notification, CancellationToken ct)
        {
            if (_model.CurrentStatus != UpdateViewModel.Status.None)
            {
                return;
            }

            _model.CurrentStatus = UpdateViewModel.Status.CheckingForUpdate;
            _model.AddLog($"Текущая версия: {_appVersion.Current}");
            _model.AddLog("Начинаю проверять, а нет ли обновлений");

            var response = await _updateClient.CheckForUpdates(_appVersion.Current, ct);

            _model.AddLog("Получен ответ от сервера");

            switch (response.Status)
            {
                case GithubUpdateClient.CheckStatus.UpToDate:
                    _model.CurrentStatus = UpdateViewModel.Status.Updated;
                    _model.AddLog("Обновление не требуется");
                    break;

                case GithubUpdateClient.CheckStatus.UpdateFound:
                    _model.CurrentStatus = UpdateViewModel.Status.WaitingForDownloadOrder;
                    _model.PendingUpdate = response.FoundUpdate;
                    _model.AddLog($"Обновление найдено: {response.FoundUpdate?.Version}");
                    break;

                case GithubUpdateClient.CheckStatus.Failed:
                    _model.CurrentStatus = UpdateViewModel.Status.None;
                    _model.AddLog($"Произошла ошибка: {response.Error}");
                    break;

                default:
                    _model.CurrentStatus = UpdateViewModel.Status.None;
                    _model.AddLog($"Произошла ошибка: неизвестное состояние");
                    break;
            }
        }
    }
}