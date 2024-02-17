using NZ.RdpMaid.Updater.Models;

namespace NZ.RdpMaid.Updater.Services;

internal static class UpdateEnvironmentReader
{
    private static class Keys
    {
        public const string InstallDirPath = "NZ_RDPMAID__INSTALL_DIR_PATH";
        public const string UpdateFilePath = "NZ_RDPMAID__UPDATE_FILE_PATH";
        public const string UserDataDirPath = "NZ_RDPMAID__USER_DATA_DIR_PATH";
    }

    public static void Write(UpdateEnvironment env)
    {
        ArgumentNullException.ThrowIfNull(env, nameof(env));

        SetVar(Keys.InstallDirPath, env.InstallDirPath);
        SetVar(Keys.UpdateFilePath, env.UpdateFilePath);
        SetVar(Keys.UserDataDirPath, env.UserDataDirPath);
    }

    public static UpdateEnvironment Read()
    {
        var installDirPath = GetVar(Keys.InstallDirPath);
        var updateFilePath = GetVar(Keys.UpdateFilePath);
        var userDataDirPath = GetVar(Keys.UserDataDirPath);

        return new UpdateEnvironment(
            InstallDirPath: installDirPath,
            UpdateFilePath: updateFilePath,
            UserDataDirPath: userDataDirPath
        );
    }

    private static string GetVar(string key)
    {
        var value = Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Process);
        return value ?? string.Empty;
    }

    private static void SetVar(string key, string value)
    {
        Environment.SetEnvironmentVariable(key, value, EnvironmentVariableTarget.Process);
    }
}