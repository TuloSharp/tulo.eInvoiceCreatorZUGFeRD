using tulo.CommonMVVM.Stores;
using tulo.CommonMVVM.ViewModels;

namespace tulo.CommonMVVM.Services
{
    public class RenavigationService<TViewModel> : IRenavigationService<TViewModel> where TViewModel : BaseViewModel
    {
        private readonly INavigationStore _navigationStore;
        private readonly CreateViewModel<TViewModel> _createViewModel;

        public RenavigationService(INavigationStore navigationStore, CreateViewModel<TViewModel> createViewModel)
        {
            _navigationStore = navigationStore;
            _createViewModel = createViewModel;
        }
        public Type ViewModelType => typeof(TViewModel);

        public void Renavigate()
        {
            _navigationStore.CurrentViewModel = _createViewModel();
        }
    }
}