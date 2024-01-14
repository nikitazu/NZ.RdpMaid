using System.Windows;

namespace NZ.RdpMaid.App
{
    /// <summary>
    /// Interaction logic for PasswordPromptWindow.xaml
    /// </summary>
    public partial class PasswordPromptWindow : Window
    {
        private PasswordPromptWindowViewModel Model => (PasswordPromptWindowViewModel)DataContext;

        public PasswordPromptWindow()
        {
            InitializeComponent();
        }

        private void PasswordTextBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            Model.Password = PasswordTextBox.Password;
            Model.PasswordRepeat = PasswordRepeatTextBox.Password;

            var isValid = Model.IsValid();
            SaveButton.IsEnabled = isValid;
        }

        private void PasswordRepeatTextBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            Model.Password = PasswordTextBox.Password;
            Model.PasswordRepeat = PasswordRepeatTextBox.Password;

            var isValid = Model.IsValid();
            SaveButton.IsEnabled = isValid;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
