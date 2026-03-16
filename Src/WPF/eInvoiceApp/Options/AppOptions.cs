namespace tulo.eInvoice.eInvoiceApp.Options;
public class AppOptions : IAppOptions
{
    public LocalizationOptions Localization { get; set; } = new();

    public InvoiceOptions Invoice { get; set; } = new();

    public ArchiveOptions Archive { get; set; } = new();

    public VatsOptions Vats { get; set; } = new();
}

public class InvoiceOptions
{
    public SellerOptions Seller { get; set; } = new();
    public PaymentOptions Payment { get; set; } = new();
    public List<InvoiceNoteOptions> Notes { get; set; } = new();
}

public class SellerOptions
{
    public string ID { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string Zip { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public string VatId { get; set; } = string.Empty;
    public string LeitwegId { get; set; } = string.Empty;
    public string FiscalId { get; set; } = string.Empty;
    public string GeneralEmail { get; set; } = string.Empty;
    public string ContactPersonName { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
}

public class PaymentOptions
{
    public string Iban { get; set; } = string.Empty;
    public string Bic { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
}

public class InvoiceNoteOptions
{
    public string Content { get; set; } = string.Empty;
    public string SubjectCode { get; set; } = string.Empty;
}

public sealed class VatsOptions
{
    public List<int> VatList { get; set; } = new();
}
public sealed class ArchiveOptions
{
    public string OutputPath { get; set; } = string.Empty;
}

public sealed class LocalizationOptions
{
    public string DefaultCulture { get; set; } = "de-DE";
    public string[] SupportedCultures { get; set; } = Array.Empty<string>();
}
