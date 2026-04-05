using Microsoft.Extensions.Options;
using tulo.CommonMVVM.Collector;
using tulo.eInvoiceApp.Options;
using tulo.eInvoiceApp.Stores.Invoices;
using tulo.eInvoiceApp.ViewModels.Invoices;
using tulo.eInvoiceXmlGeneratorCii.Models;

namespace tulo.eInvoiceApp.Services;

public sealed class InvoiceBuilderService(ICollectorCollection collectorCollection) : IInvoiceBuilderService
{
    #region Get filled via CollectorCollection
    private readonly IInvoicePositionStore _invoicePositionStore = collectorCollection.GetService<IInvoicePositionStore>();
    private readonly IOptions<AppOptions> _appOptions = collectorCollection.GetService<IOptions<AppOptions>>();
    #endregion

    public async Task<Invoice> BuildAsync(InvoiceViewModel invoiceViewModel, CancellationToken ct = default)
    {
        // Do not throw. If vm is null, return an empty invoice object.
        if (invoiceViewModel is null)
            return new Invoice();

        var invoice = invoiceViewModel.Invoice;

        // 1) Fill invoice from VM fields (Buyer + Payment).
        FillInvoiceFromViewModel(invoiceViewModel, invoice);

        // 2) Fill invoice lines from store.
        await FillLinesFromStoreAsync(invoice);

        // 3) Apply appsettings enrichment (Seller + Payment account + Notes).
        ApplySellerFromAppOptions(invoice);
        ApplyPaymentAccountFromAppOptions(invoice);
        ApplyNotesFromAppOptions(invoice);

        // 4) Recalculate totals (if values are empty, results will still be consistent).
        RecalculateHeaderAmounts(invoice);

        return invoice;
    }

    // Maps the ViewModel fields into the Invoice object.
    // If ViewModel fields are empty, the invoice fields stay empty.
    private static void FillInvoiceFromViewModel(InvoiceViewModel invoiceViewModel, Invoice invoice)
    {
        invoice.Buyer ??= new eInvoiceXmlGeneratorCii.Models.Party();
        invoice.Payment ??= new PaymentDetails();

        // Invoice header
        invoice.InvoiceNumber = invoiceViewModel.InvoiceNumber?.Trim() ?? string.Empty;
        invoice.Currency = invoiceViewModel.Currency?.Trim() ?? string.Empty;
        invoice.DocumentName = invoiceViewModel.DocumentName?.Trim() ?? string.Empty;
        invoice.DocumentTypeCode = invoiceViewModel.DocumentTypeCode?.Trim() ?? string.Empty;

        // Buyer
        invoice.Buyer.Name = invoiceViewModel.CompanyBuyerParty?.Trim() ?? string.Empty;
        invoice.Buyer.FiscalId = string.IsNullOrWhiteSpace(invoiceViewModel.FiscalIdBuyerParty) ? string.Empty : invoiceViewModel.FiscalIdBuyerParty.Trim();
        invoice.Buyer.LegalOrganizationId = invoice.Buyer.FiscalId!;
        invoice.Buyer.IdSchemeId = "0002"; //ToDo
        invoice.Buyer.VatId = invoiceViewModel.VatIdBuyerParty?.Trim() ?? string.Empty;

        // Use ERP customer number as buyer ID (your template used this as Buyer.ID).
        invoice.Buyer.ID = invoiceViewModel.ErpCustomerNumberBuyerParty?.Trim() ?? string.Empty;

        invoice.Buyer.LeitwegId = invoiceViewModel.LeitwegIdBuyerParty?.Trim() ?? string.Empty;

        // Street + house number
        var street = invoiceViewModel.StreetBuyerParty?.Trim() ?? string.Empty;
        var house = invoiceViewModel.HouseNumberBuyerParty?.Trim() ?? string.Empty;
        invoice.Buyer.Street = string.IsNullOrWhiteSpace(house) ? street : $"{street} {house}".Trim();

        invoice.Buyer.Zip = invoiceViewModel.PostalCodeBuyerParty?.Trim() ?? string.Empty;
        invoice.Buyer.City = invoiceViewModel.CityBuyerParty?.Trim() ?? string.Empty;
        invoice.Buyer.CountryCode = invoiceViewModel.CountryCodeBuyerParty?.Trim() ?? string.Empty;

        invoice.Buyer.ContactPersonName = string.IsNullOrWhiteSpace(invoiceViewModel.PersonBuyerParty) ? string.Empty : invoiceViewModel.PersonBuyerParty.Trim();
        invoice.Buyer.ContactPhone = string.IsNullOrWhiteSpace(invoiceViewModel.PhoneBuyerParty) ? string.Empty : invoiceViewModel.PhoneBuyerParty.Trim();
        invoice.Buyer.ContactEmail = string.IsNullOrWhiteSpace(invoiceViewModel.EmailAddressBuyerParty) ? string.Empty : invoiceViewModel.EmailAddressBuyerParty.Trim();

        // Payment (no parsing/validation; due date stays untouched unless you have a real DateTime property)
        invoice.Payment.PaymentMeansTypeCode = invoiceViewModel.PaymentMeansCode?.Trim() ?? string.Empty;
        invoice.Payment.PaymentReference = invoiceViewModel.PaymentReference?.Trim() ?? string.Empty;

        //Payment terms text is determined by whether a discount is present or not. If discount is present, use the discount preview text, otherwise use the no-discount preview text.
        invoice.Payment.PaymentTermsText = invoiceViewModel.HasDiscount ? invoiceViewModel.DiscountPreviewText?.Trim() ?? string.Empty : invoiceViewModel.NoDiscountPreviewText?.Trim() ?? string.Empty;

        if (invoiceViewModel.PaymentDueDate.HasValue)
        {
            var d = invoiceViewModel.PaymentDueDate.Value;
            invoice.Payment.DueDate = new DateTime(d.Year, d.Month, d.Day);
        }
        else
        {
            invoice.Payment.DueDate = null;
        }

        invoice.Payment.Terms.Clear();

        var hasCompleteDiscountData = invoiceViewModel.HasDiscount &&
                                          invoiceViewModel.DiscountBasisDate.HasValue &&
                                          !string.IsNullOrWhiteSpace(invoiceViewModel.DiscountDays) &&
                                          invoiceViewModel.DiscountPercent > 0 &&
                                          !string.IsNullOrWhiteSpace(invoiceViewModel.DiscountPreviewText) &&
                                          !string.IsNullOrWhiteSpace(invoiceViewModel.NoDiscountPreviewText);

        if (hasCompleteDiscountData)
        {
            var basisDateValue = invoiceViewModel.DiscountBasisDate!.Value;
            var discountBasisDate = new DateTime(basisDateValue.Year, basisDateValue.Month, basisDateValue.Day);

            int basisPeriodDays = 0;
            int.TryParse(invoiceViewModel.DiscountDays!.Trim(), out basisPeriodDays);

            invoice.Payment.Terms.Add(new PaymentTermDetails
            {
                Description = invoiceViewModel.DiscountPreviewText!.Trim(),
                DueDate = null,
                DiscountTerms = new PaymentDiscountTermsDetails
                {
                    BasisDate = discountBasisDate,
                    BasisPeriodDays = basisPeriodDays,
                    BasisAmount = 0m,
                    CalculationPercent = invoiceViewModel.DiscountPercent,
                    ActualDiscountAmount = 0m
                }
            });

            invoice.Payment.Terms.Add(new PaymentTermDetails
            {
                Description = invoiceViewModel.NoDiscountPreviewText!.Trim(),
                DueDate = invoice.Payment.DueDate,
                DiscountTerms = null
            });
        }
    }

    // Loads invoice positions from store and fills invoice.Lines.
    private async Task FillLinesFromStoreAsync(Invoice invoice)
    {
        var result = await _invoicePositionStore.GetAllAsync();
        if (!result.Success || result.Data == null)
            return;

        invoice.Lines.Clear();

        foreach (var dto in result.Data)
        {
            var line = new InvoiceLine
            {
                Description = dto.InvoicePositionDescription ?? string.Empty,
                ProductDescription = dto.InvoicePositionDescription ?? string.Empty,

                Quantity = dto.InvoicePositionQuantity,
                //C62 is the UN/CEFACT common code for “piece”
                UnitCode = string.IsNullOrWhiteSpace(dto.InvoicePostionUnit) ? "C62" : dto.InvoicePostionUnit,
                UnitPrice = dto.InvoicePositionUnitPrice,

                TaxPercent = dto.InvoicePositionVatRate,
                TaxCategory = string.IsNullOrWhiteSpace(dto.InvoicePositionVatCategoryCode) ? "S" : dto.InvoicePositionVatCategoryCode,

                // Discount handling: store already provides NetAmountAfterDiscount.
                ForcedLineTotalAmount = dto.InvoicePositionNetAmountAfterDiscount ?? dto.InvoicePositionNetAmount,

                // Identifiers
                SellerAssignedId = dto.InvoicePositionItemNr ?? string.Empty,
                GlobalId = dto.InvoicePositionEan ?? string.Empty,
                //if EAN is present → set scheme "0160" so receivers know it’s a GTIN/EAN
                GlobalIdSchemeId = string.IsNullOrWhiteSpace(dto.InvoicePositionEan) ? string.Empty : "0160",

                // Order reference
                BuyerOrderReferencedId = dto.InvoicePositionOrderId ?? string.Empty,
                BuyerOrderDate = dto.InvoicePositionOrderDate.HasValue
                ? new DateTime(dto.InvoicePositionOrderDate.Value.Year, dto.InvoicePositionOrderDate.Value.Month, dto.InvoicePositionOrderDate.Value.Day)
                : null,

                // Delivery note reference
                DeliveryNoteNumber = dto.InvoicePositionDeliveryNoteId ?? string.Empty,
                DeliveryNoteLineId = dto.InvoicePositionDeliveryNoteLineId ?? string.Empty,
                DeliveryNoteDate = dto.InvoicePositionDeliveryNoteDate.HasValue
                ? new DateTime(dto.InvoicePositionDeliveryNoteDate.Value.Year, dto.InvoicePositionDeliveryNoteDate.Value.Month, dto.InvoicePositionDeliveryNoteDate.Value.Day)
                : null,

                // Additional referenced document
                AdditionalReferencedDocumentId = dto.InvoicePositionRefDocId ?? string.Empty,
                AdditionalReferencedDocumentTypeCode = dto.InvoicePositionRefDocType ?? string.Empty,
                AdditionalReferencedDocumentReferenceTypeCode = dto.InvoicePositionRefDocRefType ?? string.Empty,
            };

            invoice.Lines.Add(line);
        }
    }

    // Applies seller data from appsettings Invoice.Seller.
    private void ApplySellerFromAppOptions(Invoice invoice)
    {
        var s = _appOptions.Value.Invoice?.Seller;
        if (s == null) return;

        invoice.Seller ??= new eInvoiceXmlGeneratorCii.Models.Party();

        invoice.Seller.ID = s.ID ?? "";
        invoice.Seller.Name = s.Name ?? "";
        invoice.Seller.Street = s.Street ?? "";
        invoice.Seller.Zip = s.Zip ?? "";
        invoice.Seller.City = s.City ?? "";
        invoice.Seller.CountryCode = s.CountryCode ?? "";
        invoice.Seller.VatId = s.VatId ?? "";
        invoice.Seller.LeitwegId = s.LeitwegId ?? "";
        invoice.Seller.FiscalId = s.FiscalId;
        invoice.Seller.GeneralEmail = s.GeneralEmail;
        invoice.Seller.ContactPersonName = s.ContactPersonName;
        invoice.Seller.ContactPhone = s.ContactPhone;

        // Prefer ContactEmail, fallback to GeneralEmail.
        invoice.Seller.ContactEmail = !string.IsNullOrWhiteSpace(s.ContactEmail)
            ? s.ContactEmail
            : s.GeneralEmail;
    }

    // Applies payment account data from appsettings Invoice.Payment.
    private void ApplyPaymentAccountFromAppOptions(Invoice invoice)
    {
        var p = _appOptions.Value.Invoice?.Payment;
        if (p == null) return;

        invoice.Payment ??= new PaymentDetails();
        invoice.Payment.Iban = p.Iban ?? "";
        invoice.Payment.Bic = p.Bic ?? "";
        invoice.Payment.AccountName = p.AccountName ?? "";
    }

    // Adds invoice notes from appsettings Invoice.Notes.
    private void ApplyNotesFromAppOptions(Invoice invoice)
    {
        var notes = _appOptions.Value.Invoice?.Notes;
        if (notes == null) return;

        foreach (var n in notes)
        {
            invoice.Notes.Add(new InvoiceNote
            {
                SubjectCode = string.IsNullOrWhiteSpace(n.SubjectCode) ? "REG" : n.SubjectCode,
                Content = n.Content ?? "",
                ContentCode = ""
            });
        }
    }

    // Recalculates header totals based on invoice lines.
    private static void RecalculateHeaderAmounts(Invoice invoice)
    {
        decimal sumNet = 0m;
        decimal sumTax = 0m;

        foreach (var l in invoice.Lines)
        {
            var lineNet = l.ForcedLineTotalAmount.HasValue
                ? Round2(l.ForcedLineTotalAmount.Value)
                : Round2(l.Quantity * l.UnitPrice);

            var lineTax = Round2(lineNet * (l.TaxPercent / 100m));

            sumNet += lineNet;
            sumTax += lineTax;
        }

        sumNet = Round2(sumNet);
        sumTax = Round2(sumTax);

        var charge = Round2(invoice.HeaderChargeTotalAmount);
        var allowance = Round2(invoice.HeaderAllowanceTotalAmount);
        var prepaid = Round2(invoice.HeaderTotalPrepaidAmount);

        var taxBasis = Round2(sumNet + charge - allowance);
        var grandTotal = Round2(taxBasis + sumTax);

        invoice.HeaderDuePayableAmount = Round2(grandTotal - prepaid);
    }

    private static decimal Round2(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);
}