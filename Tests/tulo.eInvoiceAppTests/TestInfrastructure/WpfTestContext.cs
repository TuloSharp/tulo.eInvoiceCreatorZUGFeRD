using System.Windows.Threading;

namespace tulo.eInvoiceCreatorZUGFeRDTests.TestInfrastructure;

public sealed class WpfTestContext : IDisposable
{
    private readonly Thread _thread;
    private Dispatcher _dispatcher = null!;
    private readonly ManualResetEventSlim _ready = new();

    public WpfTestContext()
    {
        _thread = new Thread(() =>
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            _ready.Set();
            Dispatcher.Run(); // starts the real WPF message pump
        });

        _thread.SetApartmentState(ApartmentState.STA);
        _thread.IsBackground = true;
        _thread.Start();
        _ready.Wait(); // wait until dispatcher is ready
    }

    public void Invoke(Action action) => _dispatcher.Invoke(action);

    public T Invoke<T>(Func<T> func) => _dispatcher.Invoke(func);

    // Pumps the dispatcher until all pending async void continuations are done
    public void WaitForIdle() => _dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);

    public void Dispose()
    {
        _dispatcher.InvokeShutdown();
        _thread.Join();
    }
}
