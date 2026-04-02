using tulo.UpgradeToPdfA3.Options;
using tulo.UpgradeToPdfA3.ResultPattern;

namespace tulo.UpgradeToPdfA3.Interfaces;

/// <summary>
/// Provides functionality to upgrade an existing PDF/A document to PDF/A-3 conformance
/// and embed an XML file as an attachment, as required by e-invoicing standards
/// such as ZUGFeRD and Factur-X.
/// Orchestrates validation, metadata writing, output intent, and XML attachment
/// in a single upgrade operation.
/// </summary>
public interface IToPdfA3UpgradeService
{
    /// <summary>
    /// Upgrades the specified PDF/A document to PDF/A-3 conformance and embeds
    /// the provided XML data as a named attachment into the resulting document.
    /// </summary>
    /// <param name="inputPdfAPath">
    /// The file system path to the source PDF/A document to be upgraded.
    /// Must point to an existing, accessible PDF file.
    /// </param>
    /// <param name="outputPdfA3Path">
    /// The file system path where the upgraded PDF/A-3 document will be written.
    /// The target directory must exist and be writable.
    /// </param>
    /// <param name="xmlFileName">
    /// The file name to assign to the embedded XML attachment
    /// (e.g. <c>"factur-x.xml"</c> or <c>"ZUGFeRD-invoice.xml"</c>).
    /// Must not be <see langword="null"/> or empty.
    /// </param>
    /// <param name="xmlBytes">
    /// The raw XML content to embed as a byte array.
    /// Must not be <see langword="null"/> or empty.
    /// </param>
    /// <param name="appOptions">
    /// The PDF/A-3 upgrade options providing conformance level, colour profile,
    /// metadata values, and attachment relationship settings required
    /// for the upgrade process.
    /// Must not be <see langword="null"/>.
    /// </param>
    /// <returns>
    /// An <see cref="OperationResult"/> indicating whether the upgrade
    /// and XML embedding completed successfully. Contains specific error
    /// details on failure.
    /// </returns>
    OperationResult UpgradeToPdfA3(string inputPdfAPath, string outputPdfA3Path, string xmlFileName, byte[] xmlBytes, IUpgradeToPdfA3Options appOptions);
}

