using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using MediatR;
using NZ.RdpMaid.App.EventModel;

namespace NZ.RdpMaid.App
{
    internal class MainWindowViewModel : INotifyPropertyChanged
    {
        public enum LoadStatusKind
        {
            Loading,
            Error,
            Done,
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private static class PropArgs
        {
            public static readonly PropertyChangedEventArgs AppTitle = new(nameof(AppTitle));
            public static readonly PropertyChangedEventArgs WelcomeText = new(nameof(WelcomeText));
            public static readonly PropertyChangedEventArgs PinCodeLifetimeText = new(nameof(PinCodeLifetimeText));
            public static readonly PropertyChangedEventArgs PinCode = new(nameof(PinCode));
            public static readonly PropertyChangedEventArgs ContentVisibility = new(nameof(ContentVisibility));
            public static readonly PropertyChangedEventArgs LoadingVisibility = new(nameof(LoadingVisibility));
            public static readonly PropertyChangedEventArgs LoadErrorVisibility = new(nameof(LoadErrorVisibility));
            public static readonly PropertyChangedEventArgs LoadError = new(nameof(LoadError));
            public static readonly PropertyChangedEventArgs LoadErrorHint = new(nameof(LoadErrorHint));
        }

        // Dependencies
        //

        private readonly IPublisher _pub;

        // Fields
        //

        private string _appTitle = "NZ.RdpMaid.App (cute)";
        private string _welcomeText = string.Empty;
        private string _lifetimeText = string.Empty;
        private string _pinCode = string.Empty;
        private LoadStatusKind _loadStatus = LoadStatusKind.Loading;
        private string _loadError = string.Empty;
        private string _loadErrorHint = string.Empty;
        private Timer? _timer;

        // Properties
        //

        public string AppTitle
        {
            get => _appTitle;
            set
            {
                _appTitle = value;
                PropertyChanged?.Invoke(this, PropArgs.AppTitle);
            }
        }

        public string WelcomeText
        {
            get => _welcomeText;
            set
            {
                _welcomeText = value;
                PropertyChanged?.Invoke(this, PropArgs.WelcomeText);
            }
        }

        public string PinCodeLifetimeText
        {
            get => _lifetimeText;
            set
            {
                _lifetimeText = value;
                PropertyChanged?.Invoke(this, PropArgs.PinCodeLifetimeText);
            }
        }

        public string PinCode
        {
            get => _pinCode;
            set
            {
                _pinCode = value;
                PropertyChanged?.Invoke(this, PropArgs.PinCode);
            }
        }

        public LoadStatusKind LoadStatus
        {
            get => _loadStatus;
            set
            {
                _loadStatus = value;
                PropertyChanged?.Invoke(this, PropArgs.ContentVisibility);
                PropertyChanged?.Invoke(this, PropArgs.LoadingVisibility);
                PropertyChanged?.Invoke(this, PropArgs.LoadErrorVisibility);
            }
        }

        public string LoadError
        {
            get => _loadError;
            set
            {
                _loadError = value;
                PropertyChanged?.Invoke(this, PropArgs.LoadError);
            }
        }

        public string LoadErrorHint
        {
            get => _loadErrorHint;
            set
            {
                _loadErrorHint = value;
                PropertyChanged?.Invoke(this, PropArgs.LoadErrorHint);
            }
        }

        public Visibility ContentVisibility =>
            LoadStatus == LoadStatusKind.Done ? Visibility.Visible : Visibility.Collapsed;

        public Visibility LoadingVisibility =>
            LoadStatus == LoadStatusKind.Loading ? Visibility.Visible : Visibility.Collapsed;

        public Visibility LoadErrorVisibility =>
            LoadStatus == LoadStatusKind.Error ? Visibility.Visible : Visibility.Collapsed;

        // Init
        //

        public MainWindowViewModel(IPublisher pub)
        {
            _pub = pub ?? throw new ArgumentNullException(nameof(pub));
        }

        public void OnLoadFinished()
        {
            _pub.Publish(SelfTestEventModel.Instance);
            _pub.Publish(InitPinCodeProviderEventModel.Instance);
            _pub.Publish(InitDebouncingEventModel.Instance);

            var version = typeof(MainWindowViewModel).Assembly.GetName().Version;
            AppTitle = $"NZ.RdpMaid.App v{version}";

            // TODO test if generator is available
            _timer = new(OnTimerTick, this, 0, 1000);
        }

        private void OnTimerTick(object? sender)
        {
            _pub.Publish(GeneratePinCodeEventModel.Instance);
        }
    }
}