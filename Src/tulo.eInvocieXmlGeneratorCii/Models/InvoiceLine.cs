namespace tulo.eInvoiceXmlGeneratorCii.Models;

public class InvoiceLine
{
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string UnitCode { get; set; } = "C62";
    public decimal UnitPrice { get; set; }
    public decimal TaxPercent { get; set; }
    public string TaxCategory { get; set; } = "S";
    public string ProductDescription { get; set; } = string.Empty;
    public string GlobalId { get; set; } = string.Empty;
    public string GlobalIdSchemeId { get; set; } = string.Empty;
    public string SellerAssignedId { get; set; } = string.Empty;
    public string BuyerOrderReferencedId { get; set; } = string.Empty;
    public DateTime? BuyerOrderDate { get; set; }
    public string DeliveryNoteNumber { get; set; } = string.Empty;
    public string DeliveryNoteLineId { get; set; } = string.Empty;
    public DateTime? DeliveryNoteDate { get; set; }
    public DateTime? BillingPeriodEndDate { get; set; }
    public string AdditionalReferencedDocumentId { get; set; } = string.Empty;
    public string AdditionalReferencedDocumentTypeCode { get; set; } = string.Empty; 
    public string AdditionalReferencedDocumentReferenceTypeCode { get; set; } = string.Empty;
    public string OriginCountryCode { get; set; } = string.Empty;

    public string BuyerOrderLineId { get; set; } = string.Empty;
    public string LineId { get; set; } = string.Empty;
    public string ParentLineId { get; set; } = string.Empty;
    public string LineStatusReasonCode { get; set; } = string.Empty;
    public string BuyerAssignedId { get; set; } = string.Empty;
    public decimal? ForcedLineTotalAmount { get; set; }
    
    public decimal? PriceBasisQuantity { get; set; }
    public bool? OmitNetPriceBasisQuantity { get; set; }

    public List<InvoiceNote> Notes { get; set; } = new();
}
