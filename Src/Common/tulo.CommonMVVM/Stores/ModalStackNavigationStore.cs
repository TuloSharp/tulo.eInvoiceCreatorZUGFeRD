using System.Collections.ObjectModel;
using tulo.CommonMVVM.ViewModels;

namespace tulo.CommonMVVM.Stores;

/// <summary>
/// A navigation store that manages a stack of modal view models.
/// - Always shows the top view model as <see cref="CurrentViewModel"/>.
/// - Supports multiple modals in a stack (like overlay dialogs).
/// - Closing removes only the top modal, revealing the previous one.
/// </summary>
public class ModalStackNavigationStore : IModalStackNavigationStore
{
    private readonly Stack<BaseViewModel> _modalStack = new();

    /// <summary>
    /// Event fired whenever the current view model changes
    /// (push, pop, or close all).
    /// </summary>
    public event Action CurrentViewModelChanged;

    /// <summary>
    /// Gets the view model at the top of the stack,
    /// or <c>null</c> if the stack is empty.
    /// </summary>
    public BaseViewModel CurrentViewModel => _modalStack.Count > 0 ? _modalStack.Peek() : null;

    /// <summary>
    /// True if there is at least one modal on the stack.
    /// </summary>
    public bool IsModalOpen => _modalStack.Count > 0;

    /// <summary>
    /// Gets the collection of all modal view models currently in the stack.
    /// </summary>
    public ObservableCollection<BaseViewModel> Modals { get; } = new();

    /// <summary>
    /// Opens a new modal by pushing it onto the stack.
    /// The previous modals remain in memory and keep their state.
    /// </summary>
    public void Open(BaseViewModel viewModel)
    {
        _modalStack.Push(viewModel);
        Modals.Add(viewModel);
        OnCurrentViewModelChanged();
    }

    /// <summary>
    /// Closes the top modal:
    /// - Disposes the top view model.
    /// - Reveals the previous one if available.
    /// </summary>
    public void Close(BaseViewModel viewModel)
    {
        if (viewModel == null || _modalStack.Count == 0)
            return;

        if (ReferenceEquals(_modalStack.Peek(), viewModel))
        {
            var top = _modalStack.Pop();
            Modals.Remove(top);
            top.Dispose();
            OnCurrentViewModelChanged();
            return;
        }

        var buffer = new Stack<BaseViewModel>();
        while (_modalStack.Count > 0)
        {
            var item = _modalStack.Pop();
            if (ReferenceEquals(item, viewModel))
            {
                Modals.Remove(item);
                item.Dispose();
                break;
            }
            buffer.Push(item);
        }

        while (buffer.Count > 0)
            _modalStack.Push(buffer.Pop());

        OnCurrentViewModelChanged();
    }

    private void OnCurrentViewModelChanged() => CurrentViewModelChanged?.Invoke();
}
