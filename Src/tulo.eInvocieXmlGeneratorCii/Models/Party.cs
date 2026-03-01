namespace tulo.eInvoiceXmlGeneratorCii.Models;

public class Party
{
    public string ID { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string Zip { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public string VatId { get; set; } = string.Empty;
    public string LeitwegId { get; set; } = string.Empty;
    public string? FiscalId { get; set; }
    public string? GeneralEmail { get; set; }
    public string? ContactPersonName { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public string? LegalOrganizationId { get; set; }
}
