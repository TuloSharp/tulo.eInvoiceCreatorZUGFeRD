using System.Windows;
using tulo.CommonMVVM.Commands;

namespace tulo.ResourcesWpfLib.Commands;

public class RequestBringIntoViewCommand : BaseCommand
{
    public override void Execute(object parameter)
    {
        RequestBringIntoViewEventArgs requestBringIntoViewEventArgs = parameter as RequestBringIntoViewEventArgs;

        if (requestBringIntoViewEventArgs is not null)
        {
            requestBringIntoViewEventArgs.Handled = true;
        }
    }
}
