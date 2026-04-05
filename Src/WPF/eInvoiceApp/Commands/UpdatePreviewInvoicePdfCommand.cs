using tulo.CommonMVVM.Collector;
using tulo.CommonMVVM.Commands;
using tulo.eInvoiceApp.ViewModels.Invoices;

namespace tulo.eInvoiceApp.Commands;
public class UpdatePreviewInvoicePdfCommand(InvoiceViewModel invoiceViewModel, ICollectorCollection collectorCollection) : BaseCommand

{
    private readonly ICollectorCollection _collectorCollection = collectorCollection;
    private readonly InvoiceViewModel _invoiceViewModel = invoiceViewModel;

    public override void Execute(object parameter)
    {
        
            try
            {

            _invoiceViewModel.DocumentSource = "<html><body><h1>PDF stream is empty.</h1></body></html>";
            //if (pdfResult == null)
            //{
            //    _invoiceViewModel.DocumentSource = "<html><body><h1>PDF generation returned null result.</h1></body></html>";
            //    return;
            //}

            //if (pdfResult.IsFailure)
            //{
            //    var msg = System.Net.WebUtility.HtmlEncode(pdfResult.Error.Message);
            //    _invoiceViewModel.DocumentSource = $"<html><body><h1>The selected file is not an eInvoice: {msg}</h1></body></html>";
            //    return;
            //}

            //var pdfStream = pdfResult.Value;
            //if (pdfStream == null || pdfStream.Length == 0)
            //{
            //    _invoiceViewModel.DocumentSource = "<html><body><h1>PDF stream is empty.</h1></body></html>";
            //    return;
            //}

            //pdfStream.Position = 0; // ✅ Important if the stream ends

            //_invoiceViewModel.DocumentSource = HtmlPdfRenderer.CreateHtmlViewerFromPdf(pdfStream);
        }
            catch (Exception ex)
            {
                string errorMessage = System.Net.WebUtility.HtmlEncode(ex.Message);
                _invoiceViewModel.DocumentSource = $"<html><body><h1>The selected file is not an eInvoice: {errorMessage}</h1></body></html>";
            }
        
    }
}
