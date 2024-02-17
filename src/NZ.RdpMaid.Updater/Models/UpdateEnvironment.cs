namespace NZ.RdpMaid.Updater.Models;

internal record UpdateEnvironment(
    string InstallDirPath,
    string UpdateFilePath,
    string UserDataDirPath
);