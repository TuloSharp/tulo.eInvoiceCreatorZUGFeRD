using System.Windows;
using tulo.CommonMVVM.Commands;

namespace tulo.ResourcesWpfLib.Commands
{
    public class MinimizedWindowCommand : BaseCommand
    {
        public override void Execute(object parameter)
        {
            var window = parameter as Window;

            if (window is not null)
            {
                window.WindowState = WindowState.Minimized;
            }
        }
    }
}
