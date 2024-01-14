using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NZ.RdpMaid.App.UiServices;

namespace NZ.RdpMaid.App.EventModel
{
    internal record SetupPinCodeSourceEventModel : INotification;

    internal class SetupPinCodeSourceEventConsumer(DialogProvider dialogProvider) : INotificationHandler<SetupPinCodeSourceEventModel>
    {
        private readonly DialogProvider _dialogProvider = dialogProvider ?? throw new ArgumentNullException(nameof(dialogProvider));

        public Task Handle(SetupPinCodeSourceEventModel notification, CancellationToken cancellationToken)
        {
            _dialogProvider.ShowDialog<PinCodeSourcePromptWindow, PinCodeSourcePromptWindowViewModel>();

            return Task.CompletedTask;
        }
    }
}