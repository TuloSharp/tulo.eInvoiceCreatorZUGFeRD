using System.Windows.Input;

namespace tulo.CommonMVVM.Commands;

public abstract class BaseCommand : ICommand
{
    public event EventHandler CanExecuteChanged;

    public virtual bool CanExecute(object parameter) => true;

    public abstract void Execute(object parameter);

    protected void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, new EventArgs());
    }
}
