using Microsoft.Extensions.Configuration;

namespace tulo.eInvoice.eInvoiceApp.Options;
internal class AppOptions : IAppOptions
{
    public LanguageOptions Language { get; set; } = new();

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
    [ConfigurationKeyName("VatList")]
    private List<int> VatListListMutable { get; } = [];
    private IReadOnlyList<int>? _vatListRo;

    public IReadOnlyList<int> VatList => _vatListRo ??= VatListListMutable.AsReadOnly();
}
public sealed class ArchiveOptions
{
    public string OutputPathPdfA3 { get; set; } = string.Empty;
}

public sealed class LanguageOptions
{
    public string Culture { get; set; } = string.Empty;
}
