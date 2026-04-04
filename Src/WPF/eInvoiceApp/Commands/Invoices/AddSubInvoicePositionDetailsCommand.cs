using Microsoft.Extensions.Logging;
using tulo.CommonMVVM.Collector;
using tulo.CommonMVVM.Commands;
using tulo.eInvoice.eInvoiceApp.DTOs;
using tulo.eInvoice.eInvoiceApp.Services;
using tulo.eInvoice.eInvoiceApp.ViewModels.Invoices;

namespace tulo.eInvoice.eInvoiceApp.Commands.Invoices;

public class AddSubInvoicePositionDetailsCommand(AddInvoicePositionViewModel addInvoicePositionViewModel, ICollectorCollection collectorCollection) : AsyncBaseCommand
{
    private readonly AddInvoicePositionViewModel _addInvoicePositionViewModel = addInvoicePositionViewModel;

    #region Services / Stores filled via CollectorCollection
    private readonly ILogger<AddSubInvoicePositionDetailsCommand> _logger = collectorCollection.GetService<ILoggerFactory>().CreateLogger<AddSubInvoicePositionDetailsCommand>();
    private readonly IInvoicePositionService _invoicePositionService = collectorCollection.GetService<IInvoicePositionService>();
    #endregion

    protected override async Task ExecuteAsync(object parameter)
    {
        _logger.LogInformation($"{nameof(AddSubInvoicePositionDetailsCommand)} start execution");

        _addInvoicePositionViewModel.StatusMessage = string.Empty;

        // Safety check — this command must only run in a sub-position context
        if (_addInvoicePositionViewModel.ParentPositionId is not { } parentId)
        {
            _logger.LogWarning($"{nameof(AddSubInvoicePositionDetailsCommand)} was executed without a ParentPositionId — aborted");
            return;
        }

        InvoicePositionDetailsFormViewModel invPosDetailsViewModel = _addInvoicePositionViewModel.InvoicePositionDetailsFormViewModel;

        InvoicePositionDetailsDTO newSubInvPos = new()
        {
            // ParentPositionId and LineStatusReasonCode are set by the store/service internally,
            // but we pass them here explicitly for clarity and safety
            ParentPositionId = parentId,
            LineStatusReasonCode = "DETAIL",

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
            await _invoicePositionService.AddSubInvoicePositionAsync(parentId, newSubInvPos);

            if (_invoicePositionService.IsCreated)
            {
                _addInvoicePositionViewModel.StatusMessage = string.Empty;

                // Deactivate confirmUnsavedChangesOnClose
                var @params = new object[] { null!, false };
                _addInvoicePositionViewModel.CloseAddInvoicePositionDetailsCommand.Execute(parameter: @params);
                _logger.LogInformation($"{nameof(AddSubInvoicePositionDetailsCommand)} sub-position created and modal closed");
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
            _addInvoicePositionViewModel.StatusMessage = "Technical error: " + _invoicePositionService.StatusMessage + $"\n{e.Message}";
            _logger.LogError(e, _invoicePositionService.StatusMessage);
        }
        finally
        {
            _logger.LogInformation($"Sub-position under parent {parentId} — IsCreated = {_invoicePositionService.IsCreated}");
            _logger.LogInformation($"{nameof(AddSubInvoicePositionDetailsCommand)} was executed");
        }
    }
}

