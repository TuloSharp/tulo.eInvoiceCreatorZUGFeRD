using tulo.CommonMVVM.ViewModels;
using tulo.eInvoice.eInvoiceApp.Utilities;

namespace tulo.eInvoice.eInvoiceApp.ViewModels.Factories
{
    public interface INavigatorViewModelFactory
    {
        /// <summary>
        /// a selected view model is created
        /// </summary>
        /// <param name="viewTypes">the view model type is in an enum defined</param>
        /// <returns>the created view model</returns>
        BaseViewModel CreateViewModel(NavTypes viewTypes);
    }
}
