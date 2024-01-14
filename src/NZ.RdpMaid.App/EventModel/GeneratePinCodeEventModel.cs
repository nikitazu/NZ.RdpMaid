using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NZ.RdpMaid.App.Core.Services;
using NZ.RdpMaid.App.Settings;

namespace NZ.RdpMaid.App.EventModel
{
    internal record GeneratePinCodeEventModel : INotification
    {
        public static readonly GeneratePinCodeEventModel Instance = new();
    }

    internal class GeneratePinCodeEventConsumer(
        MainWindowViewModel mainWindow,
        AppSettingsProvider settingsProvider,
        PinCodeProvider pinCodeProvider
        ) : INotificationHandler<GeneratePinCodeEventModel>
    {
        private readonly MainWindowViewModel _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
        private readonly AppSettingsProvider _settingsProvider = settingsProvider ?? throw new ArgumentNullException(nameof(settingsProvider));
        private readonly PinCodeProvider _pinCodeProvider = pinCodeProvider ?? throw new ArgumentNullException(nameof(pinCodeProvider));

        public Task Handle(GeneratePinCodeEventModel notification, CancellationToken cancellationToken)
        {
            if (_settingsProvider.Settings.UsePinCodeGenerator)
            {
                var pinCode = _pinCodeProvider.GetPinCode();
                var lifetimeSeconds = _pinCodeProvider.GetLifetimeSeconds();

                if (!string.IsNullOrWhiteSpace(pinCode) && _mainWindow.PinCode != pinCode)
                {
                    _mainWindow.PinCode = pinCode;
                }

                if (lifetimeSeconds is not null)
                {
                    _mainWindow.PinCodeLifetimeText = LocalizationProvider.GetPinCodeLifetimeMessage(lifetimeSeconds.Value);
                }
            }

            return Task.CompletedTask;
        }
    }
}