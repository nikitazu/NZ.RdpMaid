using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NZ.RdpMaid.App.Core.Services;

namespace NZ.RdpMaid.App.EventModel
{
    internal record SavePasswordEventModel : INotification;

    internal class SavePasswordEventConsumer(
        PasswordPromptWindowViewModel promptModel,
        SensitiveDataProvider provider
        ) : INotificationHandler<SavePasswordEventModel>
    {
        private readonly PasswordPromptWindowViewModel _promptModel = promptModel ?? throw new ArgumentNullException(nameof(promptModel));
        private readonly SensitiveDataProvider _provider = provider ?? throw new ArgumentNullException(nameof(provider));

        public Task Handle(SavePasswordEventModel notification, CancellationToken cancellationToken)
        {
            if (_promptModel.Password == _promptModel.PasswordRepeat)
            {
                _provider.SavePassword(_promptModel.Password);
            }

            return Task.CompletedTask;
        }
    }
}