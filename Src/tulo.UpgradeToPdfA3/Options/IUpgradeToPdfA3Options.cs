namespace tulo.UpgradeToPdfA3.Options;

/// <summary>
/// Represents the configuration options required to perform a PDF/A-3 upgrade,
/// providing access to all settings needed for conformance level declaration,
/// metadata, colour profile, and XML attachment configuration.
/// </summary>
public interface IUpgradeToPdfA3Options
{
    /// <summary>
    /// Gets or sets the PDF/A-3 specific settings used during the upgrade process,
    /// including conformance level, ICC colour profile, XMP metadata values,
    /// and XML attachment relationship type.
    /// </summary>
    PdfA3Options PdfA3 { get; set; }
}

