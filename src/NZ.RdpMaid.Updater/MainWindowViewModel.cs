using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using NZ.RdpMaid.Updater.Models;
using NZ.RdpMaid.Updater.Services;

namespace NZ.RdpMaid.Updater;

internal class MainWindowViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private static class Args
    {
        public static readonly PropertyChangedEventArgs StatusText = new(nameof(StatusText));
        public static readonly PropertyChangedEventArgs DetailedText = new(nameof(DetailedText));
        public static readonly PropertyChangedEventArgs Logs = new(nameof(Logs));
        public static readonly PropertyChangedEventArgs Progress = new(nameof(Progress));
    }

    private string _statusText = "Запуск обновления";
    private string _detailedText = string.Empty;
    private ObservableCollection<LogEntry> _logs = [];
    private float _progress = 0.0F;

    public string StatusText
    {
        get => _statusText;
        set
        {
            _statusText = value;
            PropertyChanged?.Invoke(this, Args.StatusText);
        }
    }

    public string DetailedText
    {
        get => _detailedText;
        set
        {
            _detailedText = value;
            PropertyChanged?.Invoke(this, Args.DetailedText);
        }
    }

    public ObservableCollection<LogEntry> Logs
    {
        get => _logs;
        set
        {
            _logs = value;
            PropertyChanged?.Invoke(this, Args.Logs);
        }
    }

    public float Progress
    {
        get => _progress;
        set
        {
            _progress = value;
            PropertyChanged?.Invoke(this, Args.Progress);
        }
    }

    public async Task StartUpdate(CancellationToken ct = default)
    {
        // Подготовка к обновлению - проверка окружения
        //

        Progress = 0.0F;
        StatusText = "Подготовка к обновлению";
        DetailedText = "Проверка окружения";
        await ResetFileLog(ct);
        await AddLog("Подготовка к обновлению", ct);
        await AddLog("Проверка окружения", ct);

        var env = UpdateEnvironmentReader.Read();

        if (string.IsNullOrEmpty(env.InstallDirPath))
        {
            await AddErrorLog("Не установлен путь к каталогу установки", ct);
            return;
        }

        await AddLog($"Проверка каталога установки: {env.InstallDirPath}", ct);

        if (!Directory.Exists(env.InstallDirPath))
        {
            await AddErrorLog("Каталог установки не найден", ct);
            return;
        }

        if (string.IsNullOrEmpty(env.UpdateFilePath))
        {
            await AddErrorLog("Не установлен путь к файлу обновления", ct);
            return;
        }

        await AddLog($"Проверка файла обновления: {env.UpdateFilePath}", ct);

        if (!File.Exists(env.UpdateFilePath))
        {
            await AddErrorLog("Файл обновления не найден", ct);
            return;
        }

        await AddLog($"Проверка каталога данных пользователя: {env.UserDataDirPath}", ct);

        if (!string.IsNullOrEmpty(env.UserDataDirPath) && !Directory.Exists(env.UserDataDirPath))
        {
            await AddErrorLog($"Каталог данных пользователя не найден", ct);
            return;
        }

        // Подготовка к обновлению - закрытие основного приложения
        //

        Progress = 0.1F;
        DetailedText = "Закрытие основного приложения";
        await AddLog("Закрытие основного приложения", ct);
        await AddLog("Поиск процесса NZ.RdpMaid.App", ct);

        var processes = Process.GetProcessesByName("NZ.RdpMaid.App");

        foreach (var process in processes)
        {
            await AddLog($"Завершение процесса NZ.RdpMaid.App PID={process.Id}", ct);

            try
            {
                process.Kill(entireProcessTree: false);
            }
            catch (Exception ex)
            {
                await AddErrorLog($"Не удалось завершить процесс основного приложения PID={process.Id}", ct);
                await AddErrorLog($"{ex.Message}", ct);
            }
        }

        int delayMs = 1_000;

        while (!processes.All(p => p.HasExited))
        {
            await AddLog("Ожидание завершения процессов основного приложения", ct);
            await Task.Delay(delayMs, ct);

            if (delayMs < 16_000)
            {
                delayMs *= 2;
            }
        }

        // Подготовка к обновлению - распаковка файла обновления
        //

        Progress = 0.2F;
        DetailedText = "Распаковка файла обновления";
        await AddLog("Распаковка файла обновления", ct);

        var tempPath = Path.GetTempPath();
        var tempUpdateBasePath = Path.Combine(tempPath, "NZ.RdpMaid.Update");
        var tempUpdateContentPath = Path.Combine(tempUpdateBasePath, "content");
        var tempUpdateBaseDir = new DirectoryInfo(tempUpdateBasePath);

        if (tempUpdateBaseDir.Exists)
        {
            tempUpdateBaseDir.Delete(true);
        }

        tempUpdateBaseDir.Create();

        System.IO.Compression.ZipFile.ExtractToDirectory(env.UpdateFilePath, tempUpdateContentPath, false);

        // Подготовка к обновлению - резервное копирование текущей версии
        //

        Progress = 0.3F;
        DetailedText = "Резервное копирование текущей версии";
        await AddLog("Резервное копирование текущей версии", ct);

        var currentDirPath = Path.GetDirectoryName(typeof(MainWindowViewModel).Assembly.Location)!;
        var currentDir = new DirectoryInfo(currentDirPath);
        var appBackupFilePath = Path.Combine(tempUpdateBasePath, "NZ.RdpMaid.App.Backup.zip");

        if (File.Exists(appBackupFilePath))
        {
            File.Delete(appBackupFilePath);
        }

        System.IO.Compression.ZipFile.CreateFromDirectory(currentDir.FullName, appBackupFilePath);

        // Подготовка к обновлению -  резервное копирование пользовательских данных
        //

        Progress = 0.35F;
        DetailedText = "Резервное копирование пользовательских данных";
        await AddLog("Резервное копирование пользовательских данных", ct);

        var userDataBackupFilePath = Path.Combine(tempUpdateBasePath, "NZ.RdpMaid.UserData.Backup.zip");

        if (File.Exists(userDataBackupFilePath))
        {
            File.Delete(userDataBackupFilePath);
        }

        using (var archive = System.IO.Compression.ZipFile.Open(userDataBackupFilePath, System.IO.Compression.ZipArchiveMode.Create))
        {
            foreach (var file in new DirectoryInfo(env.UserDataDirPath).GetFiles())
            {
                if (file.Name != "update.zip")
                {
                    var entry = archive.CreateEntry(file.Name);
                    using var entryStream = entry.Open();
                    using var fileStream = file.OpenRead();
                    await fileStream.CopyToAsync(entryStream, ct);
                }
            }
        }

        // Установка
        //

        Progress = 0.4F;
        StatusText = "Установка";
        DetailedText = "Копирование файлов";
        await AddLog("Установка", ct);
        await AddLog("Копирование файлов", ct);

        await CopyDirectoryRecursive(
            source: new DirectoryInfo(tempUpdateContentPath),
            target: new DirectoryInfo(env.InstallDirPath),
            ct: ct);

        // Запуск основного приложения
        //

        Progress = 0.8F;
        StatusText = "Запуск основного приложения";
        DetailedText = "Запуск основного приложения";
        await AddLog("Запуск основного приложения", ct);

        var mainExeFilePath = Path.Combine(env.InstallDirPath, "net8.0-windows7.0", "NZ.RdpMaid.App.exe");

        var mainExeProcess = Process.Start(mainExeFilePath);

        await Task.Delay(500, ct);

        Progress = 0.9F;
        StatusText = "Обновление прошло успешно";
        await AddLog("Обновление прошло успешно", ct);

        if (!mainExeProcess.HasExited)
        {
            await AddLog("Основное приложение успешно запущено", ct);
        }

        Progress = 1.0F;
        await AddLog("Выключение...", ct);

        System.Windows.Application.Current.Shutdown();
    }

    private async Task ResetFileLog(CancellationToken ct)
    {
        await File.WriteAllTextAsync("update.log", string.Empty, ct);
    }

    private async Task AddLog(string message, CancellationToken ct)
    {
        var entry = new LogEntry(DateTime.Now.TimeOfDay, message);
        Logs.Add(entry);
        await File.AppendAllTextAsync("update.log", $"[{entry.Time}] {entry.Message}\n", ct);
    }

    private async Task AddErrorLog(string message, CancellationToken ct)
    {
        await AddLog($"[ОШИБКА] {message}", ct);
    }

    private async Task CopyDirectoryRecursive(DirectoryInfo source, DirectoryInfo target, CancellationToken ct)
    {
        await AddLog($"Копирование {source.FullName} -> {target.FullName}", ct);

        foreach (var sourceFile in source.GetFiles())
        {
            var targetFileName = Path.Combine(target.FullName, sourceFile.Name);
            await AddLog($"Копирование {sourceFile.FullName} -> {targetFileName}", ct);
            await Task.Run(() => sourceFile.CopyTo(targetFileName, true), ct);
        }

        foreach (var sourceSubDir in source.GetDirectories())
        {
            var targetSubDir = new DirectoryInfo(Path.Combine(target.FullName, sourceSubDir.Name));

            if (!targetSubDir.Exists)
            {
                targetSubDir.Create();
            }

            await CopyDirectoryRecursive(sourceSubDir, targetSubDir, ct);
        }
    }
}