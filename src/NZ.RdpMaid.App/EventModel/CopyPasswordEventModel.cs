using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NZ.RdpMaid.App.Core.Constants;
using NZ.RdpMaid.App.Core.Services;

namespace NZ.RdpMaid.App.EventModel
{
    internal record CopyPasswordEventModel : INotification;

    internal class CopyPasswordEventConsumer(
        DebouncingTriggerService debouncingTriggerService,
        SensitiveDataProvider provider,
        ClipboardWrapper clipboard
    )
        : INotificationHandler<CopyPasswordEventModel>
    {
        private readonly DebouncingTriggerService _debouncingTriggerService = debouncingTriggerService ?? throw new ArgumentNullException(nameof(debouncingTriggerService));
        private readonly SensitiveDataProvider _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        private readonly ClipboardWrapper _clipboard = clipboard ?? throw new ArgumentNullException(nameof(clipboard));

        public Task Handle(CopyPasswordEventModel notification, CancellationToken cancellationToken)
        {
            var password = _provider.GetPassword() ?? string.Empty;
            _clipboard.SetText(password);
            _debouncingTriggerService.Trigger(DebouncingTriggers.ClearClipboardPassword);

            return Task.CompletedTask;
        }
    }
}