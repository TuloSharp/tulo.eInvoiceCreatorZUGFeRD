using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.IO;
using System.Windows;
using tulo.CommonMVVM.Collector;
using tulo.CommonMVVM.Commands;
using tulo.CoreLib.PDFs;
using tulo.eInvoice.eInvoiceApp.Options;
using tulo.eInvoice.eInvoiceApp.Services;
using tulo.eInvoice.eInvoiceApp.ViewModels.Invoices;
using tulo.eInvoiceXmlGeneratorCii.Mappers;
using tulo.eInvoiceXmlGeneratorCii.Services;
using tulo.UpgradeToPdfA3.Interfaces;
using tulo.UpgradeToPdfA3.Options;
using tulo.UpgradeToPdfA3.ResultPattern;
using tulo.XMLeInvoiceToPdf.Services;

namespace tulo.eInvoice.eInvoiceApp.Commands.Invoices;

public class CreateElectronicInvoiceComponentsCommand(InvoiceViewModel invoiceViewModel, ICollectorCollection collectorCollection) : AsyncBaseCommand
{
    #region Get filled via CollectorCollection
    private readonly IInvoiceBuilderService _invoiceBuilderService = collectorCollection.GetService<IInvoiceBuilderService>();
    private readonly ICiiMapper _ciiMapper = collectorCollection.GetService<ICiiMapper>();
    private readonly IXmlCiiExporter _xmlCiiExporter = collectorCollection.GetService<IXmlCiiExporter>();
    private readonly ILogger<CreateElectronicInvoiceComponentsCommand> _logger = collectorCollection.GetService<ILoggerFactory>().CreateLogger<CreateElectronicInvoiceComponentsCommand>();
    private readonly IPdfGeneratorFromInvoice _pdfGeneratorFromInvoice = collectorCollection.GetService<IPdfGeneratorFromInvoice>();
    private readonly IPdfWatermarkService _watermarckService = collectorCollection.GetService<IPdfWatermarkService>();
    private readonly IOptions<AppOptions> _appOptions = collectorCollection.GetService<IOptions<AppOptions>>();
    private readonly IOptions<UpgradeToPdfA3Options> _upgradeToPdfA3Options = collectorCollection.GetService<IOptions<UpgradeToPdfA3Options>>();
    private readonly IToPdfAConverterService _toPdfAConverterService = collectorCollection.GetService<IToPdfAConverterService>();
    private readonly IToPdfA3UpgradeService _toPdfA3UpgradeService = collectorCollection.GetService<IToPdfA3UpgradeService>();
    #endregion

    protected override async Task ExecuteAsync(object parameter)
    {
        _logger.LogInformation($"{nameof(CreateElectronicInvoiceComponentsCommand)} start execution");

        #region Parse Command parameters
        Window? window = null;
        bool isPreview = false;
        bool hasToCreate = false;

        if (parameter is object[] arr)
        {
            if (arr.Length > 0 && arr[0] is Window w)
                window = w;

            if (arr.Length > 1)
            {
                isPreview = arr[1] switch
                {
                    bool b => b,
                    string s when bool.TryParse(s, out var p) => p,
                    _ => false
                };
            }
            if (arr.Length > 2)
            {
                hasToCreate = arr[2] switch
                {
                    bool b => b,
                    string s when bool.TryParse(s, out var p) => p,
                    _ => false
                };
            }
        }
        #endregion

        #region UI UPDATE FIRST (must be on UI thread)
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            if (isPreview)
            {
                invoiceViewModel.IsPreviewEnabled = true;

                if (window != null)
                {
                    const double baseWidth = 740;
                    const double extraWidth = 800;

                    invoiceViewModel.NormalWidthBeforePreview ??= baseWidth;

                    // remember current "normal" width before expanding (only when turning ON)
                    invoiceViewModel.NormalWidthBeforePreview = Math.Max(baseWidth, window.Width);

                    var target = invoiceViewModel.NormalWidthBeforePreview.Value + extraWidth;

                    var max = SystemParameters.WorkArea.Width;
                    window.Width = Math.Min(target, max);

                    window.UpdateLayout();
                }
            }
            else
            {
                invoiceViewModel.IsPreviewEnabled = false;

                // ✅ shrink back when turning OFF
                if (window != null && invoiceViewModel.NormalWidthBeforePreview.HasValue)
                {
                    window.Width = invoiceViewModel.NormalWidthBeforePreview.Value;
                    window.UpdateLayout();
                }
            }
        }, System.Windows.Threading.DispatcherPriority.Render);
        #endregion

        #region Create Invocie Pdf-A3
        try
        {
            var invoice = await _invoiceBuilderService.BuildAsync(invoiceViewModel, default);

            var cii = _ciiMapper.Map(invoice);
            string xmlInvoiceContent = _xmlCiiExporter.ToXml(cii);

            var xmlInvoiceFileName = string.Empty;

            // Generate PDF stream
            var pdfStream = _pdfGeneratorFromInvoice.GeneratePdfStream(xmlInvoiceFileName, xmlInvoiceContent, hasToRenderHeader: false);

            if (pdfStream is null)
            {
                invoiceViewModel.DocumentSource = BuildErrorHtml("PDF generation returned null result.");
                return;
            }

            // Copy to MemoryStream to ensure it is seekable and stable for downstream processing
            MemoryStream pdfMemoryStream = new MemoryStream();
            using (pdfStream)
            {
                pdfStream.Position = 0;
                await pdfStream.CopyToAsync(pdfMemoryStream);
            }

            if (pdfMemoryStream.Length == 0)
            {
                pdfMemoryStream.Dispose();
                invoiceViewModel.DocumentSource = BuildErrorHtml("PDF stream is empty.");
                return;
            }

            pdfMemoryStream.Position = 0;

            // Apply watermark ONLY in preview mode (result is also a MemoryStream)
            MemoryStream streamToRender = pdfMemoryStream;

            // PREVIEW MODE: Watermark + Render ONLY (no create)
            if (isPreview)
            {
                var previewInput = new MemoryStream(pdfMemoryStream.ToArray());
                pdfMemoryStream.Position = 0;

                var watermarkedPdf = _watermarckService.AddWatermark(pdfMemoryStream, "PREVIEW");

                watermarkedPdf.Position = 0;
                streamToRender = watermarkedPdf;
            }

            if (hasToCreate)
            {
                var invoiceFileName = invoiceViewModel.InvoiceNumber ?? "NotInvoiceNrPresent";
                CancellationToken ct = default;

                var configuredPath = _appOptions?.Value?.Archive?.OutputPath ?? string.Empty;
                //var configuredPath = string.Empty;
                var archiveRootPath = !string.IsNullOrWhiteSpace(configuredPath) && Path.IsPathFullyQualified(configuredPath) ? configuredPath : Path.GetTempPath();

                string? inputPdfPath = null;
                string? inputXmlPath = null;
                string? outputPdfPath = null;

                var safeInvoiceFileName = MakeSafeFileName(invoiceFileName);

                inputPdfPath = Path.Combine(archiveRootPath, $"{safeInvoiceFileName}.pdf");
                inputXmlPath = Path.Combine(archiveRootPath, $"{safeInvoiceFileName}.xml");
                outputPdfPath = Path.Combine(archiveRootPath, $"{safeInvoiceFileName}_PdfA3.pdf");

                await File.WriteAllBytesAsync(inputPdfPath, pdfMemoryStream.ToArray(), ct);
                await File.WriteAllTextAsync(inputXmlPath, xmlInvoiceContent, ct);

                // Step 1: Convert the generated PDF to PDF/A
                string intermediatePdfAPath = Path.Combine(archiveRootPath, $"{safeInvoiceFileName}_PdfA.pdf");

                using (PdfDocument pdfDocument = PdfReader.Open(inputPdfPath, PdfDocumentOpenMode.Modify))
                {
                    OperationResult pdfAResult = _toPdfAConverterService.ApplyPdfA(pdfDocument, _upgradeToPdfA3Options.Value);

                    if (!pdfAResult.Success)
                    {
                        invoiceViewModel.ResetSlideButton = !invoiceViewModel.ResetSlideButton;

                        if (!isPreview)
                            pdfMemoryStream.Dispose();

                        invoiceViewModel.StatusMessage = $"ApplyPdfA failed: {pdfAResult.Message}";
                        return;
                    }

                    pdfDocument.Save(intermediatePdfAPath);
                }

                // Step 2: Upgrade PDF/A to PDF/A-3 with embedded XML
                byte[] xmlBytes = await File.ReadAllBytesAsync(inputXmlPath, ct);

                OperationResult pdfA3Result = _toPdfA3UpgradeService.UpgradeToPdfA3(inputPdfAPath: intermediatePdfAPath, outputPdfA3Path: outputPdfPath, xmlFileName: Path.GetFileName(inputXmlPath), xmlBytes: xmlBytes, appOptions: _upgradeToPdfA3Options.Value);

                if (!pdfA3Result.Success)
                {
                    invoiceViewModel.ResetSlideButton = !invoiceViewModel.ResetSlideButton;

                    if (!isPreview)
                        pdfMemoryStream.Dispose();

                    invoiceViewModel.StatusMessage = $"UpgradeToPdfA3 failed: {pdfA3Result.Message}";
                    return;
                }

                invoiceViewModel.ResetSlideButton = !invoiceViewModel.ResetSlideButton;

                if (!isPreview)
                {
                    pdfMemoryStream.Dispose();
                    return;
                }
            }

            // Render ONLY when preview is requested
            if (isPreview)
            {
                streamToRender.Position = 0;
                invoiceViewModel.DocumentSource = HtmlPdfRenderer.CreateHtmlViewerFromPdf(streamToRender);
            }

            // If we rendered watermarked stream, pdfMemoryStream is still alive -> dispose it too
            if (!ReferenceEquals(streamToRender, pdfMemoryStream))
                pdfMemoryStream.Dispose();
        }
        catch (Exception ex)
        {
            invoiceViewModel.DocumentSource = BuildErrorHtml($"Failed to generate or render PDF: {ex.Message}", encode: true);
        }
        #endregion
    }

    #region Utilities
    static string BuildErrorHtml(string message, bool encode = true)
    {
        var safe = encode ? System.Net.WebUtility.HtmlEncode(message) : message;
        return $"<html><body><h1>{safe}</h1></body></html>";
    }

    static string MakeSafeFileName(string value)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            value = value.Replace(c, '_');
        return value.Trim();
    }
    #endregion
}
