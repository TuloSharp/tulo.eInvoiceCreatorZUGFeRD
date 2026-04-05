using Microsoft.Extensions.Logging;
using tulo.CommonMVVM.Collector;
using tulo.CommonMVVM.Commands;
using tulo.CommonMVVM.Stores;
using tulo.eInvoiceApp.Utilities;
using tulo.eInvoiceApp.ViewModels.Factories;

namespace tulo.eInvoiceApp.Commands.Common;

public class UpdateCurrentViewModelCommand : BaseCommand
{
    private readonly INavigatorViewModelFactory _navigatorListViewModelFactory;

    #region Services / Stores filled via CollectorCollection
    private readonly INavigationStore _navigationStore;
    private readonly ILogger<UpdateCurrentViewModelCommand> _logger;
    #endregion

    public UpdateCurrentViewModelCommand(INavigatorViewModelFactory navigatorListViewModelFactory, ICollectorCollection collectorCollection)
    {
        _navigatorListViewModelFactory = navigatorListViewModelFactory;

        #region Get Services / Stores from CollectorCollection
        _navigationStore = collectorCollection.GetService<INavigationStore>();
        _logger = collectorCollection.GetService<ILoggerFactory>().CreateLogger<UpdateCurrentViewModelCommand>();
        #endregion
    }

    public override void Execute(object parameter)
    {
        if (parameter is NavTypes)
        {
            NavTypes viewType = (NavTypes)parameter;
            _navigationStore.CurrentViewModel = _navigatorListViewModelFactory.CreateViewModel(viewType);

            switch (viewType)
            {
                case NavTypes.InvoiceView:
                    {
                        _logger.LogInformation($"switch to '{nameof(NavTypes.InvoiceView)}'");
                        return;
                    }
                case NavTypes.SellerView:
                    {
                        _logger.LogInformation($"switch to '{nameof(NavTypes.SellerView)}'");
                        return;
                    }
                case NavTypes.AboutView:
                    {
                        _logger.LogInformation($"switch to '{nameof(NavTypes.AboutView)}'");
                        return;
                    }
                    throw new ArgumentException("the ViewType doesn't have a ViewModel" + nameof(viewType) + "viewType");
            }
        }
    }
}
