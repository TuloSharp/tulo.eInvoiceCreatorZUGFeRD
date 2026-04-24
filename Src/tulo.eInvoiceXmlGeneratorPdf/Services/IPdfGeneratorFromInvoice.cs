namespace tulo.XMLeInvoiceToPdf.Services;

/// <summary>
/// Defines the contract for generating PDF documents from XML-based e-invoice data.
/// Implementations are typically registered as named plugins and resolved via
/// <see cref="IGetEinvoiceServiceByName"/> based on the required e-invoicing standard
/// (e.g. ZUGFeRD, Factur-X, XRechnung).
/// </summary>
public interface IPdfGeneratorFromInvoice
{
    /// <summary>
    /// Gets the unique name of this PDF generator plugin.
    /// Used for registration, lookup, and identification during logging and execution
    /// (e.g. <c>"ZUGFeRD"</c>, <c>"FacturX"</c>, <c>"XRechnung"</c>).
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Generates a PDF document from the provided XML invoice data
    /// and writes it directly to the specified file path.
    /// </summary>
    /// <param name="pdfPath">
    /// The full file system path where the generated PDF file will be saved.
    /// The target directory must exist and be writable.
    /// </param>
    /// <param name="xmlInvoiceFileName">
    /// The file name of the XML invoice
    /// (e.g. <c>"factur-x.xml"</c> or <c>"ZUGFeRD-invoice.xml"</c>).
    /// Used to correctly identify and reference the invoice source.
    /// </param>
    /// <param name="xmlInvoiceContent">
    /// The full XML invoice content as a string.
    /// Must be a valid, well-formed XML document conforming to the
    /// applicable e-invoicing standard.
    /// </param>
    /// <param name="hasToRenderHeader">
    /// Indicates whether the invoice header information (e.g. seller logo,
    /// document title, invoice number) should be rendered in the PDF output.
    /// </param>
    /// <returns>
    /// The full file system path of the generated PDF file.
    /// </returns>
    string GeneratePdfFile(string pdfPath, string xmlInvoiceFileName, string xmlInvoiceContent, bool hasToRenderHeader, string companyLogoPath);

    /// <summary>
    /// Generates a PDF document from the provided XML invoice data
    /// and returns it as an in-memory stream without writing to disk.
    /// Suitable for scenarios where the PDF is passed directly to a
    /// further processing step such as e-mail delivery or PDF/A upgrading.
    /// </summary>
    /// <param name="xmlInvoiceFileName">
    /// The file name of the XML invoice
    /// (e.g. <c>"factur-x.xml"</c> or <c>"ZUGFeRD-invoice.xml"</c>).
    /// Used to correctly identify and reference the invoice source.
    /// </param>
    /// <param name="xmlInvoiceContent">
    /// The full XML invoice content as a string.
    /// Must be a valid, well-formed XML document conforming to the
    /// applicable e-invoicing standard.
    /// </param>
    /// <param name="hasToRenderHeader">
    /// Indicates whether the invoice header information (e.g. seller logo,
    /// document title, invoice number) should be rendered in the PDF output.
    /// </param>
    /// <returns>
    /// A <see cref="MemoryStream"/> containing the generated PDF data.
    /// The caller is responsible for disposing the stream after use.
    /// </returns>
    MemoryStream GeneratePdfStream(string xmlInvoiceFileName, string xmlInvoiceContent, bool hasToRenderHeader, string companyLogoPath);
}

