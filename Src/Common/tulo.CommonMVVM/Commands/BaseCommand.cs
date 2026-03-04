using System.Windows.Input;

namespace tulo.CommonMVVM.Commands;

public abstract class BaseCommand : ICommand
{
    // Raised when CanExecute() may have changed.
    // WPF command sources (Button, MenuItem, etc.) listen to this event.
    public event EventHandler CanExecuteChanged;

    // Default: command is always executable.
    // Override in derived classes to provide enable/disable logic.
    public virtual bool CanExecute(object parameter) => true;

    // Derived classes must implement the execution logic.
    public abstract void Execute(object parameter);

    // Call this when something changes that affects CanExecute().
    protected void RaiseCanExecuteChanged()
    {
        // EventArgs.Empty avoids allocating a new EventArgs object.
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
