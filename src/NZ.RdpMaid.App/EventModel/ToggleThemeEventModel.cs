using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NZ.RdpMaid.App.UiServices;

namespace NZ.RdpMaid.App.EventModel
{
    internal record ToggleThemeEventModel : INotification;

    internal class ToggleThemeEventConsumer(ThemeLoader themeLoader) : INotificationHandler<ToggleThemeEventModel>
    {
        private readonly ThemeLoader _themeLoader = themeLoader ?? throw new ArgumentNullException(nameof(themeLoader));

        public Task Handle(ToggleThemeEventModel notification, CancellationToken cancellationToken)
        {
            _themeLoader.ToggleTheme();

            return Task.CompletedTask;
        }
    }
}