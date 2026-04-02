using PdfSharp.Pdf;
using tulo.UpgradeToPdfA3.Options;
using tulo.UpgradeToPdfA3.ResultPattern;

namespace tulo.UpgradeToPdfA3.Interfaces;

/// <summary>
/// Provides functionality to apply PDF/A conformance requirements
/// to an existing <see cref="PdfDocument"/> in place.
/// Orchestrates the necessary steps such as writing metadata,
/// output intent, document info, and language settings
/// to produce a PDF/A-compliant document.
/// </summary>
public interface IToPdfAConverterService
{
    /// <summary>
    /// Applies all PDF/A conformance requirements to the provided
    /// <see cref="PdfDocument"/> by writing the necessary metadata,
    /// output intent, document info, and language identifier.
    /// </summary>
    /// <param name="pdfDocument">
    /// The target <see cref="PdfDocument"/> to which PDF/A conformance
    /// requirements will be applied. The document is modified in place.
    /// Must not be <see langword="null"/>.
    /// </param>
    /// <param name="appOptions">
    /// The PDF/A-3 upgrade options providing conformance level, colour profile,
    /// metadata values, and language settings required for the conversion.
    /// Must not be <see langword="null"/>.
    /// </param>
    /// <returns>
    /// An <see cref="OperationResult"/> indicating whether the PDF/A
    /// conformance requirements were applied successfully.
    /// Contains specific error details on failure.
    /// </returns>
    OperationResult ApplyPdfA(PdfDocument pdfDocument, IUpgradeToPdfA3Options appOptions);
}

