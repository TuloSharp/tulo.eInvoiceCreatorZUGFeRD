using PdfSharp.Pdf;
using tulo.UpgradeToPdfA3.ResultPattern;

namespace tulo.UpgradeToPdfA3.Interfaces;

/// <summary>
/// Provides functionality to write the natural language identifier
/// into a <see cref="PdfDocument"/>, as required by PDF/A-3 conformance standards
/// to ensure accessibility and proper text interpretation.
/// </summary>
public interface IPdfALanguageWriter
{
    /// <summary>
    /// Writes the specified language identifier into the provided
    /// <see cref="PdfDocument"/>.
    /// </summary>
    /// <param name="pdfDocument">
    /// The target <see cref="PdfDocument"/> into which the language
    /// identifier will be written. Must not be <see langword="null"/>.
    /// </param>
    /// <param name="language">
    /// The IETF language tag to assign to the document
    /// (e.g. <c>"de-DE"</c>, <c>"en-US"</c>).
    /// If <see langword="null"/> or empty, a fallback or default
    /// language may be applied depending on the implementation.
    /// </param>
    /// <returns>
    /// An <see cref="OperationResult"/> indicating whether the language
    /// identifier was written successfully. Contains specific error
    /// details on failure.
    /// </returns>
    OperationResult Write(PdfDocument pdfDocument, string? language);
}

