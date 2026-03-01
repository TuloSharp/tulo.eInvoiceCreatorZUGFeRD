using System.IO;
using System.Text;
using tulo.CommonMVVM.Collector;
using tulo.CommonMVVM.Commands;
using tulo.eInvoice.eInvoiceViewer.Utilities;
using tulo.eInvoice.eInvoiceViewer.ViewModels;
using tulo.UiUtilitiesLib.PDFs;

namespace tulo.eInvoice.eInvoiceViewer.Commands;
public class XmlToPdfContentCommand(ContentXmlToPdfViewerViewModel contentXmlToPdfViewerViewModel, ICollectorCollection collectorCollection) : BaseCommand
{
    private readonly ICollectorCollection _collectorCollection = collectorCollection;
    private readonly ContentXmlToPdfViewerViewModel _contentXmlToPdfViewerViewModel = contentXmlToPdfViewerViewModel;

    public override void Execute(object parameter)
    {
        IAppRunner appRunner = _collectorCollection.GetService<IAppRunner>();
        string xmlInvoicePath = parameter as string ?? _contentXmlToPdfViewerViewModel.DocumentSource;

        if (File.Exists(xmlInvoicePath))
        {
            try
            {
                var xmlInvoiceFileName = Path.GetFileName(xmlInvoicePath);
                string xmlInvoiceContent = File.ReadAllText(xmlInvoicePath, Encoding.UTF8);
                var pdfResult = appRunner.CreateAndGetPdfStream(xmlInvoiceFileName, xmlInvoiceContent, string.Empty);

                if (pdfResult == null)
                {
                    _contentXmlToPdfViewerViewModel.DocumentSource = "<html><body><h1>PDF generation returned null result.</h1></body></html>";
                    return;
                }

                if (pdfResult.IsFailure)
                {
                    var msg = System.Net.WebUtility.HtmlEncode(pdfResult.Error.Message);
                    _contentXmlToPdfViewerViewModel.DocumentSource = $"<html><body><h1>The selected file is not an eInvoice: {msg}</h1></body></html>";
                    return;
                }

                var pdfStream = pdfResult.Value;
                if (pdfStream == null || pdfStream.Length == 0)
                {
                    _contentXmlToPdfViewerViewModel.DocumentSource = "<html><body><h1>PDF stream is empty.</h1></body></html>";
                    return;
                }

                pdfStream.Position = 0; // ✅ Important if the stream ends

                _contentXmlToPdfViewerViewModel.DocumentSource = HtmlPdfRenderer.CreateHtmlViewerFromPdf(pdfStream);
                //throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                string errorMessage = System.Net.WebUtility.HtmlEncode(ex.Message);
                _contentXmlToPdfViewerViewModel.DocumentSource = $"<html><body><h1>The selected file is not an eInvoice: {errorMessage}</h1></body></html>";
            }
        }
    }
}
