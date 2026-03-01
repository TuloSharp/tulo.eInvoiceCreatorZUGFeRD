using tulo.CommonMVVM.Stores;
using tulo.CommonMVVM.ViewModels;

namespace tulo.CommonMVVM.Services
{
    public class ModalNavigationService<TViewModel> : INavigationService where TViewModel : BaseViewModel
    {
        private readonly ModalNavigationStore _modalNavigationStore;
        private readonly CreateViewModel<TViewModel> _createViewModel;

        public ModalNavigationService(ModalNavigationStore modalNavigationStore, CreateViewModel<TViewModel> createViewModel)
        {
            _modalNavigationStore = modalNavigationStore;
            _createViewModel = createViewModel;
        }

        public void Navigate()
        {
            _modalNavigationStore.CurrentViewModel = _createViewModel();
        }
    }
}
