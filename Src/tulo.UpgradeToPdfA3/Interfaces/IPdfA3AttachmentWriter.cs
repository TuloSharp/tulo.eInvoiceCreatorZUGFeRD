using PdfSharp.Pdf;
using tulo.UpgradeToPdfA3.Options;
using tulo.UpgradeToPdfA3.ResultPattern;

namespace tulo.UpgradeToPdfA3.Interfaces;

/// <summary>
/// Provides functionality to embed an XML file as an attachment
/// into an existing PDF document and upgrade it to PDF/A-3 conformance,
/// as required by e-invoicing standards such as ZUGFeRD and Factur-X.
/// </summary>
public interface IPdfA3AttachmentWriter
{
    /// <summary>
    /// Embeds the provided XML data as a named attachment into the given
    /// <see cref="PdfDocument"/> and upgrades the document to PDF/A-3 conformance.
    /// </summary>
    /// <param name="pdfDocument">
    /// The target <see cref="PdfDocument"/> into which the XML attachment will be embedded.
    /// Must not be <see langword="null"/>.
    /// </param>
    /// <param name="xmlFileName">
    /// The file name to assign to the embedded XML attachment
    /// (e.g. <c>"factur-x.xml"</c> or <c>"ZUGFeRD-invoice.xml"</c>).
    /// </param>
    /// <param name="xmlBytes">
    /// The raw XML content to embed as a byte array.
    /// Must not be <see langword="null"/> or empty.
    /// </param>
    /// <param name="appOptions">
    /// The PDF/A-3 upgrade options providing conformance level, metadata,
    /// and attachment relationship settings required for the upgrade process.
    /// </param>
    /// <returns>
    /// An <see cref="OperationResult"/> indicating whether the attachment
    /// was successfully embedded and the document successfully upgraded to PDF/A-3.
    /// Contains error details on failure.
    /// </returns>
    OperationResult AddXmlAttachment(PdfDocument pdfDocument, string xmlFileName, byte[] xmlBytes, IUpgradeToPdfA3Options appOptions);
}
