using System;
using System.Windows;
using System.Windows.Input;

namespace NZ.RdpMaid.App.UiServices
{
    internal class ToggleContextMenuCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            if (parameter is FrameworkElement fe && fe.ContextMenu is not null)
            {
                fe.ContextMenu.IsOpen = true;
            }
        }
    }
}