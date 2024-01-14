using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NZ.RdpMaid.App.UiServices;

namespace NZ.RdpMaid.App.EventModel
{
    internal record SetupPasswordEventModel : INotification;

    internal class SetupPasswordEventConsumer(DialogProvider dialogProvider) : INotificationHandler<SetupPasswordEventModel>
    {
        private readonly DialogProvider _dialogProvider = dialogProvider ?? throw new ArgumentNullException(nameof(dialogProvider));

        public Task Handle(SetupPasswordEventModel notification, CancellationToken cancellationToken)
        {
            _dialogProvider.ShowDialog<PasswordPromptWindow, PasswordPromptWindowViewModel>();

            return Task.CompletedTask;
        }
    }
}