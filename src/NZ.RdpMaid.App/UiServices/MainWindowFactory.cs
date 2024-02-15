using System.Windows;

namespace NZ.RdpMaid.App.UiServices
{
    internal class MainWindowFactory(MainWindowViewModel viewModel)
    {
        public void CreateMainWindow()
        {
            var window = new MainWindow
            {
                DataContext = viewModel,
            };

            Application.Current.MainWindow = window;

            window.Show();
            viewModel.OnLoadFinished();
        }
    }
}