using System.Windows;

namespace NZ.RdpMaid.Updater;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel model && model is not null)
        {
            await model.StartUpdate();
        }
    }
}