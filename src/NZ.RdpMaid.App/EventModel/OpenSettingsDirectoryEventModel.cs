using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NZ.RdpMaid.App.Core.Services;

namespace NZ.RdpMaid.App.EventModel
{
    internal record OpenSettingsDirectoryEventModel : INotification;

    internal class OpenSettingsDirectoryEventConsumer(FileStorage fileStorage) : INotificationHandler<OpenSettingsDirectoryEventModel>
    {
        public Task Handle(OpenSettingsDirectoryEventModel notification, CancellationToken cancellationToken)
        {
            var settingDir = fileStorage.EnsureDataDirectoryExists();

            if (settingDir.Exists)
            {
                Process.Start("explorer.exe", settingDir.FullName);
            }

            return Task.CompletedTask;
        }
    }
}