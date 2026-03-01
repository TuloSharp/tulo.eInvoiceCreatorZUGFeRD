using Microsoft.Extensions.Logging;
using tulo.CommonMVVM.Collector;
using tulo.CommonMVVM.Commands;
using tulo.eInvoice.eInvoiceApp.Services;
using tulo.eInvoice.eInvoiceApp.Stores.Invoices;
using tulo.eInvoice.eInvoiceApp.ViewModels.Invoices;

namespace tulo.eInvoice.eInvoiceApp.Commands.Invoices;

public class LoadInvoicePositionsCommand(InvoiceViewModel invoiceViewModel, ICollectorCollection collectorCollection) : AsyncBaseCommand
{
    private readonly InvoiceViewModel _invoiceViewModel = invoiceViewModel;
    private readonly IInvoicePositionService _invoiceService = collectorCollection.GetService<IInvoicePositionService>();
    private readonly ILogger<LoadInvoicePositionsCommand> _logger = collectorCollection.GetService<ILoggerFactory>().CreateLogger<LoadInvoicePositionsCommand>();
    private readonly ISelectedInvoicePositionStore _selectedInvoicePositionStore = collectorCollection.GetService<ISelectedInvoicePositionStore>();

    protected override async Task ExecuteAsync(object parameter)
    {
        try
        {
            await _invoiceService.LoadAllInvoicePositionsAsync();
        }
        catch (Exception ex)
        {
            _invoiceViewModel.StatusMessage = $"Technical error: please contact the service: {ex.Message}";
        }
        finally
        {
            // remember selected item at vm and return it when renavigate to parent
            if (_invoiceViewModel.InvoicePositionCardListItemCollectionView is not null)
            {
                InvoicePositionCardItemViewModel? toSelect = null;

                // 1) Restore from selected-store
                var storedKey = _selectedInvoicePositionStore?.SelectedInvoicePosition?.InvoicePositionNr;
                if (storedKey is not null)
                {
                    toSelect = _invoiceViewModel.InvoicePositionCardListItemCollectionView
                        .Cast<InvoicePositionCardItemViewModel>()
                        .FirstOrDefault(vm => vm.InvoicePositionNr == storedKey);
                }

                // 2) Fallback: first item
                if (toSelect is null)
                {
                    var enumerator = _invoiceViewModel.InvoicePositionCardListItemCollectionView.GetEnumerator();
                    if (enumerator.MoveNext())
                        toSelect = (InvoicePositionCardItemViewModel)enumerator.Current;
                }

                if (toSelect is not null)
                    _invoiceViewModel.SelectedInvoicePositionCardListItemViewModel = toSelect;
            }

            _logger.LogInformation($"Command was Called: {nameof(LoadInvoicePositionsCommand)}");
        }
    }
}
