using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using NZ.RdpMaid.App.Core.Services;

namespace NZ.RdpMaid.App.UiServices
{
    internal class OpenLinkInBrowserCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        private ShellProvider? _shell;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            if (_shell is null)
            {
                var serviceProvider = ((App)Application.Current).ServiceProvider
                    ?? throw new InvalidOperationException("Application service provider not initialized");

                _shell = serviceProvider.GetRequiredService<ShellProvider>();
            }

            if (parameter is string url)
            {
                _shell.OpenUrl(url);
            }
        }
    }
}