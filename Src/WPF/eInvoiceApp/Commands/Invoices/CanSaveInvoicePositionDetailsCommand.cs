using Microsoft.Extensions.Logging;
using System.Windows.Input;
using tulo.CommonMVVM.Collector;
using tulo.CommonMVVM.Commands;
using tulo.eInvoice.eInvoiceApp.ViewModels.Invoices;

namespace tulo.eInvoice.eInvoiceApp.Commands.Invoices;
public class CanSaveInvoicePositionDetailsCommand(InvoicePositionDetailsFormViewModel invoicePositionDetailsFormViewModel, ICommand submitCommand, ICollectorCollection collectorCollection) : BaseCommand
{
    private readonly InvoicePositionDetailsFormViewModel _invoicePositionDetailsFormViewModel = invoicePositionDetailsFormViewModel;
    private readonly ICommand _submitCommand = submitCommand;

    #region Get filled via CollectorCollection
    private readonly ILogger<CanSaveInvoicePositionDetailsCommand> _logger = collectorCollection.GetService<ILoggerFactory>().CreateLogger<CanSaveInvoicePositionDetailsCommand>();
    #endregion

    public override void Execute(object parameter)
    {
        _logger.LogInformation($"{nameof(CanSaveInvoicePositionDetailsCommand)} is called");

        if (_invoicePositionDetailsFormViewModel.IsEnableToSaveData)
            _submitCommand.Execute(null);
        else
            _invoicePositionDetailsFormViewModel.IsEnableToSaveData = false;
        
        _logger.LogInformation($"{nameof(CanSaveInvoicePositionDetailsCommand)} was executed");
    }
}
