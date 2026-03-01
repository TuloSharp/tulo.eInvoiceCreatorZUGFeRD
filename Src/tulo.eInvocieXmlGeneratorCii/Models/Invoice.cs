namespace tulo.eInvoiceXmlGeneratorCii.Models;

public class Invoice
{
    public string InvoiceNumber { get; set; } = string.Empty;

    public DateTime InvoiceDate { get; set; }

    public string DocumentName { get; set; } = string.Empty;

    public string DocumentTypeCode { get; set; } = string.Empty;

    public Party Seller { get; set; } = new();
 
    public Party Buyer { get; set; } = new();

    public string Currency { get; set; } = string.Empty;

    public List<InvoiceLine> Lines { get; set; } = new();

    public PaymentDetails Payment { get; set; } = new();

    public List<InvoiceNote> Notes { get; set; } = new();

    public string BuyerReference { get; set; } = string.Empty;

    public string SellerOrderReferencedId { get; set; } = string.Empty;

    public string BuyerOrderReferencedId { get; set; } = string.Empty;

    public string ContractReferencedId { get; set; } = string.Empty;

    public string AdditionalReferencedDocumentId { get; set; } = string.Empty;

    public string AdditionalReferencedDocumentTypeCode { get; set; } = string.Empty;

    public string ProcuringProjectId { get; set; } = string.Empty;

    public string ProcuringProjectName { get; set; } = string.Empty;

    public string ReceivableAccountingAccountId { get; set; } = string.Empty;

    public decimal HeaderChargeTotalAmount { get; set; } = 0m;    // ram:ChargeTotalAmount

    public decimal HeaderAllowanceTotalAmount { get; set; } = 0m;  // ram:AllowanceTotalAmount

    public decimal HeaderTotalPrepaidAmount { get; set; } = 0m;

    public decimal HeaderDuePayableAmount { get; set; } = 0m;
}
