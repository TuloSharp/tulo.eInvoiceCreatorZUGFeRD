namespace tulo.eInvoiceApp.Options;
/// <summary>
/// Represents the root application configuration, grouping all
/// option sections used across the application.
/// Implementations are typically bound from <c>appsettings.json</c>
/// or another configuration provider.
/// </summary>
public interface IAppOptions
{
    /// <summary>
    /// Gets or sets the localization settings, including the default culture,
    /// supported cultures, and language fallback behavior.
    /// </summary>
    LocalizationOptions Localization { get; set; }

    /// <summary>
    /// Gets or sets the invoice-specific settings, such as default document
    /// type codes, payment terms, and invoice numbering configuration.
    /// </summary>
    InvoiceOptions Invoice { get; set; }

    /// <summary>
    /// Gets or sets the archive settings, including the storage path,
    /// folder structure, and file retention rules for generated invoices.
    /// </summary>
    ArchiveOptions Archive { get; set; }

    /// <summary>
    /// Gets or sets the VAT configuration, including the list of
    /// applicable VAT rates available for selection in the UI.
    /// </summary>
    VatsOptions Vats { get; set; }

    /// <summary>
    /// Gets or sets the digital signature settings, including certificate
    /// paths, signature algorithms, and signing behavior.
    /// </summary>
    SignatureOptions Signature { get; set; }
}

