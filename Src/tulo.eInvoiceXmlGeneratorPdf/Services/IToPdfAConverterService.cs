using PdfSharp.Pdf;
using tulo.XMLeInvoiceToPdf.Options;
using tulo.XMLeInvoiceToPdf.ResultPattern;

namespace tulo.XMLeInvoiceToPdf.Services;
public interface IToPdfAConverterService
{
    OperationResult ApplyPdfA(PdfDocument pdfDocument, IAppOptions options);
}
