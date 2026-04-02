using PdfSharp.Pdf;
using tulo.UpgradeToPdfA3.Options;
using tulo.UpgradeToPdfA3.ResultPattern;

namespace tulo.UpgradeToPdfA3.Interfaces;

/// <summary>
/// Provides validation logic to verify that a <see cref="PdfDocument"/>
/// and the associated upgrade options meet all requirements before
/// attempting a PDF/A conversion.
/// Should be executed prior to the actual conversion process.
/// </summary>
public interface IPdfAConverterValidator
{
    /// <summary>
    /// Validates the provided <see cref="PdfDocument"/> and upgrade options
    /// to ensure all preconditions for a successful PDF/A conversion are met.
    /// </summary>
    /// <param name="pdfDocument">
    /// The <see cref="PdfDocument"/> to be validated before conversion.
    /// Checks may include document integrity, encryption state,
    /// and existing conformance level.
    /// Must not be <see langword="null"/>.
    /// </param>
    /// <param name="appOptions">
    /// The PDF/A-3 upgrade options providing conformance level, metadata,
    /// and attachment relationship settings required for the conversion.
    /// Must not be <see langword="null"/>.
    /// </param>
    /// <returns>
    /// An <see cref="OperationResult"/> indicating whether all validation
    /// checks passed successfully. Contains specific error details if any
    /// precondition is not met.
    /// </returns>
    OperationResult Validate(PdfDocument pdfDocument, IUpgradeToPdfA3Options appOptions);
}

