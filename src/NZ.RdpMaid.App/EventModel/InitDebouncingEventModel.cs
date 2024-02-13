using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using MediatR;
using NZ.RdpMaid.App.Core.Constants;
using NZ.RdpMaid.App.Core.Services;

namespace NZ.RdpMaid.App.EventModel;

internal record InitDebouncingEventModel : INotification
{
    public static readonly InitDebouncingEventModel Instance = new();
}

internal class InitDebouncingEventModelHandler(
    DebouncingTriggerService debouncingTriggerService,
    SensitiveDataProvider sensitiveDataProvider,
    ClipboardWrapper clipboard
)
    : INotificationHandler<InitDebouncingEventModel>
{
    private readonly DebouncingTriggerService _debouncingTriggerService = debouncingTriggerService ?? throw new ArgumentNullException(nameof(debouncingTriggerService));
    private readonly SensitiveDataProvider _sensitiveDataProvider = sensitiveDataProvider ?? throw new ArgumentNullException(nameof(sensitiveDataProvider));
    private readonly ClipboardWrapper _clipboard = clipboard ?? throw new ArgumentNullException(nameof(clipboard));

    public Task Handle(InitDebouncingEventModel notification, CancellationToken _)
    {
        var currentDispatcher = Dispatcher.CurrentDispatcher;

        _debouncingTriggerService.Register(DebouncingTriggers.ClearClipboardPassword, _ =>
        {
            currentDispatcher.Invoke(() =>
            {
                var currentText = _clipboard.GetText();
                var password = _sensitiveDataProvider.GetPassword() ?? string.Empty;

                if (!string.IsNullOrEmpty(currentText) && string.Equals(password, currentText))
                {
                    _clipboard.ClearText();
                }
            });

            return Task.CompletedTask;
        });

        return Task.CompletedTask;
    }
}