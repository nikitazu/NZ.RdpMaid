using System.ComponentModel;

namespace NZ.RdpMaid.App
{
    internal class PasswordPromptWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private static class PropArgs
        {
            public static readonly PropertyChangedEventArgs Password = new(nameof(Password));
            public static readonly PropertyChangedEventArgs PasswordRepeat = new(nameof(PasswordRepeat));
        }

        // Fields
        //

        private string _password = string.Empty;
        private string _passwordRepeat = string.Empty;

        // Properties
        //

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                PropertyChanged?.Invoke(this, PropArgs.Password);
            }
        }

        public string PasswordRepeat
        {
            get => _passwordRepeat;
            set
            {
                _passwordRepeat = value;
                PropertyChanged?.Invoke(this, PropArgs.PasswordRepeat);
            }
        }

        public bool IsValid()
        {
            return Password == PasswordRepeat;
        }
    }
}
