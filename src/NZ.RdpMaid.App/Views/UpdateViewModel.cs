﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using MediatR;
using NZ.RdpMaid.App.Core.Services;
using NZ.RdpMaid.App.Core.Services.HttpClients;
using NZ.RdpMaid.App.EventModel.Updates;
using NZ.RdpMaid.App.UiServices.Contracts;

namespace NZ.RdpMaid.App.Views
{
    internal class UpdateViewModel(IPublisher pub, AppVersionProvider appVersion)
        : INotifyPropertyChanged, ILoadableViewModel
    {
        public enum Status
        {
            None,
            CheckingForUpdate,
            WaitingForDownloadOrder,
            Downloading,
            WaitingForInstallOrder,
            Installing,
            Updated,
        }

        public record LogEntry(TimeSpan Time, string Message);

        public event PropertyChangedEventHandler? PropertyChanged;

        private static class PropArgs
        {
            public static readonly PropertyChangedEventArgs Logs = new(nameof(Logs));
            public static readonly PropertyChangedEventArgs CurrentStatus = new(nameof(CurrentStatus));
            public static readonly PropertyChangedEventArgs CurrentStatusText = new(nameof(CurrentStatusText));
            public static readonly PropertyChangedEventArgs PendingUpdate = new(nameof(PendingUpdate));
            public static readonly PropertyChangedEventArgs DownloadSectionVisibility = new(nameof(DownloadSectionVisibility));
            public static readonly PropertyChangedEventArgs IsDownloadButtonEnabled = new(nameof(IsDownloadButtonEnabled));
            public static readonly PropertyChangedEventArgs DownloadProgressValue = new(nameof(DownloadProgressValue));
            public static readonly PropertyChangedEventArgs InstallSectionVisibility = new(nameof(InstallSectionVisibility));
            public static readonly PropertyChangedEventArgs IsInstallButtonEnabled = new(nameof(IsInstallButtonEnabled));
        }

        // Зависимости
        //

        private readonly IPublisher _pub = pub ?? throw new ArgumentNullException(nameof(pub));

        // Поля
        //

        private ObservableCollection<LogEntry> _logs = [];
        private Status _currentStatus = Status.None;
        private UpdateModel? _pendingUpdate = null;
        private float _downloadProgressValue;

        // Свойства
        //

        public Version CurrentVersion { get; } = appVersion.Current;

        public ObservableCollection<LogEntry> Logs
        {
            get => _logs;
            set
            {
                _logs = value;
                PropertyChanged?.Invoke(this, PropArgs.Logs);
            }
        }

        public Status CurrentStatus
        {
            get => _currentStatus;
            set
            {
                _currentStatus = value;
                PropertyChanged?.Invoke(this, PropArgs.CurrentStatus);
                PropertyChanged?.Invoke(this, PropArgs.CurrentStatusText);
                PropertyChanged?.Invoke(this, PropArgs.DownloadSectionVisibility);
                PropertyChanged?.Invoke(this, PropArgs.IsDownloadButtonEnabled);
                PropertyChanged?.Invoke(this, PropArgs.InstallSectionVisibility);
                PropertyChanged?.Invoke(this, PropArgs.IsInstallButtonEnabled);
            }
        }

        public string CurrentStatusText => CurrentStatus switch
        {
            Status.None => "-",
            Status.CheckingForUpdate => "Проверяю...",
            Status.WaitingForDownloadOrder => "Есть обнова, её можно скачать!",
            Status.Downloading => "Качаю...",
            Status.WaitingForInstallOrder => "Обновление готово к установке!",
            Status.Installing => "Устанавливаю...",
            Status.Updated => "Текущая версия актуальна.",
            _ => "Неизвестное состояние " + CurrentStatus.ToString() ?? string.Empty,
        };

        public Visibility DownloadSectionVisibility =>
            CurrentStatus == Status.WaitingForDownloadOrder
            || CurrentStatus == Status.Downloading
            ? Visibility.Visible
            : Visibility.Collapsed;

        public bool IsDownloadButtonEnabled =>
            CurrentStatus == Status.WaitingForDownloadOrder;

        public Visibility InstallSectionVisibility =>
            CurrentStatus == Status.WaitingForInstallOrder
            || CurrentStatus == Status.Installing
            ? Visibility.Visible
            : Visibility.Collapsed;

        public bool IsInstallButtonEnabled =>
            CurrentStatus == Status.WaitingForInstallOrder;

        public UpdateModel? PendingUpdate
        {
            get => _pendingUpdate;
            set
            {
                _pendingUpdate = value;
                PropertyChanged?.Invoke(this, PropArgs.PendingUpdate);
            }
        }

        public float DownloadProgressValue
        {
            get => _downloadProgressValue;
            set
            {
                _downloadProgressValue = value;
                PropertyChanged?.Invoke(this, PropArgs.DownloadProgressValue);
            }
        }

        public void OnLoaded()
        {
            if (CurrentStatus == Status.None)
            {
                _pub.Publish(LoadUpdateInfoEventModel.Instance);
            }
        }

        public void AddLog(string message)
        {
            Logs.Add(new LogEntry(DateTimeOffset.Now.TimeOfDay, message));
        }

        public void AddLogIfNotEmpty(string? message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                AddLog(message);
            }
        }
    }
}