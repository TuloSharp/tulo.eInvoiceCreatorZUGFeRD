using PdfSharp.Pdf;
using tulo.UpgradeToPdfA3.Options;
using tulo.UpgradeToPdfA3.Interfaces;
using tulo.UpgradeToPdfA3.ResultPattern;

namespace tulo.UpgradeToPdfA3.Services;

public sealed class PdfAConverterValidator : IPdfAConverterValidator
{
    public OperationResult Validate(PdfDocument pdfDocument, IAppOptions appOptions)
    {
        if (appOptions is null)
            return OperationResult.Fail("App options are missing.");

        if (pdfDocument is null)
            return OperationResult.Fail("PDF document is missing.");

        if (appOptions.PdfA is null)
            return OperationResult.Fail("PdfA options are missing.");

        if (string.IsNullOrWhiteSpace(appOptions.PdfA.IccProfilePath))
            return OperationResult.Fail("ICC profile path is missing.");

        if (!File.Exists(appOptions.PdfA.IccProfilePath))
            return OperationResult.Fail($"ICC profile not found: {appOptions.PdfA.IccProfilePath}");

        if (appOptions.PdfA.Part < 1 || appOptions.PdfA.Part > 3)
            return OperationResult.Fail("PDF/A part must be 1, 2, or 3.");

        if (string.IsNullOrWhiteSpace(appOptions.PdfA.Conformance))
            return OperationResult.Fail("PDF/A conformance is missing.");

        return OperationResult.Ok();
    }
}
