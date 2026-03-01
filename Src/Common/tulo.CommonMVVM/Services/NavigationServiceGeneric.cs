using tulo.CommonMVVM.Stores;
using tulo.CommonMVVM.ViewModels;

namespace tulo.CommonMVVM.Services
{
    public class NavigationServiceGeneric<TViewModel> : INavigationServiceGeneric<TViewModel> where TViewModel : BaseViewModel
    {
        private readonly NavigationStore _navigationStore;
        private readonly CreateViewModel<TViewModel> _createViewModel;

        public NavigationServiceGeneric(NavigationStore navigationStore, CreateViewModel<TViewModel> createViewModel)
        {
            _navigationStore = navigationStore;
            _createViewModel = createViewModel;
        }

        public void Navigate()
        {
            _navigationStore.CurrentViewModel = _createViewModel();
        }
    }
}
