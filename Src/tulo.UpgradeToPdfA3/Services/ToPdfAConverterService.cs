using PdfSharp.Pdf;
using tulo.UpgradeToPdfA3.Options;
using tulo.UpgradeToPdfA3.Interfaces;
using tulo.UpgradeToPdfA3.ResultPattern;

namespace tulo.UpgradeToPdfA3.Services;

public sealed class ToPdfAConverterService : IToPdfAConverterService
{
    private readonly IPdfAConverterValidator _validator;
    private readonly IPdfADocumentInfoWriter _documentInfoWriter;
    private readonly IPdfALanguageWriter _languageWriter;
    private readonly IPdfAMetadataWriter _metadataWriter;
    private readonly IPdfAOutputIntentWriter _outputIntentWriter;

    public ToPdfAConverterService(IPdfAConverterValidator validator, IPdfADocumentInfoWriter documentInfoWriter, IPdfALanguageWriter languageWriter, IPdfAMetadataWriter metadataWriter, IPdfAOutputIntentWriter outputIntentWriter)
    {
        _validator = validator;
        _documentInfoWriter = documentInfoWriter;
        _languageWriter = languageWriter;
        _metadataWriter = metadataWriter;
        _outputIntentWriter = outputIntentWriter;
    }

    public OperationResult ApplyPdfA(PdfDocument pdfDocument, IUpgradeToPdfA3Options appOptions)
    {
        OperationResult validationResult = _validator.Validate(pdfDocument, appOptions);
        if (!validationResult.Success)
            return validationResult;

        try
        {
            OperationResult infoResult = _documentInfoWriter.Write(pdfDocument, appOptions);
            if (!infoResult.Success)
                return infoResult;

            OperationResult languageResult = _languageWriter.Write(pdfDocument, appOptions.PdfA3.Language);
            if (!languageResult.Success)
                return languageResult;

            OperationResult metadataResult = _metadataWriter.WritePdfA(pdfDocument, appOptions);
            if (!metadataResult.Success)
                return metadataResult;

            OperationResult outputIntentResult = _outputIntentWriter.Write(pdfDocument, appOptions);
            if (!outputIntentResult.Success)
                return outputIntentResult;

            return OperationResult.Ok("PDF/A metadata applied successfully.");
        }
        catch (Exception ex)
        {
            return OperationResult.Fail($"Failed to apply PDF/A: {ex.Message}");
        }
    }
}
