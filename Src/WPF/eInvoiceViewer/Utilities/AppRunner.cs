using Microsoft.Extensions.Logging;
using System.IO;
using tulo.CoreLib.Components.ResultPattern;

namespace tulo.eInvoice.eInvoiceViewer.Utilities;
public class AppRunner : IAppRunner
{
    private readonly IPdfGeneratorResolver _resolver;
    private readonly ILogger<AppRunner> _logger;

    public AppRunner(IPdfGeneratorResolver resolver, ILogger<AppRunner> logger)
    {
        _resolver = resolver;
        _logger = logger;
    }

    public Result<string> CreateAndSavePdf(string xmlInvoicePath, string xmlInvoiceContent, string outputPdfPath, string? customInfo = null)
    {
        if (string.IsNullOrWhiteSpace(xmlInvoicePath))
            return Result<string>.Failure(new Error("XML invoice path is empty.", ErrorCodes.InvalidInput));

        if (string.IsNullOrWhiteSpace(xmlInvoiceContent))
            return Result<string>.Failure(new Error("XML invoice content is empty.", ErrorCodes.InvalidInput));

        if (string.IsNullOrWhiteSpace(outputPdfPath))
            return Result<string>.Failure(new Error("Output PDF path is empty.", ErrorCodes.InvalidInput));

        var typeResult = InvoiceTypeDetector.DetectFromContent(xmlInvoiceContent);
        if (typeResult.IsFailure)
            return Result<string>.Failure(typeResult.Error);

        // Map Enum -> Name
        var generatorName = typeResult.Value == InvoiceType.CII ? "CII" : "UBL";

        var genResult = _resolver.ResolveByName(generatorName);
        if (genResult.IsFailure)
            return Result<string>.Failure(genResult.Error);

        var generator = genResult.Value;

        try
        {
            _logger.LogInformation("PDF generation started. Xml={Xml}, Generator={Gen}", xmlInvoicePath, generator!.Name);

            var pdfPath = generator.GeneratePdfFile(outputPdfPath, xmlInvoicePath, xmlInvoiceContent, true);

            if (string.IsNullOrWhiteSpace(pdfPath) || !File.Exists(pdfPath))
            {
                return Result<string>.Failure(new Error("PDF generation finished but output file was not created.", ErrorCodes.PdfGenerationFailed));
            }

            return Result<string>.Success(pdfPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PDF generation failed. Generator={Gen}", generator!.Name);
            return Result<string>.Failure(new Error( $"PDF generation failed: {ex.Message}", ErrorCodes.PdfGenerationFailed));
        }
    }

    public Result<MemoryStream> CreateAndGetPdfStream(string xmlInvoicePath, string xmlInvoiceContent, string? customInfo = null)
    {
        if (string.IsNullOrWhiteSpace(xmlInvoicePath))
            return Result<MemoryStream>.Failure(new Error("XML invoice path is empty.", ErrorCodes.InvalidInput));

        if (string.IsNullOrWhiteSpace(xmlInvoiceContent))
            return Result<MemoryStream>.Failure(new Error("XML invoice content is empty.", ErrorCodes.InvalidInput));

        var typeResult = InvoiceTypeDetector.DetectFromContent(xmlInvoiceContent);
        if (typeResult.IsFailure)
            return Result<MemoryStream>.Failure(typeResult.Error);

        var generatorName = typeResult.Value == InvoiceType.CII ? "CII" : "UBL";

        var genResult = _resolver.ResolveByName(generatorName);
        if (genResult.IsFailure)
            return Result<MemoryStream>.Failure(genResult.Error);

        var generator = genResult.Value;

        try
        {
            _logger.LogInformation("PDF stream generation started. Xml={Xml}, Generator={Gen}", xmlInvoicePath, generator!.Name);

            var stream = generator.GeneratePdfStream(xmlInvoicePath, xmlInvoiceContent, true);

            if (stream == null || stream.Length == 0)
            {
                return Result<MemoryStream>.Failure(new Error( "PDF stream generation returned an empty stream.", ErrorCodes.StreamGenerationFailed));
            }

            return Result<MemoryStream>.Success(stream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PDF stream generation failed. Generator={Gen}", generator!.Name);
            return Result<MemoryStream>.Failure(new Error($"PDF stream generation failed: {ex.Message}", ErrorCodes.StreamGenerationFailed));
        }
    }
}
