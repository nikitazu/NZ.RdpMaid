using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NZ.RdpMaid.App.Core.Services;

namespace NZ.RdpMaid.App.EventModel
{
    internal record ImportPinCodeSourceEventModel(string ImportText) : INotification;

    internal class ImportPinCodeSourceEventConsumer(
        PinCodeSourcePromptWindowViewModel model,
        PinCodeSourceImporter importer
        ) : INotificationHandler<ImportPinCodeSourceEventModel>
    {
        private readonly PinCodeSourcePromptWindowViewModel _model = model ?? throw new ArgumentNullException(nameof(model));
        private readonly PinCodeSourceImporter _importer = importer ?? throw new ArgumentNullException(nameof(importer));

        public Task Handle(ImportPinCodeSourceEventModel notification, CancellationToken cancellationToken)
        {
            var importUri = _model.ImportText;
            var result = _importer.Import(importUri);

            _model.ImportFeedback = result.Feedback;

            if (result.SecretCode is not null)
            {
                _model.Source = result.SecretCode;
            }

            return Task.CompletedTask;
        }
    }
}