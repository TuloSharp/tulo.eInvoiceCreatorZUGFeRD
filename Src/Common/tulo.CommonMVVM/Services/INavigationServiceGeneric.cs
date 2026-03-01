using tulo.CommonMVVM.ViewModels;

namespace tulo.CommonMVVM.Services
{
    public interface INavigationServiceGeneric<TViewModel> where TViewModel : BaseViewModel
    {
        void Navigate();
    }
}