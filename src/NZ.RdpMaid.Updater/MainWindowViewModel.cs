using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using NZ.RdpMaid.Updater.Commands;
using NZ.RdpMaid.Updater.Models;

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
        await PerformUpdateCommand.Run(this, ct);
    }

    public async Task ResetFileLog(CancellationToken ct)
    {
        await File.WriteAllTextAsync("update.log", string.Empty, ct);
    }

    public async Task AddLog(string message, CancellationToken ct)
    {
        var entry = new LogEntry(DateTime.Now.TimeOfDay, message);
        Logs.Add(entry);
        await File.AppendAllTextAsync("update.log", $"[{entry.Time}] {entry.Message}\n", ct);
    }

    public async Task AddErrorLog(string message, CancellationToken ct)
    {
        await AddLog($"[ОШИБКА] {message}", ct);
    }

    public async Task CopyDirectoryRecursive(DirectoryInfo source, DirectoryInfo target, CancellationToken ct)
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