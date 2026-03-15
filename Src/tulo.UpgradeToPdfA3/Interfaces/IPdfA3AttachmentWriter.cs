using PdfSharp.Pdf;
using tulo.UpgradeToPdfA3.Options;
using tulo.UpgradeToPdfA3.ResultPattern;

namespace tulo.UpgradeToPdfA3.Interfaces;

public interface IPdfA3AttachmentWriter
{
    OperationResult AddXmlAttachment(PdfDocument pdfDocument, string xmlFileName, byte[] xmlBytes, IAppOptions appOptions);
}
