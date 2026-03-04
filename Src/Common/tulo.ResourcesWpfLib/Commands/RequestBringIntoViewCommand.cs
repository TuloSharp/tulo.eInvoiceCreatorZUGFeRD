using System.Windows;

namespace tulo.ResourcesWpfLib.Commands;

public class RequestBringIntoViewCommand : BaseCommand
{
    public override void Execute(object parameter)
    {
        // Try to cast the command parameter to RequestBringIntoViewEventArgs.
        // This event args type is used by WPF when it wants to automatically
        // scroll an element into the visible area (for example inside a ScrollViewer,
        // ListBox, DataGrid, etc.).
        var requestBringIntoViewEventArgs = parameter as RequestBringIntoViewEventArgs;

        if (requestBringIntoViewEventArgs is not null)
        {
            // Mark the event as handled so WPF stops further processing.
            // Effect: prevents the default "bring this element into view" behavior,
            // which usually means WPF will NOT auto-scroll to make the element visible.
            requestBringIntoViewEventArgs.Handled = true;
        }
    }
}
