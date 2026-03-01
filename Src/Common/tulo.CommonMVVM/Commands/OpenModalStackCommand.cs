using tulo.CommonMVVM.Collector;
using tulo.CommonMVVM.Stores;
using tulo.CommonMVVM.ViewModels;

namespace tulo.CommonMVVM.Commands;

public class OpenModalStackCommand(ICollectorCollection collectorCollection, Func<BaseViewModel> factory, Type viewModelType) : BaseCommand
{
    private readonly IModalStackNavigationStore _modalStackNavigationStore = collectorCollection.GetService<IModalStackNavigationStore>();
    private readonly Func<BaseViewModel> _factory = factory;
    private readonly Type _viewModelType = viewModelType;

    public override void Execute(object parameter)
    {
        string viewModelName = _viewModelType.Name;

        if (_modalStackNavigationStore.CurrentViewModel?.GetType() == _viewModelType)
            return;

        if (_factory == null)
            return;

        var baseViewModel = _factory();
        if (baseViewModel == null)
            return;

        _modalStackNavigationStore.Open(baseViewModel);
    }
}