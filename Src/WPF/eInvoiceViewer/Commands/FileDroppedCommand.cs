using System.IO;
using System.Net;
using System.Text;
using tulo.CommonMVVM.Collector;
using tulo.CommonMVVM.Commands;
using tulo.eInvoice.eInvoiceViewer.Utilities;
using tulo.eInvoice.eInvoiceViewer.ViewModels;
using tulo.UiUtilitiesLib.PDFs;

namespace tulo.eInvoice.eInvoiceViewer.Commands;
public class FileDroppedCommand(ContentXmlToPdfViewerViewModel contentXmlToPdfViewerViewModel, ICollectorCollection collectorCollection) : BaseCommand
{
    private readonly ICollectorCollection _collectorCollection = collectorCollection;
    private readonly ContentXmlToPdfViewerViewModel _contentXmlToPdfViewerViewModel = contentXmlToPdfViewerViewModel;

    public override bool CanExecute(object? parameter)
    {
        var path = parameter as string;
        if (string.IsNullOrWhiteSpace(path)) return false;
        if (!File.Exists(path)) return false;

        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext == ".pdf" || ext == ".xml";
    }

    public override void Execute(object parameter)
    {
        IAppRunner appRunner = _collectorCollection.GetService<IAppRunner>();
        var xmlInvoicePath = parameter as string;
        if (string.IsNullOrWhiteSpace(xmlInvoicePath))
        {
            _contentXmlToPdfViewerViewModel.StatusMessage = "No file path transferred (drop failed).";
            return;
        }

        if (!File.Exists(xmlInvoicePath))
        {
            _contentXmlToPdfViewerViewModel.StatusMessage = $"File not found: {xmlInvoicePath}";
            return;
        }

        var ext = Path.GetExtension(xmlInvoicePath).ToLowerInvariant();
        var xmlInvoiceFileName = Path.GetFileName(xmlInvoicePath);
        string xmlInvoiceContent = File.ReadAllText(xmlInvoicePath, Encoding.UTF8);

        try
        {
            // -------------------- XML -> PDF -> HTML Viewer --------------------
            if (ext == ".xml")
            {
                var pdfResult = appRunner.CreateAndGetPdfStream(xmlInvoiceFileName, xmlInvoiceContent, string.Empty);

                if (pdfResult == null)
                {
                    _contentXmlToPdfViewerViewModel.DocumentSource = "<html><body><h1>PDF generation returned null result.</h1></body></html>";
                    return;
                }

                if (pdfResult.IsFailure)
                {
                    var msg = WebUtility.HtmlEncode(pdfResult.Error.Message);
                    _contentXmlToPdfViewerViewModel.DocumentSource = $"<html><body><h1>The selected file is not an eInvoice: {msg}</h1></body></html>";
                    return;
                }

                var pdfStream = pdfResult.Value;
                if (pdfStream == null || pdfStream.Length == 0)
                {
                    _contentXmlToPdfViewerViewModel.DocumentSource = "<html><body><h1>PDF stream is empty.</h1></body></html>";
                    return;
                }

                pdfStream.Position = 0;

                // Important: HTML is displayed in WebView2 -> we render HTML from PDF stream
                _contentXmlToPdfViewerViewModel.DocumentSource = HtmlPdfRenderer.CreateHtmlViewerFromPdf(pdfStream);
                return;
            }

            // --------------------  Display PDF directly (as HTML viewer) --------------------
            if (ext == ".pdf")
            {
                byte[] bytes = File.ReadAllBytes(xmlInvoicePath);
                if (bytes.Length == 0)
                {
                    _contentXmlToPdfViewerViewModel.DocumentSource = "<html><body><h1>PDF file is empty.</h1></body></html>";
                    return;
                }

                using var ms = new MemoryStream(bytes, writable: false);
                _contentXmlToPdfViewerViewModel.DocumentSource = HtmlPdfRenderer.CreateHtmlViewerFromPdf(ms);

                return;
            }

            _contentXmlToPdfViewerViewModel.DocumentSource = $"<html><body><h1>Unsupported file type:</h1><p>{WebUtility.HtmlEncode(ext)}</p></body></html>";
        }
        catch (Exception ex)
        {
            _contentXmlToPdfViewerViewModel.StatusMessage = $"Errors during processing: {ex.Message}";
        }
    }
}
