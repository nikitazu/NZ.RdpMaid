using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace NZ.RdpMaid.App.UiServices
{
    internal class DialogProvider(IServiceProvider serviceProvider)
    {
        public void ShowDialog<TWindow, TViewModel>()
            where TWindow : Window, new()
        {
            var dialog = new TWindow
            {
                Owner = Application.Current.MainWindow,
                DataContext = serviceProvider.GetRequiredService(typeof(TViewModel)),
            };

            dialog.ShowDialog();
        }
    }
}