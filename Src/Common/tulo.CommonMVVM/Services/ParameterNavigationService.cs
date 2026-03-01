using tulo.CommonMVVM.Stores;
using tulo.CommonMVVM.ViewModels;

namespace tulo.CommonMVVM.Services
{
    public class ParameterNavigationService<TParameter, TViewModel> where TViewModel : BaseViewModel
    {
        public readonly NavigationStore _navigationStore;
        public readonly CreateViewModel<TParameter, TViewModel> _createViewModel;

        public ParameterNavigationService(NavigationStore navigationStore, CreateViewModel<TParameter, TViewModel> createViewModel)
        {
            _navigationStore = navigationStore;
            _createViewModel = createViewModel;
        }

        public void Navigate(TParameter parameter)
        {
            _navigationStore.CurrentViewModel = _createViewModel(parameter);
        }
    }
}
