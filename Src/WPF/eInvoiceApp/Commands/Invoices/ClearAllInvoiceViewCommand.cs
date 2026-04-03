using Microsoft.Extensions.Logging;
using tulo.CommonMVVM.Collector;
using tulo.CommonMVVM.Commands;
using tulo.eInvoice.eInvoiceApp.Services;
using tulo.eInvoice.eInvoiceApp.ViewModels.Invoices;

namespace tulo.eInvoice.eInvoiceApp.Commands.Invoices;

public class ClearAllInvoiceViewCommand(InvoiceViewModel invoiceViewModel, ICollectorCollection collectorCollection) : AsyncBaseCommand
{
    #region Services / Stores filled via CollectorCollection
    private readonly ILogger<ClearAllInvoiceViewCommand> _logger =
        collectorCollection.GetService<ILoggerFactory>().CreateLogger<ClearAllInvoiceViewCommand>();
    private readonly IInvoicePositionService _invoicePositionService =
        collectorCollection.GetService<IInvoicePositionService>();
    #endregion

    protected override async Task ExecuteAsync(object parameter)
    {
        _logger.LogInformation("{Command} start execution", nameof(ClearAllInvoiceViewCommand));

        try
        {
            // 1. Delete all invoice positions from the store
            //    ToList() first — never modify a collection while iterating it
            var positionIds = invoiceViewModel.InvoicePositionCardListItemCollectionView
                .Cast<InvoicePositionCardItemViewModel>()
                .Select(vm => vm.InvoicePositionId)
                .ToList();

            foreach (var id in positionIds)
                await _invoicePositionService.DeleteInvoicePositionAsync(id);

            // 2. Clear invoice header
            invoiceViewModel.InvoiceNumber = string.Empty;
            invoiceViewModel.Currency = string.Empty;
            invoiceViewModel.DocumentName = string.Empty;
            invoiceViewModel.DocumentTypeCode = string.Empty;

            // 3. Clear buyer party
            invoiceViewModel.CompanyBuyerParty = string.Empty;
            invoiceViewModel.FiscalIdBuyerParty = string.Empty;
            invoiceViewModel.VatIdBuyerParty = string.Empty;
            invoiceViewModel.ErpCustomerNumberBuyerParty = string.Empty;
            invoiceViewModel.LeitwegIdBuyerParty = string.Empty;
            invoiceViewModel.PersonBuyerParty = string.Empty;
            invoiceViewModel.StreetBuyerParty = string.Empty;
            invoiceViewModel.HouseNumberBuyerParty = string.Empty;
            invoiceViewModel.PostalCodeBuyerParty = string.Empty;
            invoiceViewModel.CityBuyerParty = string.Empty;
            invoiceViewModel.CountryCodeBuyerParty = string.Empty;
            invoiceViewModel.PhoneBuyerParty = string.Empty;
            invoiceViewModel.EmailAddressBuyerParty = string.Empty;

            // 4. Clear payment info
            //    Setting the text fields is enough — their setters call SetDateText
            //    which resets PaymentDueDate to null automatically when empty
            invoiceViewModel.PaymentMeansCode = string.Empty;
            invoiceViewModel.PaymentReference = string.Empty;
            invoiceViewModel.PaymentTerms = string.Empty;
            invoiceViewModel.PaymentDueDateText = string.Empty;

            // 5. Clear payment terms
            //    DiscountBasisDateText and PaymentDueDateRangeText reset their
            //    DateOnly? backing fields to null via SetDateText when empty
            invoiceViewModel.HasDiscount = false;
            invoiceViewModel.DiscountPercent = 0;
            invoiceViewModel.DiscountDays = string.Empty;
            invoiceViewModel.DiscountBasisDateText = string.Empty;
            invoiceViewModel.PaymentDueDateRangeText = string.Empty;

            // 6. Clear UI state
            invoiceViewModel.StatusMessage = string.Empty;
            invoiceViewModel.IsDuplicate = false;

            // 7. Clear search text
            invoiceViewModel.SearchText = string.Empty;

            _logger.LogInformation("{Command} cleared invoice view successfully", nameof(ClearAllInvoiceViewCommand));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{Command} failed: {Message}", nameof(ClearAllInvoiceViewCommand), e.Message);
        }
        finally
        {
            _logger.LogInformation("{Command} was executed", nameof(ClearAllInvoiceViewCommand));
        }
    }
}

