namespace tulo.eInvoice.eInvoiceApp.DTOs;
public class InvoicePositionDetailsDTO
{
    public Guid Id { get; set; }
    public int InvoicePositionNr { get; set; }
    public string InvoicePositionDescription { get; set; } = string.Empty;
    public string InvoicePositionProductDescription { get; set; } = string.Empty;
    public string InvoicePositionItemNr { get; set; } = string.Empty;
    public string InvoicePositionEan { get; set; } = string.Empty;

    public decimal InvoicePositionQuantity { get; set; }
    public string InvoicePostionUnit { get; set; } = string.Empty;

    public decimal InvoicePositionUnitPrice { get; set; }

    public int InvoicePositionVatRate { get; set; }
    public string InvoicePositionVatCategoryCode { get; set; } = string.Empty;

    public decimal InvoicePositionNetAmount { get; set; }
    public decimal InvoicePositionGrossAmount { get; set; }

    public string InvoicePositionDiscountReason { get; set; } = string.Empty;

    public decimal InvoicePositionDiscountNetAmount { get; set; }
    public decimal? InvoicePositionNetAmountAfterDiscount { get; set; }

    public DateOnly? InvoicePositionOrderDate { get; set; }
    public string InvoicePositionOrderId { get; set; } = string.Empty;

    public DateOnly? InvoicePositionDeliveryNoteDate { get; set; }
    public string InvoicePositionDeliveryNoteId { get; set; } = string.Empty;
    public string InvoicePositionDeliveryNoteLineId { get; set; } = string.Empty;

    public string InvoicePositionRefDocId { get; set; } = string.Empty;
    public string InvoicePositionRefDocType { get; set; } = string.Empty;
    public string InvoicePositionRefDocRefType { get; set; } = string.Empty;

    public object? InvoicePositionSelectedVatCategory { get; set; }
}
