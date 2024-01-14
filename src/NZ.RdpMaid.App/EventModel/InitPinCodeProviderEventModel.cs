using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NZ.RdpMaid.App.Core.Services;

namespace NZ.RdpMaid.App.EventModel
{
    internal record InitPinCodeProviderEventModel : INotification
    {
        public static readonly InitPinCodeProviderEventModel Instance = new();
    }

    internal class InitPinCodeProviderEventConsumer(
        SensitiveDataProvider sensitiveDataProvider,
        PinCodeProvider pinCodeProvider,
        IPublisher pub
        ) : INotificationHandler<InitPinCodeProviderEventModel>
    {
        private readonly SensitiveDataProvider _sensitiveDataProvider = sensitiveDataProvider ?? throw new ArgumentNullException(nameof(sensitiveDataProvider));
        private readonly PinCodeProvider _pinCodeProvider = pinCodeProvider ?? throw new ArgumentNullException(nameof(pinCodeProvider));
        private readonly IPublisher _pub = pub ?? throw new ArgumentNullException(nameof(pub));

        public Task Handle(InitPinCodeProviderEventModel notification, CancellationToken ct)
        {
            var pinCodeSource = _sensitiveDataProvider.GetPinCodeSource();

            _pinCodeProvider.SetSource(pinCodeSource);
            _pub.Publish(LoadLocalizationEventModel.Instance, ct);
            _pub.Publish(GeneratePinCodeEventModel.Instance, ct);

            return Task.CompletedTask;
        }
    }
}