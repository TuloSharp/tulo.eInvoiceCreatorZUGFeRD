using Microsoft.Win32;
using System.IO;
using System.Text;
using tulo.CommonMVVM.Collector;
using tulo.CommonMVVM.Commands;
using tulo.CoreLib.PDFs;
using tulo.eInvoice.eInvoiceViewer.Utilities;
using tulo.eInvoice.eInvoiceViewer.ViewModels;

namespace tulo.eInvoice.eInvoiceViewer.Commands;
public class SelectXmlFilePathCommand(ContentXmlToPdfViewerViewModel contentXmlToPdfViewerViewModel, ICollectorCollection collectorCollection) : BaseCommand
{
    private readonly ICollectorCollection _collectorCollection = collectorCollection;
    private readonly ContentXmlToPdfViewerViewModel _contentXmlToPdfViewerViewModel = contentXmlToPdfViewerViewModel;

    public override void Execute(object parameter)
    {
        IAppRunner appRunner = _collectorCollection.GetService<IAppRunner>();

        var dlg = new OpenFileDialog
        {
            CheckFileExists = false,
            CheckPathExists = true,
            Title = "Select an eInvoice XML file ",
            //Filter = "All Files (*.*)|*.*"
            Filter = "XML-Files (*.xml)|*.xml",
            Multiselect = false
        };

        if (dlg.ShowDialog() == true)
        {
            string selectedXmlInvoicePath = dlg.FileName;


            if (File.Exists(selectedXmlInvoicePath))
            {
               
                _contentXmlToPdfViewerViewModel.StatusMessage = "Selected file: " + selectedXmlInvoicePath;
               
                try
                {
                    var xmlInvoiceFileName = Path.GetFileName(selectedXmlInvoicePath);
                    string xmlInvoiceContent = File.ReadAllText(selectedXmlInvoicePath, Encoding.UTF8);

                    var pdfResult = appRunner.CreateAndGetPdfStream(xmlInvoiceFileName, xmlInvoiceContent, string.Empty);

                    if (pdfResult == null)
                    {
                        _contentXmlToPdfViewerViewModel.DocumentSource = "<html><body><h1>PDF generation returned null result.</h1></body></html>";
                        return;
                    }

                    if (pdfResult.IsFailure)
                    {
                        var msg = System.Net.WebUtility.HtmlEncode(pdfResult.Error.Message);
                        _contentXmlToPdfViewerViewModel.DocumentSource = $"<html><body><h1>PDF creation failed for {System.Net.WebUtility.HtmlEncode(xmlInvoiceFileName)}: {msg}</h1></body></html>";
                        return;
                    }

                    var stream = pdfResult.Value;
                    if (stream == null || stream.Length == 0)
                    {
                        _contentXmlToPdfViewerViewModel.DocumentSource = $"<html><body><h1>PDF stream is empty for {System.Net.WebUtility.HtmlEncode(xmlInvoiceFileName)}.</h1></body></html>";
                        return;
                    }

                    stream.Position = 0;

                    _contentXmlToPdfViewerViewModel.DocumentSource =
                        HtmlPdfRenderer.CreateHtmlViewerFromPdf(stream);
                }
                catch (Exception ex)
                {
                    string errorMessage = System.Net.WebUtility.HtmlEncode(ex.Message);
                    _contentXmlToPdfViewerViewModel.DocumentSource = $"<html><body><h1>The selected file is not an eInvoice: {errorMessage}</h1></body></html>";
                }
            }
            else
            {
                _contentXmlToPdfViewerViewModel.StatusMessage = $"path is invalid: {selectedXmlInvoicePath}";
            }
        }
    }
}
