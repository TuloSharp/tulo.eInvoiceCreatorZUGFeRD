using PdfSharp.Pdf;
using tulo.UpgradeToPdfA3.Options;
using tulo.UpgradeToPdfA3.ResultPattern;

namespace tulo.UpgradeToPdfA3.Interfaces;

/// <summary>
/// Provides functionality to write XMP metadata into a <see cref="PdfDocument"/>
/// to declare PDF/A and PDF/A-3 conformance, as required by the ISO 19005 standard.
/// XMP metadata is mandatory for a document to be recognized as PDF/A-compliant
/// by validators and downstream processing tools.
/// </summary>
public interface IPdfAMetadataWriter
{
    /// <summary>
    /// Writes the XMP metadata required to declare general PDF/A conformance
    /// into the provided <see cref="PdfDocument"/>.
    /// </summary>
    /// <param name="pdfDocument">
    /// The target <see cref="PdfDocument"/> into which the PDF/A
    /// XMP metadata will be written. Must not be <see langword="null"/>.
    /// </param>
    /// <param name="appOptions">
    /// The PDF/A-3 upgrade options providing conformance level, part,
    /// and amendment values to be declared in the XMP metadata.
    /// Must not be <see langword="null"/>.
    /// </param>
    /// <returns>
    /// An <see cref="OperationResult"/> indicating whether the PDF/A
    /// XMP metadata was written successfully. Contains specific error
    /// details on failure.
    /// </returns>
    OperationResult WritePdfA(PdfDocument pdfDocument, IUpgradeToPdfA3Options appOptions);

    /// <summary>
    /// Writes the XMP metadata required to declare PDF/A-3 conformance
    /// with an embedded XML attachment into the provided <see cref="PdfDocument"/>.
    /// Extends the base PDF/A metadata with attachment-specific entries
    /// such as the XML file name and its relationship type,
    /// as required by standards such as ZUGFeRD and Factur-X.
    /// </summary>
    /// <param name="pdfDocument">
    /// The target <see cref="PdfDocument"/> into which the PDF/A-3
    /// XMP metadata will be written. Must not be <see langword="null"/>.
    /// </param>
    /// <param name="xmlFileName">
    /// The file name of the embedded XML attachment to declare in the metadata
    /// (e.g. <c>"factur-x.xml"</c> or <c>"ZUGFeRD-invoice.xml"</c>).
    /// Must not be <see langword="null"/> or empty.
    /// </param>
    /// <param name="appOptions">
    /// The PDF/A-3 upgrade options providing conformance level, attachment
    /// relationship type, and additional metadata values.
    /// Must not be <see langword="null"/>.
    /// </param>
    /// <returns>
    /// An <see cref="OperationResult"/> indicating whether the PDF/A-3
    /// XMP metadata was written successfully. Contains specific error
    /// details on failure.
    /// </returns>
    OperationResult WritePdfA3(PdfDocument pdfDocument, string xmlFileName, IUpgradeToPdfA3Options appOptions);
}
