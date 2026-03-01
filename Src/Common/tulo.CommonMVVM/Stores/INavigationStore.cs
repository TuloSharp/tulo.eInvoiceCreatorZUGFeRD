using tulo.CommonMVVM.ViewModels;

namespace tulo.CommonMVVM.Stores;

public interface INavigationStore
{
    event Action CurrentViewModelChanged;
    BaseViewModel CurrentViewModel { get; set; }
}
