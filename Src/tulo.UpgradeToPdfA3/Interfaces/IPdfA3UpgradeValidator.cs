using tulo.UpgradeToPdfA3.Options;
using tulo.UpgradeToPdfA3.ResultPattern;

namespace tulo.UpgradeToPdfA3.Interfaces;

/// <summary>
/// Provides validation logic to verify that all prerequisites are met
/// before upgrading a PDF document to PDF/A-3 conformance with an embedded XML attachment.
/// Should be executed prior to the actual upgrade and attachment process.
/// </summary>
public interface IPdfA3UpgradeValidator
{
    /// <summary>
    /// Validates all inputs and preconditions required for a successful
    /// PDF/A-3 upgrade with an embedded XML attachment.
    /// </summary>
    /// <param name="inputPdfAPath">
    /// The file system path to the source PDF/A document to be upgraded.
    /// Must point to an existing, accessible PDF file.
    /// </param>
    /// <param name="outputPdfA3Path">
    /// The file system path where the upgraded PDF/A-3 document will be written.
    /// The directory must exist and be writable.
    /// </param>
    /// <param name="xmlFileName">
    /// The file name to assign to the embedded XML attachment
    /// (e.g. <c>"factur-x.xml"</c> or <c>"ZUGFeRD-invoice.xml"</c>).
    /// Must not be <see langword="null"/> or empty.
    /// </param>
    /// <param name="xmlBytes">
    /// The raw XML content to be embedded as a byte array.
    /// Must not be <see langword="null"/> or empty.
    /// </param>
    /// <param name="appOptions">
    /// The PDF/A-3 upgrade options providing conformance level, metadata,
    /// and attachment relationship settings required for the upgrade process.
    /// </param>
    /// <returns>
    /// An <see cref="OperationResult"/> indicating whether all validation
    /// checks passed successfully. Contains specific error details if any
    /// precondition is not met.
    /// </returns>
    OperationResult Validate(string inputPdfAPath, string outputPdfA3Path, string xmlFileName, byte[] xmlBytes, IUpgradeToPdfA3Options appOptions);
}
