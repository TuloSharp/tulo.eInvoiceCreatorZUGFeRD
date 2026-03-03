using Microsoft.Extensions.Logging;
using tulo.CommonMVVM.Collector;
using tulo.CommonMVVM.Commands;
using tulo.eInvoice.eInvoiceApp.DTOs;
using tulo.eInvoice.eInvoiceApp.Services;
using tulo.eInvoice.eInvoiceApp.ViewModels.Invoices;

namespace tulo.eInvoice.eInvoiceApp.Commands.Invoices;
public class EditInvoicePositionDetailsCommand(EditInvoicePositionViewModel editInvoicePositionViewModel, ICollectorCollection collectorCollection) : AsyncBaseCommand
{
    private EditInvoicePositionViewModel _editInvoicePositionViewModel = editInvoicePositionViewModel;

    #region Services / Stores filled via CollectorCollection
    private readonly ILogger<EditInvoicePositionDetailsCommand> _logger = collectorCollection.GetService<ILoggerFactory>().CreateLogger<EditInvoicePositionDetailsCommand>();
    private readonly IInvoicePositionService _invoicePositionService = collectorCollection.GetService<IInvoicePositionService>();
    #endregion

    protected override async Task ExecuteAsync(object parameter)
    {
        _logger.LogInformation($"{nameof(AddInvoicePositionDetailsCommand)} start exection");

        _editInvoicePositionViewModel.StatusMessage = string.Empty;

        InvoicePositionDetailsFormViewModel invPosDetailsViewModel = _editInvoicePositionViewModel.InvoicePositionDetailsFormViewModel;

        InvoicePositionDetailsDTO updatedInvPos = new()
        {
            Id = invPosDetailsViewModel.Id,
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
            await _invoicePositionService.UpdateInvoicePositionAsync(updatedInvPos.Id, updatedInvPos);
            if (_invoicePositionService.IsCreated)
            {
                _editInvoicePositionViewModel.StatusMessage = string.Empty;

                //deactivate confirmUnsavedChangesOnClose
                var @params = new object[] { null!, false };
                _editInvoicePositionViewModel.CloseEditInvoicePositionDetailsCommand.Execute(parameter: @params);
                _logger.LogInformation($"{nameof(EditInvoicePositionDetailsCommand)} is closed");
            }
            else
            {
                _editInvoicePositionViewModel.InvalidStateAtInputField = _invoicePositionService.AreRequiredFieldsFilled;
                _editInvoicePositionViewModel.StatusMessage = _invoicePositionService.StatusMessage;
                _logger.LogInformation(_invoicePositionService.StatusMessage);
            }
        }
        catch (Exception e)
        {
            _editInvoicePositionViewModel.StatusMessage = "Technical error" + _invoicePositionService.StatusMessage + $"\n{e.Message}";
            _logger.LogError(e, _invoicePositionService.StatusMessage);
        }
        finally
        {
            _logger.LogInformation(updatedInvPos.InvoicePositionNr.ToString() + " is created = " + _invoicePositionService.IsCreated);
            _logger.LogInformation($"{nameof(EditInvoicePositionDetailsCommand)} was ececuted");
        }
    }
}
