using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using NZ.RdpMaid.App.UiServices.Contracts;

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

        public void ShowDialogView<TView, TViewModel>(string? title = null)
            where TView : UserControl, new()
            where TViewModel : notnull
        {
            var owner = Application.Current.MainWindow;
            var viewModel = serviceProvider.GetRequiredService<TViewModel>();

            var dialog = new Window
            {
                Owner = owner,
                Width = owner.Width,
                Height = owner.Height,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                // Background = Brushes.Transparent,
                // AllowsTransparency = true,
                // WindowStyle = WindowStyle.None,
                DataContext = viewModel,
                Content = new TView(),
                Title = title,
            };

            dialog.Activated += Dialog_Activated;
            dialog.ShowDialog();
            dialog.Activated -= Dialog_Activated;
        }

        private void Dialog_Activated(object? sender, EventArgs e)
        {
            if (sender is Window window && window.DataContext is ILoadableViewModel loadable)
            {
                loadable.OnLoaded();
            }
        }
    }
}