using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.Diagnostics;
using tulo.XMLeInvoiceToPdf.Languages;
using tulo.XMLeInvoiceToPdf.Options;
using tulo.XMLeInvoiceToPdf.ResultPattern;
using tulo.XMLeInvoiceToPdf.Services;
using tulo.XMLeInvoiceToPdf.Utilities;

namespace tulo.XMLeInvoiceToPdfTests;

[TestClass]
public class GeneratePdfA3FromCiiIngetrationTests
{
    private ITranslatorProvider _translatorProvider = null!;
    private IPdfGeneratorFromInvoice _pdfGeneratorInvoiceCii = null!;
    private IToPdfAConverterService _pdfAService = null!;
    private IToPdfA3UpgradeService _pdfA3UpgradeService = null!;
    private IAppOptions _appOptions = null!;

    private string _tempDir = null!;
    private string _translationPath = null!;
    private string _iccProfilePath = null!;

    [TestInitialize]
    public void Setup()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "InvoicePdfA3IntegrationTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
        _translationPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Languages", "de.xml");

        _iccProfilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Assets", "ColorProfiles", "sRGB.icc");

        if (GlobalFontSettings.FontResolver == null)
            GlobalFontSettings.FontResolver = new EmbeddedFontResolver();

        _translatorProvider = new TranslatorProvider(_translationPath);
        _pdfAService = new ToPdfAConverterService();
        _pdfA3UpgradeService = new ToPdfA3UpgradeService();

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
                Part = 3,
                AttachmentDescription = "Factur-X / ZUGFeRD invoice XML",
                AfRelationship = "Alternative",
                DocumentType = "INVOICE",
                FacturXVersion = "1.0",
                ConformanceLevel = "EXTENDED"
            }
        };

        _pdfGeneratorInvoiceCii = new PdfGeneratorFromInvoiceCii(_translatorProvider, _pdfAService, _appOptions);
    }

    [TestMethod(DisplayName = "Create an  PDF/A-3 from xml ZF_Extended__Sammelrechnung_3_Bestellungen ")]
    [DataRow("ZF_Extended__Sammelrechnung_3_Bestellungen.xml", "ZF_Extended__Sammelrechnung_3_Bestellungen.pdf", true)]
    public void Run_Create_eInvoicePdfA3_ReturnsSuccess(string xmlInvoiceFileName, string outputPdfFileName, bool hasToRenderHeader)
    {
        // Arrange
        string pathExampleInvoices = GetInvoiceExamplesPath();
        string xmlInvoicePath = Path.Combine(pathExampleInvoices, xmlInvoiceFileName);
        string outputPdfAPath = Path.Combine(_tempDir, outputPdfFileName);
        string outputPdfA3Path = Path.Combine(_tempDir, "PDF_A3_" + outputPdfFileName);

        string xmlInvoiceContent = File.ReadAllText(xmlInvoicePath, System.Text.Encoding.UTF8);
        byte[] xmlBytes = System.Text.Encoding.UTF8.GetBytes(xmlInvoiceContent);

        // Act
        string createdPdfAPath = _pdfGeneratorInvoiceCii.GeneratePdfFile(outputPdfAPath, xmlInvoiceFileName, xmlInvoiceContent, hasToRenderHeader);
        OperationResult pdfA3Result = _pdfA3UpgradeService.UpgradeToPdfA3(createdPdfAPath, outputPdfA3Path, "factur-x.xml", xmlBytes, _appOptions);

        // Assert PDF/A
        Assert.IsFalse(string.IsNullOrWhiteSpace(createdPdfAPath), "Generator returned an empty output path.");
        Assert.AreEqual(outputPdfAPath, createdPdfAPath, "Generator returned an unexpected output path.");
        Assert.IsTrue(File.Exists(createdPdfAPath), $"PDF/A output file was not created: '{createdPdfAPath}'");
        Assert.IsTrue(new FileInfo(createdPdfAPath).Length > 0, "Generated PDF/A file is empty.");

        using (PdfDocument pdfADocument = PdfReader.Open(createdPdfAPath, PdfDocumentOpenMode.Import))
        {
            Assert.IsTrue(pdfADocument.PageCount > 0, "PDF/A contains no pages.");
            Assert.IsNotNull(pdfADocument.Internals.Catalog.Elements["/OutputIntents"], "PDF/A does not contain /OutputIntents.");
            Assert.IsNotNull(pdfADocument.Internals.Catalog.Elements["/Metadata"], "PDF/A does not contain /Metadata.");
            Assert.IsNotNull(pdfADocument.Internals.Catalog.Elements["/Lang"], "PDF/A does not contain /Lang.");

            Assert.IsFalse(string.IsNullOrWhiteSpace(pdfADocument.Info.Title), "PDF/A title is missing.");
            Assert.IsFalse(string.IsNullOrWhiteSpace(pdfADocument.Info.Author), "PDF/A author is missing.");
            Assert.IsFalse(string.IsNullOrWhiteSpace(pdfADocument.Info.Creator), "PDF/A creator is missing.");
            Assert.IsFalse(string.IsNullOrWhiteSpace(pdfADocument.Info.Producer), "PDF/A producer is missing.");
        }

        // Assert PDF/A-3 upgrade result
        Assert.IsTrue(pdfA3Result.Success, $"PDF/A-3 upgrade failed: {pdfA3Result.Message}");
        Assert.IsTrue(File.Exists(outputPdfA3Path), $"PDF/A-3 output file was not created: '{outputPdfA3Path}'");
        Assert.IsTrue(new FileInfo(outputPdfA3Path).Length > 0, "Generated PDF/A-3 file is empty.");

        using (PdfDocument pdfA3Document = PdfReader.Open(outputPdfA3Path, PdfDocumentOpenMode.Import))
        {
            Assert.IsTrue(pdfA3Document.PageCount > 0, "PDF/A-3 contains no pages.");
            Assert.IsNotNull(pdfA3Document.Internals.Catalog.Elements["/Metadata"], "PDF/A-3 does not contain /Metadata.");
            Assert.IsNotNull(pdfA3Document.Internals.Catalog.Elements["/AF"], "PDF/A-3 does not contain /AF.");

            PdfDictionary? names = pdfA3Document.Internals.Catalog.Elements.GetDictionary("/Names");
            Assert.IsNotNull(names, "PDF/A-3 does not contain /Names.");

            PdfDictionary? embeddedFiles = names?.Elements.GetDictionary("/EmbeddedFiles");
            Assert.IsNotNull(embeddedFiles, "PDF/A-3 does not contain /EmbeddedFiles.");

            PdfArray? embeddedNames = embeddedFiles?.Elements.GetArray("/Names");
            Assert.IsNotNull(embeddedNames, "PDF/A-3 does not contain embedded file name entries.");
            Assert.IsTrue(embeddedNames!.Elements.Count >= 2, "PDF/A-3 embedded file entries are incomplete.");
        }

        OpenWithDefaultPdfViewer(outputPdfA3Path);
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
