using System.Windows;
using System.Windows.Input;

namespace NZ.RdpMaid.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Delete:
                case Key.Back:
                case Key.Left:
                case Key.Right:
                case Key.Home:
                case Key.End:
                case Key.Tab:
                case Key.D0:
                case Key.D1:
                case Key.D2:
                case Key.D3:
                case Key.D4:
                case Key.D5:
                case Key.D6:
                case Key.D7:
                case Key.D8:
                case Key.D9:
                case Key.NumPad0:
                case Key.NumPad1:
                case Key.NumPad2:
                case Key.NumPad3:
                case Key.NumPad4:
                case Key.NumPad5:
                case Key.NumPad6:
                case Key.NumPad7:
                case Key.NumPad8:
                case Key.NumPad9:
                    break;
                case Key.Enter:
                    e.Handled = true;
                    ConnectButton.Focus();
                    break;
                default:
                    e.Handled = true;
                    break;
            }
        }
    }
}
