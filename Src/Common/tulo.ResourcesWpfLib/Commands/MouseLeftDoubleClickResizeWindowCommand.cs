using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using tulo.CommonMVVM.Commands;
using tulo.ResourcesWpfLib.Viewmodels;

namespace tulo.ResourcesWpfLib.Commands;

public class MouseLeftDoubleClickResizeWindowCommand(IResizeWindowViewModel resizeWindowViewModel) : BaseCommand
{
    private readonly IResizeWindowViewModel _resizeWindowViewModel = resizeWindowViewModel;
    private Window _window;
    private DispatcherTimer _doubleClickTimer;
    private const int DoubleClickDelay = 300; // Delay in milliseconds to handle double-clicks
    private bool _isHandlingDoubleClick;
    private bool _isClickOnTitleBar;

    public override void Execute(object parameter)
    {
        _window = parameter as Window;

        if (_window is not null)
        {
            _window.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
            _window.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
            _window.MouseDoubleClick -= Window_MouseDoubleClick;
            _window.MouseDoubleClick += Window_MouseDoubleClick;
        }
    }

    private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is UIElement element && element.IsMouseOver)
        {
            Point clickPosition = e.GetPosition(_window);

            _isClickOnTitleBar = _window.InputHitTest(clickPosition) is UIElement clickedElement && IsClickOnTitleBar(clickedElement);
        }
        else
        {
            _isClickOnTitleBar = false;
        }
    }

    private static bool IsClickOnTitleBar(UIElement clickedElement)
    {
        FrameworkElement element = clickedElement as FrameworkElement;
        while (element != null)
        {
            if (element.Name == "TitleBarGrid")
            {
                return true;
            }
            element = VisualTreeHelper.GetParent(element) as FrameworkElement;
        }
        return false;
    }

    private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (_isClickOnTitleBar && !_isHandlingDoubleClick && e.LeftButton == MouseButtonState.Pressed)
        {
            _isHandlingDoubleClick = true;

            if (_doubleClickTimer == null)
            {
                _doubleClickTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(DoubleClickDelay)
                };
                _doubleClickTimer.Tick += (s, args) =>
                {
                    _doubleClickTimer.Stop();
                    _doubleClickTimer = null;

                    if (sender is Window window)
                    {
                        ResizeMainWindow(window);
                    }

                    _isHandlingDoubleClick = false;
                    _isClickOnTitleBar = false; // Reset flag
                };
                _doubleClickTimer.Start();
            }
        }
    }

    private void ResizeMainWindow(Window window)
    {
        if (window.WindowState == WindowState.Maximized)
        {
            _resizeWindowViewModel.IsWindowMaximized = false;
            _resizeWindowViewModel.IsWindowCustomResized = true;
            window.WindowState = WindowState.Normal;
        }
        else
        {
            _resizeWindowViewModel.IsWindowMaximized = true;
            _resizeWindowViewModel.IsWindowCustomResized = false;
            window.WindowState = WindowState.Maximized;
        }
    }

    public void Dispose()
    {
        if (_window is not null)
        {
            _window.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
            _window.MouseDoubleClick -= Window_MouseDoubleClick;
            _window = null;
        }
        if (_doubleClickTimer != null)
        {
            _doubleClickTimer.Stop();
            _doubleClickTimer = null;
        }
    }
}
