using System.IO;
using tulo.CoreLib.Components.ResultPattern;

namespace tulo.eInvoice.eInvoiceViewer.Utilities;
public interface IAppRunner
{
    /// <summary>
    /// Creates a PDF from the given XML invoice content and saves it to <paramref name="outputPdfPath"/>.
    /// </summary>
    /// <param name="xmlInvoiceFileName">
    /// File name of the XML invoice (file name only, no full path). Used for display/logging and/or format detection.
    /// </param>
    /// <param name="xmlInvoiceContent">
    /// The XML invoice content as a string.
    /// </param>
    /// <param name="outputPdfPath">
    /// Full file path (including file name) where the generated PDF should be saved.
    /// </param>
    /// <param name="customInfo">
    /// Optional custom information to be embedded or displayed in the generated PDF (implementation-specific).
    /// </param>
    /// <returns>
    /// A successful result containing the created PDF file path, or a failure result containing an error.
    /// </returns>
    Result<string> CreateAndSavePdf(string xmlInvoiceFileName, string xmlInvoiceContent, string outputPdfPath, string? customInfo = null);

    /// <summary>
    /// Creates a PDF from the given XML invoice content and returns it as an in-memory stream.
    /// </summary>
    /// <param name="xmlInvoiceFileName">
    /// File name of the XML invoice (file name only, no full path). Used for display/logging and/or format detection.
    /// </param>
    /// <param name="xmlInvoiceContent">
    /// The XML invoice content as a string.
    /// </param>
    /// <param name="customInfo">
    /// Optional custom information to be embedded or displayed in the generated PDF (implementation-specific).
    /// </param>
    /// <returns>
    /// A successful result containing a <see cref="MemoryStream"/> with the generated PDF data,
    /// or a failure result containing an error.
    /// </returns>
    Result<MemoryStream> CreateAndGetPdfStream(string xmlInvoiceFileName, string xmlInvoiceContent, string? customInfo = null);

}
