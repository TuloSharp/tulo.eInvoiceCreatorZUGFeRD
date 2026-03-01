namespace tulo.eInvoiceXmlGeneratorCii.Models;

public class PaymentDiscountTermsDetails
{
    public DateTime BasisDate { get; set; }
    public int BasisPeriodDays { get; set; }
    public decimal BasisAmount { get; set; }  
    public decimal CalculationPercent { get; set; } 
    public decimal ActualDiscountAmount { get; set; }
}
