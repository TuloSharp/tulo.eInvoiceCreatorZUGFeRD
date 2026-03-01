namespace tulo.XMLeInvoiceToPdf.Services;

public interface IPdfGeneratorFromInvoice
{
    /// <summary>
    /// Generates the PDF file from the specified XML invoice file and saves it to the given path.
    /// </summary>
    /// <param name="pdfPath">The path to save the generated PDF file.</param>
    /// <param name="xmlInvoiceFileName">The XML invoice file name.</param>
    /// <param name="xmlInvoiceContent">The XML invoice content.</param>
    /// <param name="hasToRenderHeader">Header information can display in the PDF.</param>
    /// <returns>return the path, where the file is created.</returns>
    string GeneratePdfFile(string pdfPath, string xmlInvoiceFileName, string xmlInvoiceContent, bool hasToRenderHeader);

    /// <summary>
    /// Generates a PDF as a memory stream based on the provided XML data and custom information.
    /// </summary>
    /// <param name="xmlInvoiceFileName">The name of the XML file containing the data to populate the PDF.</param>
    /// <param name="xmlInvoiceContent">The XML invoice content.</param>
    /// <param name="hasToRenderHeader">Header information can display in the PDF.</param>
    /// <returns>A <see cref="MemoryStream"/> containing the generated PDF data.</returns>
    MemoryStream GeneratePdfStream(string xmlInvoiceFileName, string xmlInvoiceContent,  bool hasToRenderHeader);

    /// <summary>
    /// Gets the name of the plugin.
    /// This is typically used for logging and identifying the plugin during execution.
    /// </summary>
    string Name { get; }
}
