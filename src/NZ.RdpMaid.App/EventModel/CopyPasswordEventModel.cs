using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using MediatR;
using NZ.RdpMaid.App.Core.Services;

namespace NZ.RdpMaid.App.EventModel
{
    internal record CopyPasswordEventModel : INotification;

    internal class CopyPasswordEventConsumer(SensitiveDataProvider provider) : INotificationHandler<CopyPasswordEventModel>
    {
        private readonly SensitiveDataProvider _provider = provider ?? throw new ArgumentNullException(nameof(provider));

        public Task Handle(CopyPasswordEventModel notification, CancellationToken cancellationToken)
        {
            var password = _provider.GetPassword() ?? string.Empty;
            Clipboard.SetText(password);

            return Task.CompletedTask;
        }
    }
}