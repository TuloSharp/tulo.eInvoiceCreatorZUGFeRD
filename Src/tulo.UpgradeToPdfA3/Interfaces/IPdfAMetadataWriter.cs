using PdfSharp.Pdf;
using tulo.UpgradeToPdfA3.Options;
using tulo.UpgradeToPdfA3.ResultPattern;

namespace tulo.UpgradeToPdfA3.Interfaces;

public interface IPdfAMetadataWriter
{
    OperationResult WritePdfA(PdfDocument pdfDocument, IUpgradeToPdfA3Options appOptions);
    OperationResult WritePdfA3(PdfDocument pdfDocument, string xmlFileName, IUpgradeToPdfA3Options appOptions);
}
