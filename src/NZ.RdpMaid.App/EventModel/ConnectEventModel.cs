using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NZ.RdpMaid.App.Core.Constants;
using NZ.RdpMaid.App.Core.Services;

namespace NZ.RdpMaid.App.EventModel
{
    internal record ConnectEventModel : INotification;

    internal class ConnectEventConsumer(
        MainWindowViewModel mainWindow,
        SessionProvider sessionProvider,
        SensitiveDataProvider sensitiveDataProvider,
        DebouncingTriggerService debouncingTriggerService,
        ClipboardWrapper clipboard
    )
        : INotificationHandler<ConnectEventModel>
    {
        private readonly MainWindowViewModel _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
        private readonly SessionProvider _sessionProvider = sessionProvider ?? throw new ArgumentNullException(nameof(sessionProvider));
        private readonly SensitiveDataProvider _sensitiveDataProvider = sensitiveDataProvider ?? throw new ArgumentNullException(nameof(sensitiveDataProvider));
        private readonly DebouncingTriggerService _debouncingTriggerService = debouncingTriggerService ?? throw new ArgumentNullException(nameof(debouncingTriggerService));
        private readonly ClipboardWrapper _clipboard = clipboard ?? throw new ArgumentNullException(nameof(clipboard));

        public Task Handle(ConnectEventModel request, CancellationToken _)
        {
            var pinCode = _mainWindow.PinCode;

            if (!string.IsNullOrWhiteSpace(pinCode) && pinCode.Length == 6)
            {
                _sessionProvider.CreateSession(pinCode);
                var password = _sensitiveDataProvider.GetPassword() ?? string.Empty;
                _clipboard.SetText(password);
                _debouncingTriggerService.Trigger(DebouncingTriggers.ClearClipboardPassword);
            }

            return Task.CompletedTask;
        }
    }
}