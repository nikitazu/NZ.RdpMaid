using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using MediatR;
using NZ.RdpMaid.App.Core.Services;

namespace NZ.RdpMaid.App.EventModel
{
    internal record ConnectEventModel : INotification;

    internal class ConnectEventConsumer(
        MainWindowViewModel mainWindow,
        SessionProvider sessionProvider,
        SensitiveDataProvider sensitiveDataProvider
        ) : INotificationHandler<ConnectEventModel>
    {
        private readonly MainWindowViewModel _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
        private readonly SessionProvider _sessionProvider = sessionProvider ?? throw new ArgumentNullException(nameof(sessionProvider));
        private readonly SensitiveDataProvider _sensitiveDataProvider = sensitiveDataProvider ?? throw new ArgumentNullException(nameof(sensitiveDataProvider));

        public Task Handle(ConnectEventModel request, CancellationToken cancellationToken)
        {
            var pinCode = _mainWindow.PinCode;

            if (!string.IsNullOrWhiteSpace(pinCode) && pinCode.Length == 6)
            {
                _sessionProvider.CreateSession(pinCode);
                var password = _sensitiveDataProvider.GetPassword() ?? string.Empty;
                Clipboard.SetText(password);
            }

            return Task.CompletedTask;
        }
    }
}