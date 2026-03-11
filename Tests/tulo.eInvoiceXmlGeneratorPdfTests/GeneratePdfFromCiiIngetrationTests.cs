using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.Diagnostics;
using tulo.XMLeInvoiceToPdf.Languages;
using tulo.XMLeInvoiceToPdf.Options;
using tulo.XMLeInvoiceToPdf.Services;
using tulo.XMLeInvoiceToPdf.Utilities;

namespace tulo.XMLeInvoiceToPdfTests;

[TestClass]
public class GeneratePdfFromCiiIngetrationTests
{
    private ITranslatorProvider _translatorProvider = null!;
    private IPdfGeneratorFromInvoice _pdfGeneratorInvoiceCii = null!;
    private IToPdfAConverterService _pdfAService = null!;
    private IAppOptions _appOptions = null!;

    private string _tempDir = null!;
    private string _translationPath = null!;
    private string _iccProfilePath = null!;

    [TestInitialize]
    public void Setup()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "InvoicePdfIntegrationTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
        _translationPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Languages", "de.xml");

        _iccProfilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Assets", "ColorProfiles", "sRGB.icc");

        if (GlobalFontSettings.FontResolver == null)
            GlobalFontSettings.FontResolver = new EmbeddedFontResolver();

        _translatorProvider = new TranslatorProvider(_translationPath);
        _pdfAService = new ToPdfAConverterService();

        _appOptions = new AppOptions
        {
            PdfA = new AppOptions.PdfAOptions
            {
                IccProfilePath = _iccProfilePath,
                CreatorTool = "IntegrationTests",
                Creator = "IntegrationTests",
                Producer = "PdfSharp",
                Title = "Test Invoice",
                Description = "Integration test invoice PDF/A",
                Author = "TestRunner",
                Language = "de-DE",
                Conformance = "B",
                Part = 2
            }
        };

        _pdfGeneratorInvoiceCii = new PdfGeneratorFromInvoiceCii(_translatorProvider, _pdfAService, _appOptions);
    }

    [TestMethod(DisplayName = "Create an pdf from xml ZF_Extended__Sammelrechnung_3_Bestellungen ")]
    [DataRow("ZF_Extended__Sammelrechnung_3_Bestellungen.xml", "ZF_Extended__Sammelrechnung_3_Bestellungen.pdf", true)]
    public void Run_Create_eInvoice_ReturnsSuccess(string xmlInvoiceFileName, string outputPdfFileName, bool hasToRenderHeader)
    {
        // Arrange
        string pathExampleInvoices = GetInvoiceExamplesPath();
        string xmlInvoicePath = Path.Combine(pathExampleInvoices, xmlInvoiceFileName);
        string outputPdfPath = Path.Combine(_tempDir, outputPdfFileName);
        string xmlInvoiceContent = File.ReadAllText(xmlInvoicePath, System.Text.Encoding.UTF8);

        // Act
        string createdPath = _pdfGeneratorInvoiceCii.GeneratePdfFile(outputPdfPath, xmlInvoiceFileName, xmlInvoiceContent, hasToRenderHeader);

        // Assert
        Assert.IsFalse(string.IsNullOrWhiteSpace(createdPath), "Generator returned an empty output path.");
        Assert.AreEqual(outputPdfPath, createdPath, "Generator returned an unexpected output path.");
        Assert.IsTrue(File.Exists(createdPath), $"PDF output file was not created: '{createdPath}'");

        Assert.IsTrue(new FileInfo(createdPath).Length > 0, "Generated PDF file is empty.");

        // PDF erneut laden und Struktur prüfen
        using PdfDocument document = PdfReader.Open(createdPath, PdfDocumentOpenMode.Import);

        // PDf contains pages
        Assert.IsTrue(document.PageCount > 0, "PDF contains no pages.");

        var outputIntents = document.Internals.Catalog.Elements["/OutputIntents"];
        Assert.IsNotNull(outputIntents, "PDF does not contain /OutputIntents.");

        var metadata = document.Internals.Catalog.Elements["/Metadata"];
        Assert.IsNotNull(metadata, "PDF does not contain /Metadata.");

        var lang = document.Internals.Catalog.Elements["/Lang"];
        Assert.IsNotNull(lang, "PDF does not contain /Lang.");

        // Document info
        Assert.IsFalse(string.IsNullOrWhiteSpace(document.Info.Title), "PDF title is missing.");
        Assert.IsFalse(string.IsNullOrWhiteSpace(document.Info.Author), "PDF author is missing.");
        Assert.IsFalse(string.IsNullOrWhiteSpace(document.Info.Creator), "PDF creator is missing.");
        Assert.IsFalse(string.IsNullOrWhiteSpace(document.Info.Producer), "PDF producer is missing.");

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
