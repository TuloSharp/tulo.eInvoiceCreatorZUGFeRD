using tulo.CommonMVVM.ViewModels;

namespace tulo.CommonMVVM.Stores;

public interface IModalStackNavigationStore
{
    /// <summary>
    /// Raised whenever the current modal view model changes
    /// (e.g., after Open, Close, or CloseAll).
    /// </summary>
    event Action CurrentViewModelChanged;

    /// <summary>
    /// The current (topmost) modal view model,
    /// or <c>null</c> if the stack is empty.
    /// </summary>
    BaseViewModel CurrentViewModel { get; }

    /// <summary>
    /// True if there is at least one modal in the stack.
    /// </summary>
    bool IsModalOpen { get; }

    /// <summary>
    /// Gets the collection of all modal view models currently in the stack.
    /// </summary>
    System.Collections.ObjectModel.ObservableCollection<BaseViewModel> Modals { get; }

    /// <summary>
    /// Opens a new modal by pushing it onto the top of the stack.
    /// </summary>
    void Open(BaseViewModel viewModel);

    /// <summary>
    /// Closes the topmost modal and returns it.
    /// Returns <c>null</c> if the stack is empty.
    /// </summary>
    void Close(BaseViewModel viewModel);
}
