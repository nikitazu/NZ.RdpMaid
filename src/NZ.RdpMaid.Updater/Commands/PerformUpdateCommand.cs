using System.Diagnostics;
using System.IO;
using NZ.RdpMaid.Updater.Services;

namespace NZ.RdpMaid.Updater.Commands;

internal static class PerformUpdateCommand
{
    private const int _milliSecondsToWaitForProcessToDieMin = 1_000;
    private const int _milliSecondsToWaitForProcessToDieMax = 8_000;
    private const int _milliSecondsToWaitForMainAppToStart = 1_000;

    public static async Task Run(MainWindowViewModel model, CancellationToken ct)
    {
        // Подготовка к обновлению - проверка окружения
        //

        model.Progress = 0.0F;
        model.StatusText = "Подготовка к обновлению";
        model.DetailedText = "Проверка окружения";
        await model.ResetFileLog(ct);
        await model.AddLog("Подготовка к обновлению", ct);
        await model.AddLog("Проверка окружения", ct);

        var env = UpdateEnvironmentReader.Read();

        if (string.IsNullOrEmpty(env.InstallDirPath))
        {
            await model.AddErrorLog("Не установлен путь к каталогу установки", ct);
            return;
        }

        await model.AddLog($"Проверка каталога установки: {env.InstallDirPath}", ct);

        if (!Directory.Exists(env.InstallDirPath))
        {
            await model.AddErrorLog("Каталог установки не найден", ct);
            return;
        }

        if (string.IsNullOrEmpty(env.UpdateFilePath))
        {
            await model.AddErrorLog("Не установлен путь к файлу обновления", ct);
            return;
        }

        await model.AddLog($"Проверка файла обновления: {env.UpdateFilePath}", ct);

        if (!File.Exists(env.UpdateFilePath))
        {
            await model.AddErrorLog("Файл обновления не найден", ct);
            return;
        }

        await model.AddLog($"Проверка каталога данных пользователя: {env.UserDataDirPath}", ct);

        if (!string.IsNullOrEmpty(env.UserDataDirPath) && !Directory.Exists(env.UserDataDirPath))
        {
            await model.AddErrorLog($"Каталог данных пользователя не найден", ct);
            return;
        }

        // Подготовка к обновлению - закрытие основного приложения
        //

        model.Progress = 0.1F;
        model.DetailedText = "Закрытие основного приложения";
        await model.AddLog("Закрытие основного приложения", ct);
        await model.AddLog("Поиск процесса NZ.RdpMaid.App", ct);

        var processes = Process.GetProcessesByName("NZ.RdpMaid.App");

        foreach (var process in processes)
        {
            await model.AddLog($"Завершение процесса NZ.RdpMaid.App PID={process.Id}", ct);

            try
            {
                process.Kill(entireProcessTree: false);
            }
            catch (Exception ex)
            {
                await model.AddErrorLog($"Не удалось завершить процесс основного приложения PID={process.Id}", ct);
                await model.AddErrorLog($"{ex.Message}", ct);
            }
        }

        int delayMs = _milliSecondsToWaitForProcessToDieMin;

        while (!processes.All(p => p.HasExited))
        {
            await model.AddLog($"Ожидание завершения процессов основного приложения ({delayMs}ms)", ct);
            await Task.Delay(delayMs, ct);

            if (delayMs < _milliSecondsToWaitForProcessToDieMax)
            {
                delayMs *= 2;
            }
        }

        // Подготовка к обновлению - распаковка файла обновления
        //

        model.Progress = 0.2F;
        model.DetailedText = "Распаковка файла обновления";
        await model.AddLog("Распаковка файла обновления", ct);

        var tempPath = Path.GetTempPath();
        var tempUpdateBasePath = Path.Combine(tempPath, "NZ.RdpMaid.Update");
        var tempUpdateContentPath = Path.Combine(tempUpdateBasePath, "content");
        var appBackupFilePath = Path.Combine(tempUpdateBasePath, "NZ.RdpMaid.App.Backup.zip");
        var userDataBackupFilePath = Path.Combine(tempUpdateBasePath, "NZ.RdpMaid.UserData.Backup.zip");

        PackageManager.ExtractPackage(zipPath: env.UpdateFilePath, outputDirPath: tempUpdateContentPath);

        // Подготовка к обновлению - резервное копирование текущей версии
        //

        model.Progress = 0.3F;
        model.DetailedText = "Резервное копирование текущей версии";
        await model.AddLog("Резервное копирование текущей версии", ct);

        PackageManager.BackupApplicationInstallationDirectory(outputFilePath: appBackupFilePath);

        // Подготовка к обновлению -  резервное копирование пользовательских данных
        //

        model.Progress = 0.35F;
        model.DetailedText = "Резервное копирование пользовательских данных";
        await model.AddLog("Резервное копирование пользовательских данных", ct);
        await PackageManager.BackupUserDataDirectory(env.UserDataDirPath, outputFilePath: userDataBackupFilePath, ct);

        // Установка
        //

        model.Progress = 0.4F;
        model.StatusText = "Установка";
        model.DetailedText = "Копирование файлов";
        await model.AddLog("Установка", ct);
        await model.AddLog("Копирование файлов", ct);

        await model.CopyDirectoryRecursive(
            source: new DirectoryInfo(tempUpdateContentPath),
            target: new DirectoryInfo(env.InstallDirPath),
            ct: ct);

        // Запуск основного приложения
        //

        model.Progress = 0.8F;
        model.StatusText = "Запуск основного приложения";
        model.DetailedText = "Запуск основного приложения";
        await model.AddLog("Запуск основного приложения", ct);

        var mainExeFilePath = Path.Combine(env.InstallDirPath, "net8.0-windows7.0", "NZ.RdpMaid.App.exe");

        var mainExeProcess = Process.Start(mainExeFilePath);

        await Task.Delay(_milliSecondsToWaitForMainAppToStart, ct);

        model.Progress = 0.9F;
        model.StatusText = "Обновление прошло успешно";
        await model.AddLog("Обновление прошло успешно", ct);

        if (!mainExeProcess.HasExited)
        {
            await model.AddLog("Основное приложение успешно запущено", ct);
        }

        model.Progress = 1.0F;
        await model.AddLog("Выключение...", ct);

        System.Windows.Application.Current.Shutdown();
    }
}