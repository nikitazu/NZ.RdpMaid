using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NZ.RdpMaid.App.Core.Services;

namespace NZ.RdpMaid.App.EventModel
{
    internal record SelfTestEventModel : INotification
    {
        public static readonly SelfTestEventModel Instance = new();
    }

    internal class SelfTestEventConsumer(
        MainWindowViewModel mainWindow,
        SessionProvider sessionProvider,
        SensitiveDataProvider sensitiveDataProvider
        ) : INotificationHandler<SelfTestEventModel>
    {
        private readonly MainWindowViewModel _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
        private readonly SessionProvider _sessionProvider = sessionProvider ?? throw new ArgumentNullException(nameof(sessionProvider));
        private readonly SensitiveDataProvider _sensitiveDataProvider = sensitiveDataProvider ?? throw new ArgumentNullException(nameof(sensitiveDataProvider));

        public Task Handle(SelfTestEventModel notification, CancellationToken cancellationToken)
        {
            (string Error, string Hint)? result = _sessionProvider.TestCanCreateSession();

            if (result == null)
            {
                result = TrySafeStorageMigration();
            }

            _mainWindow.LoadStatus =
                result is null
                    ? MainWindowViewModel.LoadStatusKind.Done
                    : MainWindowViewModel.LoadStatusKind.Error;

            if (result is not null)
            {
                _mainWindow.LoadError = result.Value.Error;
                _mainWindow.LoadErrorHint = result.Value.Hint;
            }

            return Task.CompletedTask;
        }

        private (string Error, string Hint)? TrySafeStorageMigration()
        {
            try
            {
                _sensitiveDataProvider.TryMigrateToSafeStorageIfNeeded();
                return null;
            }
            catch (Exception ex)
            {
                return (Error: ex.Message, Hint: ex.ToString());
            }
        }
    }
}