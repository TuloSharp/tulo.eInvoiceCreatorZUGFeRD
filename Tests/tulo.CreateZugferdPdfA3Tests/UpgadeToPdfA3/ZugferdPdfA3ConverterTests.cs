using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.IO;
using System.Diagnostics;
using System.Text;
using tulo.UpgradeToPdfA3.Interfaces;
using tulo.UpgradeToPdfA3.Options;
using tulo.UpgradeToPdfA3.ResultPattern;
using tulo.UpgradeToPdfA3.Services;
using tulo.XMLeInvoiceToPdf.Languages;
using tulo.XMLeInvoiceToPdf.Services;
using tulo.XMLeInvoiceToPdf.Utilities;

namespace tulo.UpgradeToPdfA3Tests.UpgadeToPdfA3;

[TestClass]
public class ZugferdPdfA3PdfSharpIntegrationTests
{
    private ITranslatorProvider _translatorProvider = null!;
    private IPdfGeneratorFromInvoice _pdfGeneratorFromInvoice = null!;
    private IToPdfAConverterService _toPdfAConverterService = null!;
    private IToPdfA3UpgradeService _toPdfA3UpgradeService = null!;
    private IUpgradeToPdfA3Options _appOptions = null!;

    [TestInitialize]
    public void Setup()
    {
        string translationPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Languages", "de-DE.xml");

        if (GlobalFontSettings.FontResolver == null)
            GlobalFontSettings.FontResolver = new EmbeddedFontResolver();

        _translatorProvider = new TranslatorProvider(translationPath);
        _pdfGeneratorFromInvoice = new PdfGeneratorFromInvoiceCii(_translatorProvider);

        _toPdfAConverterService = new ToPdfAConverterService(new PdfAConverterValidator(), new PdfADocumentInfoWriter(), new PdfALanguageWriter(), new PdfAMetadataWriter(), new PdfAOutputIntentWriter());

        _toPdfA3UpgradeService = new ToPdfA3UpgradeService(new PdfA3UpgradeValidator(), new PdfA3AttachmentWriter(), new PdfAMetadataWriter());

        _appOptions = CreateTestAppOptions();
    }

    [TestMethod]
    public void Creates_Pdf_From_Xml_Then_PdfA3_And_Validates_Result()
    {
        string fileName = "ZF_Extended__Sammelrechnung_3_Bestellungen";

        string examplesDir = GetExamplesPath();

        Assert.IsTrue(Directory.Exists(examplesDir), $"Examples folder not found: {examplesDir}");

        string xmlPath = Path.Combine(examplesDir, fileName + ".xml");
        Assert.IsTrue(File.Exists(xmlPath), $"XML file not found: {xmlPath}");

        string xmlContent = File.ReadAllText(xmlPath, Encoding.UTF8);
        byte[] xmlBytes = File.ReadAllBytes(xmlPath);

        string outputGeneratedPdfPath = Path.Combine(examplesDir, fileName + "_generated.pdf");
        string outputPdfAPath = Path.Combine(examplesDir, fileName + "_generated_pdfa.pdf");
        string outputPdfA3Path = Path.Combine(examplesDir, fileName + "_generated_pdfa3.pdf");

        string createdPdfPath = _pdfGeneratorFromInvoice.GeneratePdfFile(outputGeneratedPdfPath, fileName + ".xml", xmlContent, hasToRenderHeader: true);

        Assert.IsFalse(string.IsNullOrWhiteSpace(createdPdfPath), "Generated PDF path is empty.");
        Assert.AreEqual(outputGeneratedPdfPath, createdPdfPath, "Generated PDF path is unexpected.");
        Assert.IsTrue(File.Exists(createdPdfPath), $"Generated PDF file was not created: {createdPdfPath}");
        Assert.IsTrue(new FileInfo(createdPdfPath).Length > 0, "Generated PDF file is empty.");

        using (PdfDocument pdfDocument = PdfReader.Open(createdPdfPath, PdfDocumentOpenMode.Modify))
        {
            OperationResult pdfAResult = _toPdfAConverterService.ApplyPdfA(pdfDocument, _appOptions);
            Assert.IsTrue(pdfAResult.Success, $"ApplyPdfA failed: {pdfAResult.Message}");

            pdfDocument.Save(outputPdfAPath);
        }

        Assert.IsTrue(File.Exists(outputPdfAPath), $"PDF/A file was not created: {outputPdfAPath}");
        Assert.IsTrue(new FileInfo(outputPdfAPath).Length > 0, "Generated PDF/A file is empty.");

        OperationResult pdfA3Result = _toPdfA3UpgradeService.UpgradeToPdfA3(inputPdfAPath: outputPdfAPath, outputPdfA3Path: outputPdfA3Path, xmlFileName: Path.GetFileName(xmlPath), xmlBytes: xmlBytes, appOptions: _appOptions);

        Assert.IsTrue(pdfA3Result.Success, $"UpgradeToPdfA3 failed: {pdfA3Result.Message}");
        Assert.IsTrue(File.Exists(outputPdfA3Path), $"PDF/A-3 file was not created: {outputPdfA3Path}");
        Assert.IsTrue(new FileInfo(outputPdfA3Path).Length > 0, "Generated PDF/A-3 file is empty.");

        using (PdfDocument pdfDocument = PdfReader.Open(outputPdfA3Path, PdfDocumentOpenMode.Modify))
        {
            Assert.IsTrue(pdfDocument.PageCount > 0, "Generated PDF/A-3 has no pages.");

            Assert.IsNotNull(pdfDocument.Info, "PDF info metadata object is null.");
            Assert.IsFalse(string.IsNullOrWhiteSpace(pdfDocument.Info.Title), "PDF metadata 'Title' is missing.");
            Assert.IsFalse(string.IsNullOrWhiteSpace(pdfDocument.Info.Author), "PDF metadata 'Author' is missing.");

            Assert.IsTrue(pdfDocument.Internals.Catalog.Elements.ContainsKey("/Metadata"),
                "PDF catalog does not contain /Metadata.");

            Assert.IsTrue(pdfDocument.Internals.Catalog.Elements.ContainsKey("/AF"),
                "PDF catalog does not contain /AF.");

            Assert.IsTrue(pdfDocument.Internals.Catalog.Elements.ContainsKey("/Names"),
                "PDF catalog does not contain /Names.");

            var namesDictionary = pdfDocument.Internals.Catalog.Elements.GetDictionary("/Names");
            Assert.IsNotNull(namesDictionary, "/Names dictionary is missing.");

            var embeddedFilesDictionary = namesDictionary!.Elements.GetDictionary("/EmbeddedFiles");
            Assert.IsNotNull(embeddedFilesDictionary, "/EmbeddedFiles dictionary is missing.");

            var namesArray = embeddedFilesDictionary!.Elements.GetArray("/Names");
            Assert.IsNotNull(namesArray, "Embedded files name array is missing.");
            Assert.IsTrue(namesArray!.Elements.Count >= 2, "Embedded files name array does not contain expected entries.");

            var embeddedName = namesArray.Elements[0] as PdfString;
            Assert.IsNotNull(embeddedName, "The first embedded files name entry is not a PdfString.");
            Assert.AreEqual("factur-x.xml", embeddedName!.Value, "The embedded XML file name does not match.");

            var fileSpecReference = namesArray.Elements[1] as PdfReference;
            Assert.IsNotNull(fileSpecReference, "The second embedded files name entry is not a file specification reference.");

            var fileSpecDictionary = fileSpecReference!.Value as PdfDictionary;
            Assert.IsNotNull(fileSpecDictionary, "The file specification dictionary is missing.");

            var fileSpecFileName = fileSpecDictionary!.Elements["/F"] as PdfString;
            Assert.IsNotNull(fileSpecFileName, "The file specification does not contain /F.");
            Assert.AreEqual("factur-x.xml", fileSpecFileName!.Value, "The embedded file specification /F value does not match.");
        }

        OpenWithDefaultPdfViewer(outputPdfA3Path);
    }

    private IUpgradeToPdfA3Options CreateTestAppOptions()
    {
        string iccProfilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ColorProfiles", "sRGB.icc");

        return new UpgradeToPdfA3Options
        {
            PdfA3 = new PdfA3Options
            {
                Part = 3,
                Conformance = "B",
                Title = "E-Invoice",
                Description = "E-Invoice as PDF/A-3",
                Author = "tulo.UpgradeToPdfA3",
                Creator = "tulo.UpgradeToPdfA3",
                CreatorTool = "tulo.UpgradeToPdfA3",
                Producer = "PdfSharp Extended",
                Language = "de-DE",
                IccProfilePath = iccProfilePath,
                AttachmentDescription = "Factur-X / ZUGFeRD invoice XML",
                AfRelationship = "Alternative",
                DocumentType = "INVOICE",
                FacturXVersion = "1.0",
                ConformanceLevel = "EXTENDED"
            }
        };
    }

    private string GetExamplesPath()
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string examplesPath = Path.Combine(baseDirectory, "Examples");

        if (!Directory.Exists(examplesPath))
        {
            throw new DirectoryNotFoundException($"The examples directory was not found: {examplesPath}");
        }

        return examplesPath;
    }

    private void OpenWithDefaultPdfViewer(string pdfPath)
    {
        if (!File.Exists(pdfPath))
            throw new FileNotFoundException("PDF file was not found.", pdfPath);

        var processInfo = new ProcessStartInfo
        {
            FileName = pdfPath,
            UseShellExecute = true
        };

        Process.Start(processInfo);
    }
}