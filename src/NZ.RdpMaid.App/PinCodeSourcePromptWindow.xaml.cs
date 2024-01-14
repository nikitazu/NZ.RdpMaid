using System.Windows;

namespace NZ.RdpMaid.App
{
    /// <summary>
    /// Interaction logic for PinCodeSourcePromptWindow.xaml
    /// </summary>
    public partial class PinCodeSourcePromptWindow : Window
    {
        private PinCodeSourcePromptWindowViewModel Model => (PinCodeSourcePromptWindowViewModel)DataContext;

        public PinCodeSourcePromptWindow()
        {
            InitializeComponent();
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
