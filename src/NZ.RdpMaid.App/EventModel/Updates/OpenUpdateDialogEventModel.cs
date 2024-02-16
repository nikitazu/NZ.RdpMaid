using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NZ.RdpMaid.App.UiServices;
using NZ.RdpMaid.App.Views;

namespace NZ.RdpMaid.App.EventModel.Updates
{
    internal record OpenUpdateDialogEventModel : INotification;

    internal class OpenUpdateDialogConsumer(DialogProvider dialogProvider) : INotificationHandler<OpenUpdateDialogEventModel>
    {
        private readonly DialogProvider _dialogProvider = dialogProvider ?? throw new ArgumentNullException(nameof(dialogProvider));

        public Task Handle(OpenUpdateDialogEventModel notification, CancellationToken cancellationToken)
        {
            _dialogProvider.ShowDialogView<UpdateView, UpdateViewModel>(title: "NZ.RdpMaid.App | Обновление");

            return Task.CompletedTask;
        }
    }
}