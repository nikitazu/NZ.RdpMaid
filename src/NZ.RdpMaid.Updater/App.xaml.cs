#define UPDATER_ENV_TEST_MODE__OFF

using System.Windows;

namespace NZ.RdpMaid.Updater;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private void Application_Startup(object sender, StartupEventArgs e)
    {
#if UPDATER_ENV_TEST_MODE__ON
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        Services.UpdateEnvironmentReader.Write(new Models.UpdateEnvironment(
            InstallDirPath: System.IO.Path.Combine("C:", "opt", "apps", "NZ.RdpMaid"),
            UpdateFilePath: System.IO.Path.Combine(appDataPath, "NZ.RdpMaid", "update.zip"),
            UserDataDirPath: System.IO.Path.Combine(appDataPath, "NZ.RdpMaid")
        ));
#endif

        Current.MainWindow = new MainWindow
        {
            DataContext = new MainWindowViewModel(),
        };

        Current.MainWindow.Show();
    }
}