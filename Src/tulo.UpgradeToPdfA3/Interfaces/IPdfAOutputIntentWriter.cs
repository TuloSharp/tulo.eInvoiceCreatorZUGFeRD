using PdfSharp.Pdf;
using tulo.UpgradeToPdfA3.Options;
using tulo.UpgradeToPdfA3.ResultPattern;

namespace tulo.UpgradeToPdfA3.Interfaces;

public interface IPdfAOutputIntentWriter
{
    OperationResult Write(PdfDocument pdfDocument, IAppOptions appOptions);
}
