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
        AddLog("Подготовка к обновлению");
        AddLog("Проверка окружения");

        var env = UpdateEnvironmentReader.Read();

        if (string.IsNullOrEmpty(env.InstallDirPath))
        {
            AddErrorLog("Не установлен путь к каталогу установки");
            return;
        }

        AddLog($"Проверка каталога установки: {env.InstallDirPath}");

        if (!Directory.Exists(env.InstallDirPath))
        {
            AddErrorLog("Каталог установки не найден");
            return;
        }

        if (string.IsNullOrEmpty(env.UpdateFilePath))
        {
            AddErrorLog("Не установлен путь к файлу обновления");
            return;
        }

        AddLog($"Проверка файла обновления: {env.UpdateFilePath}");

        if (!File.Exists(env.UpdateFilePath))
        {
            AddErrorLog("Файл обновления не найден");
            return;
        }

        AddLog($"Проверка каталога данных пользователя: {env.UserDataDirPath}");

        if (!string.IsNullOrEmpty(env.UserDataDirPath) && !Directory.Exists(env.UserDataDirPath))
        {
            AddErrorLog($"Каталог данных пользователя не найден");
            return;
        }

        // Подготовка к обновлению - закрытие основного приложения
        //

        Progress = 0.1F;
        DetailedText = "Закрытие основного приложения";
        AddLog("Закрытие основного приложения");
        AddLog("Поиск процесса NZ.RdpMaid.App");

        var processes = Process.GetProcessesByName("NZ.RdpMaid.App");

        foreach (var process in processes)
        {
            AddLog($"Завершение процесса NZ.RdpMaid.App PID={process.Id}");
            process.Kill(true);
        }

        int delayMs = 1_000;

        while (!processes.All(p => p.HasExited))
        {
            AddLog("Ожидание завершения процессов основного приложения");
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
        AddLog("Распаковка файла обновления");

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
        AddLog("Резервное копирование текущей версии");

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
        AddLog("Резервное копирование пользовательских данных");

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
        AddLog("Установка");
        AddLog("Копирование файлов");

        await CopyDirectoryRecursive(
            source: new DirectoryInfo(tempUpdateContentPath),
            target: new DirectoryInfo(env.InstallDirPath),
            ct: ct);

        // Запуск основного приложения
        //

        Progress = 0.8F;
        StatusText = "Запуск основного приложения";
        DetailedText = "Запуск основного приложения";
        AddLog("Запуск основного приложения");

        var mainExeFilePath = Path.Combine(env.InstallDirPath, "net8.0-windows7.0", "NZ.RdpMaid.App.exe");

        var mainExeProcess = Process.Start(mainExeFilePath);

        await Task.Delay(500, ct);

        Progress = 0.9F;
        StatusText = "Обновление прошло успешно";
        AddLog("Обновление прошло успешно");

        if (!mainExeProcess.HasExited)
        {
            AddLog("Основное приложение успешно запущено");
        }

        Progress = 1.0F;
        AddLog("Выключение...");

        System.Windows.Application.Current.Shutdown();
    }

    private void AddLog(string message)
    {
        Logs.Add(new LogEntry(DateTime.Now.TimeOfDay, message));
    }

    private void AddErrorLog(string message)
    {
        Logs.Add(new LogEntry(DateTime.Now.TimeOfDay, $"[ОШИБКА] {message}"));
    }

    private async Task CopyDirectoryRecursive(DirectoryInfo source, DirectoryInfo target, CancellationToken ct)
    {
        AddLog($"Копирование {source.FullName}");

        foreach (var file in source.GetFiles())
        {
            AddLog($"Копирование {file.FullName}");
            await Task.Run(() => file.CopyTo(Path.Combine(target.FullName, file.Name), true), ct);
        }

        foreach (var dir in target.GetDirectories())
        {
            var targetDir = new DirectoryInfo(Path.Combine(source.FullName, dir.Name));

            if (!targetDir.Exists)
            {
                targetDir.Create();
            }

            await CopyDirectoryRecursive(dir, targetDir, ct);
        }
    }
}