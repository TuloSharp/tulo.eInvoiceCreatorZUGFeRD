using tulo.CommonMVVM.Services;
using tulo.CommonMVVM.ViewModels;

namespace tulo.CommonMVVM.Commands;

public class NavigateCommandGeneric<TViewModel> : BaseCommand where TViewModel : BaseViewModel
{
    private readonly INavigationServiceGeneric<TViewModel> _navigationService;

    public NavigateCommandGeneric(INavigationServiceGeneric<TViewModel> navigationService)
    {
        _navigationService = navigationService;
    }
    public override void Execute(object parameter)
    {
        _navigationService.Navigate();
    }
}
