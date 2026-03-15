using PdfSharp.Pdf;
using tulo.UpgradeToPdfA3.Options;
using tulo.UpgradeToPdfA3.Interfaces;
using tulo.UpgradeToPdfA3.ResultPattern;

namespace tulo.UpgradeToPdfA3.Services;

public sealed class PdfADocumentInfoWriter : IPdfADocumentInfoWriter
{
    public OperationResult Write(PdfDocument pdfDocument, IAppOptions appOptions)
    {
        try
        {
            DateTime now = DateTime.UtcNow;

            pdfDocument.Info.Title = appOptions.PdfA.Title ?? string.Empty;
            pdfDocument.Info.Subject = appOptions.PdfA.Description ?? string.Empty;
            pdfDocument.Info.Author = appOptions.PdfA.Author ?? string.Empty;
            pdfDocument.Info.Creator = appOptions.PdfA.Creator ?? string.Empty;
            pdfDocument.Info.CreationDate = now;
            pdfDocument.Info.ModificationDate = now;

            return OperationResult.Ok();
        }
        catch (Exception ex)
        {
            return OperationResult.Fail($"Failed to apply PDF info: {ex.Message}");
        }
    }
}
