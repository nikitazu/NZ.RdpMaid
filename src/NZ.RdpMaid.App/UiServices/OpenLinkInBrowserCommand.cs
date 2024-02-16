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

        private BrowserProvider? _browserProvider;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            if (_browserProvider is null)
            {
                var serviceProvider = ((App)Application.Current).ServiceProvider
                    ?? throw new InvalidOperationException("Application service provider not initialized");

                _browserProvider = serviceProvider.GetRequiredService<BrowserProvider>();
            }

            if (parameter is string url)
            {
                _browserProvider.OpenUrl(url);
            }
        }
    }
}