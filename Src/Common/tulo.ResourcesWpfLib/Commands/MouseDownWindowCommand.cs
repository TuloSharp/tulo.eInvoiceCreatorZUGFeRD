using System;
using System.Windows;
using System.Windows.Input;
using tulo.ResourcesWpfLib.Viewmodels;

namespace tulo.ResourcesWpfLib.Commands;

public class MouseDownWindowCommand(IResizeWindowViewModel resizeWindowViewModel) : BaseCommand
{
    private readonly IResizeWindowViewModel _resizeWindowViewModel = resizeWindowViewModel;
    private Window _window;
    private bool _isDragging;
    private bool _isFirstClick = true;

    public override void Execute(object parameter)
    {
        _window = parameter as Window;

        if (_window is not null)
        {
            _window.MouseDown -= Window_MouseDown;
            _window.MouseDown += Window_MouseDown;
            _window.LocationChanged -= Window_LocationChanged;
            _window.LocationChanged += Window_LocationChanged;
            // Unsubscription is required as dispose only hits whenever the window is closed

            if (Mouse.LeftButton == MouseButtonState.Pressed && _isFirstClick)
            {
                _isDragging = true;
                _window?.DragMove();
                _isDragging = false;
                _isFirstClick = false;
            }
        }
    }

    private void Window_LocationChanged(object sender, EventArgs e)
    {
        if (_isDragging)
        {
            var screenTop = SystemParameters.WorkArea.Top;
            var windowTop = _window.Top;

            if (windowTop <= screenTop)
            {
                _resizeWindowViewModel.IsWindowMaximized = true;
                _resizeWindowViewModel.IsWindowCustomResized = false;
            }
            else
            {
                _resizeWindowViewModel.IsWindowMaximized = false;
                _resizeWindowViewModel.IsWindowCustomResized = true;
            }
        }
    }

    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            _isDragging = true;
            _window?.DragMove();
            _isDragging = false;
        }
    }

    public void Dispose()
    {
        if (_window is not null)
        {
            _window.MouseDown -= Window_MouseDown;
            _window.LocationChanged -= Window_LocationChanged;
            _window = null;
        }
    }
}