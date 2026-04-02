using PdfSharp.Pdf;
using tulo.UpgradeToPdfA3.Options;
using tulo.UpgradeToPdfA3.ResultPattern;

namespace tulo.UpgradeToPdfA3.Interfaces;

/// <summary>
/// Provides functionality to write PDF/A-compliant document metadata
/// (such as title, author, subject, and creator) into a <see cref="PdfDocument"/>.
/// Correct document info is required to meet PDF/A-3 conformance standards.
/// </summary>
public interface IPdfADocumentInfoWriter
{
    /// <summary>
    /// Writes the required document metadata into the provided <see cref="PdfDocument"/>
    /// using the values defined in the given upgrade options.
    /// </summary>
    /// <param name="pdfDocument">
    /// The target <see cref="PdfDocument"/> into which the document metadata
    /// will be written. Must not be <see langword="null"/>.
    /// </param>
    /// <param name="appOptions">
    /// The PDF/A-3 upgrade options providing the metadata values to write,
    /// such as title, author, subject, creator, and producer information.
    /// Must not be <see langword="null"/>.
    /// </param>
    /// <returns>
    /// An <see cref="OperationResult"/> indicating whether the metadata
    /// was written successfully. Contains specific error details on failure.
    /// </returns>
    OperationResult Write(PdfDocument pdfDocument, IUpgradeToPdfA3Options appOptions);
}

