using System;
using System.Windows;
using System.Windows.Input;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace NZ.RdpMaid.App.UiServices
{
    internal class PublishCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        private IPublisher? _publisher;

        public bool CanExecute(object? parameter)
        {
            return parameter is not null;
        }

        public void Execute(object? parameter)
        {
            if (_publisher is null)
            {
                var serviceProvider = ((App)Application.Current).ServiceProvider
                    ?? throw new InvalidOperationException("Application service provider not initialized");

                _publisher = serviceProvider.GetRequiredService<IPublisher>();
            }

            if (parameter is not null)
            {
                _publisher.Publish(parameter);
            }
        }
    }
}