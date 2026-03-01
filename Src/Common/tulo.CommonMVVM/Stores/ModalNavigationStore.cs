using tulo.CommonMVVM.ViewModels;

namespace tulo.CommonMVVM.Stores;

public class ModalNavigationStore : IModalNavigationStore
{
    public event Action CurrentViewModelChanged;

    private BaseViewModel _currentViewModel;
    public BaseViewModel CurrentViewModel
    {
        get => _currentViewModel;
        set
        {
            _currentViewModel?.Dispose();
            _currentViewModel = value;
            OnCurrentViewModelChanged();
        }
    }

    public bool IsModalOpen => _currentViewModel != null;

    public void Close()
    {
        CurrentViewModel = null;
    }

    private void OnCurrentViewModelChanged()
    {
        CurrentViewModelChanged?.Invoke();
    }
}
