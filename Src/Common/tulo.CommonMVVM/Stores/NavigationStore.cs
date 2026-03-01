using tulo.CommonMVVM.ViewModels;

namespace tulo.CommonMVVM.Stores;

public class NavigationStore : INavigationStore
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

    private void OnCurrentViewModelChanged()
    {
        CurrentViewModelChanged?.Invoke();
    }
}
