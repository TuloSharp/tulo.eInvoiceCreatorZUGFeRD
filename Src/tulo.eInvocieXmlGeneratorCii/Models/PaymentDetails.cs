namespace tulo.eInvoiceXmlGeneratorCii.Models;

public class PaymentDetails
{
    public string Iban { get; set; } = string.Empty;
    public string Bic { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string PaymentTermsText { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public string DirectDebitMandateId { get; set; } = string.Empty;
    public string PaymentMeansTypeCode { get; set; } = string.Empty;
    public string PaymentMeansInformation { get; set; } = string.Empty;
    public string PaymentReference { get; set; } = string.Empty;
    public List<PaymentTermDetails> Terms { get; set; } = new();
}
