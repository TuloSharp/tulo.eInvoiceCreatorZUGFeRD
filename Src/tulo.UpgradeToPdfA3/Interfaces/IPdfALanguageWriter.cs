using PdfSharp.Pdf;
using tulo.UpgradeToPdfA3.ResultPattern;

namespace tulo.UpgradeToPdfA3.Interfaces;

public interface IPdfALanguageWriter
{
    OperationResult Write(PdfDocument pdfDocument, string? language);
}
