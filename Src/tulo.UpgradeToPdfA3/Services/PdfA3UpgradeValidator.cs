using tulo.UpgradeToPdfA3.Options;
using tulo.UpgradeToPdfA3.Interfaces;
using tulo.UpgradeToPdfA3.ResultPattern;

namespace tulo.UpgradeToPdfA3.Services;

public sealed class PdfA3UpgradeValidator : IPdfA3UpgradeValidator
{
    public OperationResult Validate(string inputPdfAPath, string outputPdfA3Path, string xmlFileName, byte[] xmlBytes, IAppOptions appOptions)
    {
        if (string.IsNullOrWhiteSpace(inputPdfAPath))
            return OperationResult.Fail("Input PDF/A path is required.");

        if (!File.Exists(inputPdfAPath))
            return OperationResult.Fail($"Input PDF/A file not found: {inputPdfAPath}");

        if (string.IsNullOrWhiteSpace(outputPdfA3Path))
            return OperationResult.Fail("Output PDF/A-3 path is required.");

        if (string.IsNullOrWhiteSpace(xmlFileName))
            return OperationResult.Fail("XML file name is required.");

        if (xmlBytes is null || xmlBytes.Length == 0)
            return OperationResult.Fail("XML content is required.");

        if (appOptions is null)
            return OperationResult.Fail("PDF/A options are required.");

        if (appOptions.PdfA is null)
            return OperationResult.Fail("PdfA options are missing.");

        return OperationResult.Ok();
    }
}
