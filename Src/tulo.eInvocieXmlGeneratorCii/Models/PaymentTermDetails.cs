namespace tulo.eInvoiceXmlGeneratorCii.Models;

public class PaymentTermDetails
{
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public PaymentDiscountTermsDetails? DiscountTerms { get; set; }
}
