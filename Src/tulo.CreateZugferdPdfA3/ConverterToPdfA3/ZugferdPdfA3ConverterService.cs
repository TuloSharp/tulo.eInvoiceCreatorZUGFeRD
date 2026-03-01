using s2industries.ZUGFeRD;
using s2industries.ZUGFeRD.PDF;

namespace tulo.CreateZugferdPdfA3.ConverterToPdfA3;

public sealed class ZugferdPdfA3ConverterService : IZugferdPdfA3ConverterService
{
    public async Task<Result<string>> ConvertAsync(string inputPdfPath, string inputXmlPath, string outputPdfPath, ZUGFeRDVersion zugferdVersion, Profile profile, ZUGFeRDFormats format = ZUGFeRDFormats.CII)
    {
        // 1) Validation
        if (string.IsNullOrWhiteSpace(inputPdfPath))
            return Result<string>.Failure(new Error("inputPdfPath is empty.", "ZUGF-ARG-001"));

        if (string.IsNullOrWhiteSpace(inputXmlPath))
            return Result<string>.Failure(new Error("inputXmlPath is empty.", "ZUGF-ARG-002"));

        if (string.IsNullOrWhiteSpace(outputPdfPath))
            return Result<string>.Failure(new Error("outputPdfPath is empty.", "ZUGF-ARG-003"));

        if (!File.Exists(inputPdfPath))
            return Result<string>.Failure(new Error($"PDF not found: {inputPdfPath}", "ZUGF-IO-001"));

        if (!File.Exists(inputXmlPath))
            return Result<string>.Failure(new Error($"XML not found: {inputXmlPath}", "ZUGF-IO-002"));

        try
        {
            var outDir = Path.GetDirectoryName(outputPdfPath);
            if (!string.IsNullOrWhiteSpace(outDir))
                Directory.CreateDirectory(outDir);
        }
        catch (Exception ex)
        {
            return Result<string>.Failure(new Error($"Output folder could not be created: {ex.Message}", "ZUGF-IO-003"));
        }

        // 2) PDF + XML -> PDF/A-3 (ZUGFeRD)
        try
        {
            InvoiceDescriptor descriptor;

            try
            {
                descriptor = InvoiceDescriptor.Load(inputXmlPath);
            }
            catch (Exception ex)
            {
                return Result<string>.Failure(new Error($"XML could not be read as ZUGFeRD/Factur-X: {ex.Message}", "ZUGF-XML-001"));
            }

            try
            {
                await InvoicePdfProcessor.SaveToPdfAsync(outputPdfPath, zugferdVersion, profile, format, inputPdfPath, descriptor);

                return Result<string>.Success(outputPdfPath);
            }
            catch (Exception ex)
            {
                return Result<string>.Failure(new Error($"PDF/A-3 could not be generated: {ex.Message}", "ZUGF-PDF-001"));
            }
        }
        catch (Exception ex)
        {
            return Result<string>.Failure(new Error($"Unexpected error: {ex.Message}", "ZUGF-UNEXPECTED"));
        }
    }
}
