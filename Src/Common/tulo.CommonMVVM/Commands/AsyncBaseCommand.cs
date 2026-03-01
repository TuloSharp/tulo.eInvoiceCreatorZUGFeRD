using System.Windows.Input;

namespace tulo.CommonMVVM.Commands;

public abstract class AsyncBaseCommand : ICommand
{
    public readonly Action<Exception> OnException;

    private bool _isExecuting;
    public bool IsExecuting
    {
        get => _isExecuting;
        private set
        {
            _isExecuting = value;
            CanExecuteChanged?.Invoke(this,new EventArgs());
        }
    }

    public AsyncBaseCommand(Action<Exception> onException = null)
    {
        OnException = onException;
    }

    public event EventHandler CanExecuteChanged;

    public virtual bool CanExecute(object parameter) => !IsExecuting;


    public async void Execute(object parameter)
    {
        IsExecuting = true;
        try { await ExecuteAsync(parameter); }
        catch (Exception ex) { OnException?.Invoke(ex); }
        finally { IsExecuting = false; }
    }

    protected abstract Task ExecuteAsync(object parameter);

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
