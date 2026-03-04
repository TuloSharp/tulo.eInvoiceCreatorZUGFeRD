using System.Windows;
using tulo.ResourcesWpfLib.Viewmodels;

namespace tulo.ResourcesWpfLib.Commands;

public class ResizeWindowCommand(IResizeWindowViewModel resizeWindowViewModel) : BaseCommand
{
    private readonly IResizeWindowViewModel _resizeWindowViewModel = resizeWindowViewModel;

    public override void Execute(object parameter)
    {
        var window = parameter as Window;

        if (window is not null)
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
    }
}
