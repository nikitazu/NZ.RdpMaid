using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NZ.RdpMaid.App.Core.Services;

namespace NZ.RdpMaid.App.EventModel
{
    internal record LoadLocalizationEventModel : INotification
    {
        public static readonly LoadLocalizationEventModel Instance = new();
    }

    internal class LoadLocalizationEventConsumer(
        MainWindowViewModel mainWindow,
        SensitiveDataProvider sensitiveDataProvider,
        PinCodeProvider pinCodeProvider
        ) : INotificationHandler<LoadLocalizationEventModel>
    {
        private readonly MainWindowViewModel _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
        private readonly SensitiveDataProvider _sensitiveDataProvider = sensitiveDataProvider ?? throw new ArgumentNullException(nameof(sensitiveDataProvider));
        private readonly PinCodeProvider _pinCodeProvider = pinCodeProvider ?? throw new ArgumentNullException(nameof(pinCodeProvider));

        public Task Handle(LoadLocalizationEventModel notification, CancellationToken cancellationToken)
        {
            var isPinCodeSourceAvailable =
                !string.IsNullOrWhiteSpace(_sensitiveDataProvider.GetPinCodeSource());

            var lifetimeSeconds = _pinCodeProvider.GetLifetimeSeconds();

            _mainWindow.WelcomeText =
                isPinCodeSourceAvailable
                    ? LocalizationProvider.AutoPinCodeMessage
                    : LocalizationProvider.EnterPinCodeMessage;

            _mainWindow.PinCodeLifetimeText =
                lifetimeSeconds is not null
                    ? LocalizationProvider.GetPinCodeLifetimeMessage(lifetimeSeconds.Value)
                    : string.Empty;

            return Task.CompletedTask;
        }
    }
}