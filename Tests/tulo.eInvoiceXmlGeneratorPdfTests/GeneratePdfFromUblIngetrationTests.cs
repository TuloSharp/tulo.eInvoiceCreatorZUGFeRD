using tulo.XMLeInvoiceToPdf.Languages;
using tulo.XMLeInvoiceToPdf.Services;
using tulo.XMLeInvoiceToPdf.Utilities;
using PdfSharp.Fonts;
using System.Diagnostics;
using System.Text;

namespace tulo.XMLeInvoiceToPdfTests;

[TestClass]
public class GeneratePdfFromUblIngetrationTests
{
    private ITranslatorProvider _translatorProvider = null!;
    private IPdfGeneratorFromInvoice _pdfGeneratorInvoiceUbl = null!;
    private string _tempDir = null!;
    private string _translationPath = null!;

    [TestInitialize]
    public void Setup()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "InvoicePdfIntegrationTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
        _translationPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Languages", "de-DE.xml");

        if (GlobalFontSettings.FontResolver == null)
            GlobalFontSettings.FontResolver = new EmbeddedFontResolver();

        _translatorProvider = new TranslatorProvider(_translationPath);
        _pdfGeneratorInvoiceUbl = new PdfGeneratorFromInvoiceUbl(_translatorProvider);
    }

    [TestMethod(DisplayName = "Create an pdf from xml 05.01a-INVOICE_ubl")]
    [DataRow("05.01a-INVOICE_ubl.xml", "05.01a-INVOICE_ubl.pdf", true)]
    public void Run_Create_eInvoice_ReturnsSuccess(string xmlInvoiceFileName, string outputPdfFileName, bool customInfo)
    {
        // Arrange
        string pathExampleInvoices = GetInvoiceExamplesPath();
        string xmlInvoicePath = Path.Combine(pathExampleInvoices, xmlInvoiceFileName);
        string outputPdfPath = Path.Combine(_tempDir, outputPdfFileName);
        string xmlInvoiceContent = File.ReadAllText(xmlInvoicePath, Encoding.UTF8);

        // Act
        string createdPath = _pdfGeneratorInvoiceUbl.GeneratePdfFile(outputPdfPath, xmlInvoiceFileName, xmlInvoiceContent, customInfo);


        // Assert
        Assert.IsFalse(string.IsNullOrWhiteSpace(createdPath), "Generator returned an empty output path.");
        Assert.AreEqual(outputPdfPath, createdPath, "Generator returned an unexpected output path.");
        Assert.IsTrue(File.Exists(createdPath), $"PDF output file was not created: '{createdPath}'");

        var fileInfo = new FileInfo(createdPath);
        Assert.IsGreaterThan(0, fileInfo.Length, "Generated PDF file is empty.");

        var createdPdf = Path.Combine(_tempDir, createdPath);
        if (File.Exists(createdPdf))
        {
            OpenWithDefaultPdfViewer(createdPdf);
        }
    }

    private string GetInvoiceExamplesPath()
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

        string invoiceExamplesPath = Path.Combine(baseDirectory, "Examples");

        if (!Directory.Exists(invoiceExamplesPath))
        {
            throw new DirectoryNotFoundException($"Das Verzeichnis für Rechnungsbeispiele wurde nicht gefunden: {invoiceExamplesPath}");
        }

        return invoiceExamplesPath;
    }
    private void OpenWithDefaultPdfViewer(string pdfPath)
    {
        var processInfo = new ProcessStartInfo(pdfPath)
        {
            UseShellExecute = true
        };
        Process.Start(processInfo);
    }

}
