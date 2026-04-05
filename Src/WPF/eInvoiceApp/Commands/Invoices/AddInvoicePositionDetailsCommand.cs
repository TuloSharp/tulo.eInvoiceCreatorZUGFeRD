using Microsoft.Extensions.Logging;
using tulo.CommonMVVM.Collector;
using tulo.CommonMVVM.Commands;
using tulo.eInvoiceApp.DTOs;
using tulo.eInvoiceApp.Services;
using tulo.eInvoiceApp.ViewModels.Invoices;

namespace tulo.eInvoiceApp.Commands.Invoices;
public class AddInvoicePositionDetailsCommand(AddInvoicePositionViewModel addInvoicePositionViewModel, ICollectorCollection collectorCollection) : AsyncBaseCommand
{
    private AddInvoicePositionViewModel _addInvoicePositionViewModel = addInvoicePositionViewModel;

    #region Services / Stores filled via CollectorCollection
    private readonly ILogger<AddInvoicePositionDetailsCommand> _logger = collectorCollection.GetService<ILoggerFactory>().CreateLogger<AddInvoicePositionDetailsCommand>();
    private readonly IInvoicePositionService _invoicePositionService = collectorCollection.GetService<IInvoicePositionService>();
    #endregion

    protected override async Task ExecuteAsync(object parameter)
    {
        _logger.LogInformation($"{nameof(AddInvoicePositionDetailsCommand)} start exection");

        _addInvoicePositionViewModel.StatusMessage = string.Empty;

        InvoicePositionDetailsFormViewModel invPosDetailsViewModel = _addInvoicePositionViewModel.InvoicePositionDetailsFormViewModel;

        InvoicePositionDetailsDTO newInvPos = new()
        {
            InvoicePositionNr = invPosDetailsViewModel.InvoicePositionNr,
            InvoicePositionDescription = invPosDetailsViewModel.InvoicePositionDescription,
            InvoicePositionProductDescription = invPosDetailsViewModel.InvoicePositionProductDescription,
            InvoicePositionItemNr = invPosDetailsViewModel.InvoicePositionItemNr,
            InvoicePositionEan = invPosDetailsViewModel.InvoicePositionEan,

            InvoicePositionQuantity = invPosDetailsViewModel.InvoicePositionQuantity,
            InvoicePostionUnit = invPosDetailsViewModel.InvoicePostionUnit,

            InvoicePositionUnitPrice = invPosDetailsViewModel.InvoicePositionUnitPrice,
            InvoicePositionVatRate = invPosDetailsViewModel.InvoicePositionVatRate,

            InvoicePositionNetAmount = invPosDetailsViewModel.InvoicePositionNetAmount,
            InvoicePositionGrossAmount = invPosDetailsViewModel.InvoicePositionGrossAmount,

            InvoicePositionDiscountReason = invPosDetailsViewModel.InvoicePositionDiscountReason,
            InvoicePositionDiscountNetAmount = invPosDetailsViewModel.InvoicePositionDiscountNetAmount,
            InvoicePositionNetAmountAfterDiscount = invPosDetailsViewModel.InvoicePositionNetAmountAfterDiscount,

            InvoicePositionOrderDate = invPosDetailsViewModel.InvoicePositionOrderDate,
            InvoicePositionOrderId = invPosDetailsViewModel.InvoicePositionOrderId,

            InvoicePositionDeliveryNoteId = invPosDetailsViewModel.InvoicePositionDeliveryNoteId,
            InvoicePositionDeliveryNoteLineId = invPosDetailsViewModel.InvoicePositionDeliveryNoteLineId,
            InvoicePositionDeliveryNoteDate = invPosDetailsViewModel.InvoicePositionDeliveryNoteDate,

            InvoicePositionRefDocId = invPosDetailsViewModel.InvoicePositionRefDocId,
            InvoicePositionRefDocType = invPosDetailsViewModel.InvoicePositionRefDocType,
            InvoicePositionRefDocRefType = invPosDetailsViewModel.InvoicePositionRefDocRefType,

            InvoicePositionSelectedVatCategory = invPosDetailsViewModel.InvoicePositionSelectedVatCategory
        };

        try
        {
            await _invoicePositionService.AddInvoicePositionAsync(newInvPos);
            if (_invoicePositionService.IsCreated)
            {
                _addInvoicePositionViewModel.StatusMessage = string.Empty;

                //deactivate confirmUnsavedChangesOnClose
                var @params = new object[] { null!, false };
                _addInvoicePositionViewModel.CloseAddInvoicePositionDetailsCommand.Execute(parameter: @params);
                _logger.LogInformation($"{nameof(AddInvoicePositionDetailsCommand)} is closed");
            }
            else
            {
                _addInvoicePositionViewModel.InvalidStateAtInputField = _invoicePositionService.AreRequiredFieldsFilled;
                _addInvoicePositionViewModel.StatusMessage = _invoicePositionService.StatusMessage;
                _logger.LogInformation(_invoicePositionService.StatusMessage);
            }
        }
        catch (Exception e)
        {
            _addInvoicePositionViewModel.StatusMessage = "Technical error" + _invoicePositionService.StatusMessage + $"\n{e.Message}";
            _logger.LogError(e, _invoicePositionService.StatusMessage);
        }
        finally
        {
            _logger.LogInformation(newInvPos.InvoicePositionNr.ToString() + " is created = " + _invoicePositionService.IsCreated);
            _logger.LogInformation($"{nameof(AddInvoicePositionDetailsCommand)} was executed");
        }
    }
}
