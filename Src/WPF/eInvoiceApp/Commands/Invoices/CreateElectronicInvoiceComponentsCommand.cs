using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PdfSharp.Pdf.IO;
using System.Diagnostics;
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
using tulo.XMLeInvoiceToPdf.Services;

namespace tulo.eInvoice.eInvoiceApp.Commands.Invoices;

public class CreateElectronicInvoiceComponentsCommand(InvoiceViewModel invoiceViewModel, ICollectorCollection collectorCollection) : AsyncBaseCommand
{
    #region Services – filled via CollectorCollection
    private readonly IInvoiceBuilderService _invoiceBuilderService = collectorCollection.GetService<IInvoiceBuilderService>();
    private readonly ICiiMapper _ciiMapper = collectorCollection.GetService<ICiiMapper>();
    private readonly IXmlCiiExporter _xmlCiiExporter = collectorCollection.GetService<IXmlCiiExporter>();
    private readonly ILogger<CreateElectronicInvoiceComponentsCommand> _logger = collectorCollection.GetService<ILoggerFactory>().CreateLogger<CreateElectronicInvoiceComponentsCommand>();
    private readonly IPdfGeneratorFromInvoice _pdfGeneratorFromInvoice = collectorCollection.GetService<IPdfGeneratorFromInvoice>();
    private readonly IPdfWatermarkService _watermarkService = collectorCollection.GetService<IPdfWatermarkService>();
    private readonly IOptions<AppOptions> _appOptions = collectorCollection.GetService<IOptions<AppOptions>>();
    private readonly IOptions<UpgradeToPdfA3Options> _upgradeToPdfA3Options = collectorCollection.GetService<IOptions<UpgradeToPdfA3Options>>();
    private readonly IToPdfAConverterService _toPdfAConverterService = collectorCollection.GetService<IToPdfAConverterService>();
    private readonly IToPdfA3UpgradeService _toPdfA3UpgradeService = collectorCollection.GetService<IToPdfA3UpgradeService>();
    #endregion

    protected override async Task ExecuteAsync(object parameter)
    {
        _logger.LogInformation("[{Cmd}] Execution started.", nameof(CreateElectronicInvoiceComponentsCommand));

        // ── 1. Parse parameters ───────────────────────────────────────────────
        var (window, isPreview, hasToCreate) = ParseParameters(parameter);
        _logger.LogDebug("[{Cmd}] Parameters parsed → isPreview={IsPreview}, hasToCreate={HasToCreate}, windowPresent={WindowPresent}.",
            nameof(CreateElectronicInvoiceComponentsCommand), isPreview, hasToCreate, window is not null);

        // ── 2. UI update (must run on UI thread) ──────────────────────────────
        await UpdateUiAsync(window, isPreview);

        // ── 3. Core pipeline ──────────────────────────────────────────────────
        try
        {
            await RunPipelineAsync(isPreview, hasToCreate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{Cmd}] Unhandled exception in pipeline.", nameof(CreateElectronicInvoiceComponentsCommand));
            SetError($"Failed to generate or render PDF: {ex.Message}", encode: true);
        }
    }

    #region Pipeline
    private async Task RunPipelineAsync(bool isPreview, bool hasToCreate)
    {
        _logger.LogInformation("[Pipeline] Building invoice model.");
        var invoice = await _invoiceBuilderService.BuildAsync(invoiceViewModel, default);

        _logger.LogDebug("[Pipeline] Mapping invoice to CII.");
        var cii = _ciiMapper.Map(invoice);

        _logger.LogDebug("[Pipeline] Exporting CII to XML.");
        string xmlInvoiceContent = _xmlCiiExporter.ToXml(cii);

        _logger.LogInformation("[Pipeline] Generating PDF stream.");
        var pdfStream = _pdfGeneratorFromInvoice.GeneratePdfStream(xmlInvoiceFileName: string.Empty, xmlInvoiceContent: xmlInvoiceContent, hasToRenderHeader: false);

        if (pdfStream is null)
        {
            _logger.LogWarning("[Pipeline] PDF generation returned null. Aborting. " + "isPreview={IsPreview}, hasToCreate={HasToCreate}, " + "InvoiceNumber={InvoiceNumber}.",
                isPreview, hasToCreate, invoiceViewModel.InvoiceNumber);
            SetError("PDF generation returned null result.");
            return;
        }

        // Copy to seekable MemoryStream
        using var rawPdfStream = pdfStream;
        var pdfMemoryStream = new MemoryStream();
        rawPdfStream.Position = 0;
        await rawPdfStream.CopyToAsync(pdfMemoryStream);

        if (pdfMemoryStream.Length == 0)
        {
            _logger.LogWarning("[Pipeline] PDF stream is empty after copy. Aborting. " + "isPreview={IsPreview}, hasToCreate={HasToCreate}, " + "InvoiceNumber={InvoiceNumber}.", isPreview, hasToCreate, invoiceViewModel.InvoiceNumber);
            pdfMemoryStream.Dispose();
            SetError("PDF stream is empty.");
            return;
        }

        _logger.LogDebug("[Pipeline] PDF stream ready ({Bytes} bytes).", pdfMemoryStream.Length);
        pdfMemoryStream.Position = 0;

        // ── 3a. Create artefacts on disk ──────────────────────────────────────
        if (hasToCreate)
        {
            _logger.LogInformation("[Pipeline] hasToCreate=true → starting file creation.");
            bool created = await CreateInvoiceFilesAsync(pdfMemoryStream, xmlInvoiceContent, isPreview);

            if (!created)
            {
                _logger.LogWarning("[Pipeline] File creation did not complete successfully. Preview will be skipped.");

                if (!isPreview)
                    pdfMemoryStream.Dispose();

                return;
            }

            _logger.LogInformation("[Pipeline] File creation finished successfully.");

            if (!isPreview)
            {
                pdfMemoryStream.Dispose();
                _logger.LogInformation("[Pipeline] Non-preview run completed. Returning.");
                return;
            }
        }

        // ── 3b. Preview rendering ─────────────────────────────────────────────
        if (isPreview)
        {
            _logger.LogInformation("[Pipeline] isPreview=true → applying watermark and rendering.");
            pdfMemoryStream.Position = 0;
            var watermarked = _watermarkService.AddWatermark(pdfMemoryStream, "PREVIEW");
            watermarked.Position = 0;

            invoiceViewModel.DocumentSource = HtmlPdfRenderer.CreateHtmlViewerFromPdf(watermarked);
            _logger.LogDebug("[Pipeline] DocumentSource updated with watermarked preview.");

            if (!ReferenceEquals(watermarked, pdfMemoryStream))
                pdfMemoryStream.Dispose();
        }
        else
        {
            _logger.LogInformation("[Pipeline] isPreview=false and hasToCreate=false → nothing to render or create. " +
                                   "InvoiceNumber={InvoiceNumber}.", invoiceViewModel.InvoiceNumber);
            pdfMemoryStream.Dispose();
        }

        _logger.LogInformation("[Pipeline] Execution completed successfully.");
    }

    #endregion

    #region File Creation
    /// <summary>
    /// Writes PDF + XML to disk, converts to PDF/A → PDF/A-3, optionally signs.
    /// Returns <c>true</c> on success, <c>false</c> if any step fails.
    /// </summary>
    private async Task<bool> CreateInvoiceFilesAsync(
        MemoryStream pdfMemoryStream,
        string xmlInvoiceContent,
        bool isPreview)
    {
        var invoiceFileName = invoiceViewModel.InvoiceNumber ?? "NotInvoiceNrPresent";
        var ct = CancellationToken.None;

        var configuredPath = _appOptions?.Value?.Archive?.OutputPath ?? string.Empty;
        var archiveRootPath = !string.IsNullOrWhiteSpace(configuredPath) && Path.IsPathFullyQualified(configuredPath)
            ? configuredPath
            : Path.GetTempPath();

        var safeFileName = MakeSafeFileName(invoiceFileName);

        var inputPdfPath = Path.Combine(archiveRootPath, $"{safeFileName}.pdf");
        var inputXmlPath = Path.Combine(archiveRootPath, $"{safeFileName}.xml");
        var intermediatePdfAPath = Path.Combine(archiveRootPath, $"{safeFileName}_PdfA.pdf");
        var outputPdfA3Path = Path.Combine(archiveRootPath, $"{safeFileName}_PdfA3.pdf");
        var outputSignedPath = Path.Combine(archiveRootPath, $"{safeFileName}_SignedPdfA3.pdf");

        _logger.LogDebug("[Create] Archive root: {ArchiveRoot}", archiveRootPath);
        _logger.LogDebug("[Create] File paths → pdf={Pdf}, xml={Xml}, pdfA={PdfA}, pdfA3={PdfA3}, signed={Signed}.",
            inputPdfPath, inputXmlPath, intermediatePdfAPath, outputPdfA3Path, outputSignedPath);

        // ── Write source files ────────────────────────────────────────────────
        _logger.LogInformation("[Create] Writing source PDF ({Bytes} bytes) → {Path}.",
            pdfMemoryStream.Length, inputPdfPath);
        await File.WriteAllBytesAsync(inputPdfPath, pdfMemoryStream.ToArray(), ct);

        _logger.LogInformation("[Create] Writing XML ({Chars} chars) → {Path}.",
            xmlInvoiceContent.Length, inputXmlPath);
        await File.WriteAllTextAsync(inputXmlPath, xmlInvoiceContent, ct);

        // ── Step 1: PDF → PDF/A ───────────────────────────────────────────────
        _logger.LogInformation("[Create] Step 1/3: Converting PDF to PDF/A → {Path}.", intermediatePdfAPath);

        using (var pdfDocument = PdfReader.Open(inputPdfPath, PdfDocumentOpenMode.Modify))
        {
            var pdfAResult = _toPdfAConverterService.ApplyPdfA(pdfDocument, _upgradeToPdfA3Options.Value);

            if (!pdfAResult.Success)
            {
                _logger.LogError("[Create] Step 1 FAILED (ApplyPdfA). " +
                                 "InvoiceNumber={InvoiceNumber}, Reason={Reason}.",
                    invoiceViewModel.InvoiceNumber, pdfAResult.Message);

                invoiceViewModel.StatusMessage = $"ApplyPdfA failed: {pdfAResult.Message}";
                invoiceViewModel.ResetSlideButton = !invoiceViewModel.ResetSlideButton;
                return false;
            }

            pdfDocument.Save(intermediatePdfAPath);
            _logger.LogDebug("[Create] Step 1 OK → {Path}.", intermediatePdfAPath);
        }

        // ── Step 2: PDF/A → PDF/A-3 + embedded XML ───────────────────────────
        _logger.LogInformation("[Create] Step 2/3: Upgrading to PDF/A-3 with embedded XML → {Path}.", outputPdfA3Path);

        byte[] xmlBytes = await File.ReadAllBytesAsync(inputXmlPath, ct);

        var pdfA3Result = _toPdfA3UpgradeService.UpgradeToPdfA3(
            inputPdfAPath: intermediatePdfAPath,
            outputPdfA3Path: outputPdfA3Path,
            xmlFileName: Path.GetFileName(inputXmlPath),
            xmlBytes: xmlBytes,
            appOptions: _upgradeToPdfA3Options.Value);

        if (!pdfA3Result.Success)
        {
            _logger.LogError("[Create] Step 2 FAILED (UpgradeToPdfA3). " +
                             "InvoiceNumber={InvoiceNumber}, Reason={Reason}.",
                invoiceViewModel.InvoiceNumber, pdfA3Result.Message);

            invoiceViewModel.StatusMessage = $"UpgradeToPdfA3 failed: {pdfA3Result.Message}";
            invoiceViewModel.ResetSlideButton = !invoiceViewModel.ResetSlideButton;
            return false;
        }

        _logger.LogDebug("[Create] Step 2 OK → {Path}.", outputPdfA3Path);

        // ── Reset UI slide button ─────────────────────────────────────────────
        invoiceViewModel.ResetSlideButton = !invoiceViewModel.ResetSlideButton;

        // ── Open with default viewer ──────────────────────────────────────────
        if (_appOptions!.Value.Archive.CanOpenPdfWithDefaultApp)
        {
            var fileToOpen = File.Exists(outputSignedPath) ? outputSignedPath
                           : File.Exists(outputPdfA3Path) ? outputPdfA3Path
                           : null;

            if (fileToOpen is not null)
            {
                _logger.LogInformation("[Create] Opening file with default viewer: {Path}.", fileToOpen);
                OpenWithDefaultPdfViewer(fileToOpen);
            }
            else
            {
                _logger.LogWarning("[Create] CanOpenPdfWithDefaultApp=true but no output file found to open. " +
                                   "Expected paths: signed={Signed}, pdfA3={PdfA3}.",
                    outputSignedPath, outputPdfA3Path);
            }
        }

        return true;
    }

    #endregion

    #region Helpers

    private async Task UpdateUiAsync(Window? window, bool isPreview)
    {
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            if (isPreview)
            {
                invoiceViewModel.IsPreviewEnabled = true;

                if (window is not null)
                {
                    const double baseWidth = 740;
                    const double extraWidth = 800;

                    invoiceViewModel.NormalWidthBeforePreview ??= baseWidth;
                    invoiceViewModel.NormalWidthBeforePreview =
                        Math.Max(baseWidth, window.Width);

                    var target = invoiceViewModel.NormalWidthBeforePreview.Value + extraWidth;
                    window.Width = Math.Min(target, SystemParameters.WorkArea.Width);
                    window.UpdateLayout();
                }
            }
            else
            {
                invoiceViewModel.IsPreviewEnabled = false;

                if (window is not null && invoiceViewModel.NormalWidthBeforePreview.HasValue)
                {
                    window.Width = invoiceViewModel.NormalWidthBeforePreview.Value;
                    window.UpdateLayout();
                }
            }
        }, System.Windows.Threading.DispatcherPriority.Render);
    }

    private void SetError(string message, bool encode = false)
    {
        var safe = encode ? System.Net.WebUtility.HtmlEncode(message) : message;
        invoiceViewModel.DocumentSource = $"<html><body><h1>{safe}</h1></body></html>";
    }

    #endregion

    #region Parameter parsing

    private static (Window? window, bool isPreview, bool hasToCreate) ParseParameters(object parameter)
    {
        Window? window = null;
        bool isPreview = false;
        bool hasToCreate = false;

        if (parameter is object[] arr)
        {
            if (arr.Length > 0 && arr[0] is Window w) window = w;

            if (arr.Length > 1)
                isPreview = arr[1] switch
                {
                    bool b => b,
                    string s when bool.TryParse(s, out var p) => p,
                    _ => false
                };

            if (arr.Length > 2)
                hasToCreate = arr[2] switch
                {
                    bool b => b,
                    string s when bool.TryParse(s, out var p) => p,
                    _ => false
                };
        }

        return (window, isPreview, hasToCreate);
    }

    #endregion

    #region Utilities

    private static string MakeSafeFileName(string value)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            value = value.Replace(c, '_');
        return value.Trim();
    }

    private static void OpenWithDefaultPdfViewer(string pdfPath) =>
        Process.Start(new ProcessStartInfo(pdfPath) { UseShellExecute = true });

    #endregion
}
