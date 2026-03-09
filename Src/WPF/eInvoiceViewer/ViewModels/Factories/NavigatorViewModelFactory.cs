using Microsoft.Extensions.Logging;
using tulo.CommonMVVM.Collector;
using tulo.CommonMVVM.ViewModels;
using tulo.eInvoice.eInvoiceViewer.Utilities;

namespace tulo.eInvoice.eInvoiceViewer.ViewModels.Factories
{
    public class NavigatorViewModelFactory(CreateViewModel<ContentXmlToPdfViewerViewModel> contentXmlToPdfViewerViewModel,
                                           CreateViewModel<AboutViewModel> createAboutViewModel,
                                           ICollectorCollection collectorCollection) : INavigatorViewModelFactory
    {
        private readonly CreateViewModel<ContentXmlToPdfViewerViewModel> _contentXmlToPdfViewerViewModel = contentXmlToPdfViewerViewModel;
        private readonly CreateViewModel<AboutViewModel> _createAboutViewModel = createAboutViewModel;
        private readonly ILogger<NavigatorViewModelFactory> _logger = collectorCollection.GetService<ILoggerFactory>().CreateLogger<NavigatorViewModelFactory>();

        public BaseViewModel CreateViewModel(NavTypes viewTypes)
        {
            switch (viewTypes)
            {
                case NavTypes.ContentXmlToPdfViewerView:
                    _logger.LogInformation($"{nameof(CreateViewModel)}: {nameof(NavTypes.ContentXmlToPdfViewerView)}");
                    return _contentXmlToPdfViewerViewModel();

                case NavTypes.AboutView:
                    _logger.LogInformation($"{nameof(CreateViewModel)}: {nameof(NavTypes.AboutView)}");
                    return _createAboutViewModel();


                default:
                    var tempMessage = $"the ViewType doesn't have a ViewModel" + nameof(viewTypes) + "viewType";
                    _logger.LogError(tempMessage);
                    throw new ArgumentException(tempMessage);
            }
        }
    }
}
