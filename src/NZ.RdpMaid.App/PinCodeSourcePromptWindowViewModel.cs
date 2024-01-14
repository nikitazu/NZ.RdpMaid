using System;
using System.ComponentModel;
using MediatR;
using NZ.RdpMaid.App.EventModel;

namespace NZ.RdpMaid.App
{
    internal class PinCodeSourcePromptWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private static class PropArgs
        {
            public static readonly PropertyChangedEventArgs ImportText = new(nameof(ImportText));
            public static readonly PropertyChangedEventArgs ImportFeedback = new(nameof(ImportFeedback));
            public static readonly PropertyChangedEventArgs Source = new(nameof(Source));
        }

        // Dependencies
        //

        private readonly IPublisher _pub;

        // Fields
        //

        private string _importText = string.Empty;
        private string _importFeedback = string.Empty;
        private string _source = string.Empty;

        // Properties
        //

        public string ImportText
        {
            get => _importText;
            set
            {
                _importText = value;
                PropertyChanged?.Invoke(this, PropArgs.ImportText);
                _pub.Publish(new ImportPinCodeSourceEventModel(ImportText: value));
            }
        }

        public string ImportFeedback
        {
            get => _importFeedback;
            set
            {
                _importFeedback = value;
                PropertyChanged?.Invoke(this, PropArgs.ImportFeedback);
            }
        }

        public string Source
        {
            get => _source;
            set
            {
                _source = value;
                PropertyChanged?.Invoke(this, PropArgs.Source);
            }
        }

        public PinCodeSourcePromptWindowViewModel(IPublisher pub)
        {
            _pub = pub ?? throw new ArgumentNullException(nameof(pub));
        }
    }
}