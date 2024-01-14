using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NZ.RdpMaid.App.Core.Services;

namespace NZ.RdpMaid.App.EventModel
{
    internal record SavePinCodeSourceEventModel : INotification;

    internal class SavePinCodeSourceEventConsumer(
        PinCodeSourcePromptWindowViewModel promptModel,
        SensitiveDataProvider sensitiveDataProvider,
        IPublisher pub
        ) : INotificationHandler<SavePinCodeSourceEventModel>
    {
        private readonly PinCodeSourcePromptWindowViewModel _promptModel = promptModel ?? throw new ArgumentNullException(nameof(promptModel));
        private readonly SensitiveDataProvider _sensitiveDataProvider = sensitiveDataProvider ?? throw new ArgumentNullException(nameof(sensitiveDataProvider));
        private readonly IPublisher _pub = pub ?? throw new ArgumentNullException(nameof(pub));

        public Task Handle(SavePinCodeSourceEventModel notification, CancellationToken ct)
        {
            _sensitiveDataProvider.SavePinCodeSource(_promptModel.Source);
            _pub.Publish(InitPinCodeProviderEventModel.Instance, ct);

            return Task.CompletedTask;
        }
    }
}