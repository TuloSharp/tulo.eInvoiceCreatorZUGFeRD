using iText.Kernel.Pdf;
using System.Security.Cryptography;
using System.Text;

[TestClass]
public class ZugferdPdfA3ITextVerificationTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void Verifies_Embedded_Xml_And_Checksum_With_iText()
    {
        string fileName = "ZF_Extended__Sammelrechnung_3_Bestellungen_generated_pdfa3.pdf";
        string expectedEmbeddedFileName = "factur-x.xml";

        TestContext.WriteLine("=== TEST START ===");
        TestContext.WriteLine($"Expected embedded file name: {expectedEmbeddedFileName}");
        TestContext.WriteLine($"PDF file name: {fileName}");

        string examplesDir = GetExamplesPath();
        string pdfA3Path = Path.Combine(examplesDir, fileName);

        TestContext.WriteLine($"Examples directory: {examplesDir}");
        TestContext.WriteLine($"Full PDF path: {pdfA3Path}");

        Assert.IsTrue(File.Exists(pdfA3Path), $"PDF/A-3 file not found: {pdfA3Path}");
        TestContext.WriteLine("✔ PDF/A-3 file exists.");

        using PdfReader reader = new PdfReader(pdfA3Path);
        using PdfDocument pdfDocument = new PdfDocument(reader);

        TestContext.WriteLine("✔ PDF opened successfully with iText.");
        TestContext.WriteLine($"Number of pages: {pdfDocument.GetNumberOfPages()}");

        PdfDictionary catalog = pdfDocument.GetCatalog().GetPdfObject();
        Assert.IsNotNull(catalog, "Catalog dictionary is missing.");
        TestContext.WriteLine("✔ Catalog dictionary found.");

        PdfDictionary names = catalog.GetAsDictionary(PdfName.Names);
        Assert.IsNotNull(names, "Catalog does not contain /Names.");
        TestContext.WriteLine("✔ /Names dictionary found in catalog.");

        PdfDictionary embeddedFiles = names.GetAsDictionary(PdfName.EmbeddedFiles);
        Assert.IsNotNull(embeddedFiles, "/EmbeddedFiles dictionary is missing.");
        TestContext.WriteLine("✔ /EmbeddedFiles dictionary found.");

        PdfArray nameArray = embeddedFiles.GetAsArray(PdfName.Names);
        Assert.IsNotNull(nameArray, "/Names array is missing.");
        TestContext.WriteLine($"✔ /Names array found. Size: {nameArray.Size()}");

        Assert.IsTrue(nameArray.Size() >= 2, "/Names array does not contain expected entries.");
        TestContext.WriteLine("✔ /Names array contains at least 2 entries.");

        for (int i = 0; i < nameArray.Size(); i++)
        {
            var obj = nameArray.Get(i);
            TestContext.WriteLine($"Names[{i}] = {obj}");
        }

        string embeddedName = nameArray.GetAsString(0).ToUnicodeString();
        TestContext.WriteLine($"Embedded name read from /Names[0]: {embeddedName}");
        Assert.AreEqual(expectedEmbeddedFileName, embeddedName, "Embedded XML file name does not match.");
        TestContext.WriteLine("✔ Embedded file name matches expected value.");

        PdfDictionary fileSpec = nameArray.GetAsDictionary(1);
        Assert.IsNotNull(fileSpec, "Embedded file specification dictionary is missing.");
        TestContext.WriteLine("✔ File specification dictionary found.");

        PdfString fValue = fileSpec.GetAsString(PdfName.F);
        Assert.IsNotNull(fValue, "File specification /F is missing.");

        string fileSpecName = fValue.ToUnicodeString();
        TestContext.WriteLine($"File specification /F: {fileSpecName}");
        Assert.AreEqual(expectedEmbeddedFileName, fileSpecName, "File specification /F does not match.");
        TestContext.WriteLine("✔ File specification /F matches expected value.");

        PdfDictionary efDictionary = fileSpec.GetAsDictionary(PdfName.EF);
        Assert.IsNotNull(efDictionary, "File specification /EF dictionary is missing.");
        TestContext.WriteLine("✔ File specification /EF dictionary found.");

        PdfStream embeddedFileStream = efDictionary.GetAsStream(PdfName.F);
        Assert.IsNotNull(embeddedFileStream, "Embedded XML file stream is missing.");
        TestContext.WriteLine("✔ Embedded XML file stream found.");

        byte[] embeddedBytes = embeddedFileStream.GetBytes();
        Assert.IsTrue(embeddedBytes.Length > 0, "Embedded XML file stream is empty.");
        TestContext.WriteLine($"✔ Embedded XML stream length: {embeddedBytes.Length} bytes");

        string embeddedXml = Encoding.UTF8.GetString(embeddedBytes);
        TestContext.WriteLine("Embedded XML preview (first 1000 chars):");
        TestContext.WriteLine(embeddedXml.Substring(0, Math.Min(1000, embeddedXml.Length)));

        Assert.IsTrue(
            embeddedXml.Contains("CrossIndustryInvoice"),
            "Embedded XML does not look like a CII invoice.");
        TestContext.WriteLine("✔ Embedded XML contains 'CrossIndustryInvoice'.");

        PdfDictionary parameters = embeddedFileStream.GetAsDictionary(PdfName.Params);
        Assert.IsNotNull(parameters, "Embedded file stream /Params is missing.");
        TestContext.WriteLine("✔ Embedded file stream /Params found.");

        PdfString checksumString = parameters.GetAsString(new PdfName("CheckSum"));
        Assert.IsNotNull(checksumString, "Embedded file stream /CheckSum is missing.");

        string actualChecksum = checksumString.ToUnicodeString();
        TestContext.WriteLine($"Checksum from PDF (/CheckSum): {actualChecksum}");

        byte[] md5Bytes = MD5.HashData(embeddedBytes);
        string expectedChecksum = BitConverter.ToString(md5Bytes).Replace("-", "").ToLowerInvariant();

        TestContext.WriteLine($"Computed MD5 checksum:      {expectedChecksum}");
        Assert.AreEqual(expectedChecksum, actualChecksum, "Embedded XML checksum does not match the computed MD5.");
        TestContext.WriteLine("✔ Embedded XML checksum matches computed MD5.");

        TestContext.WriteLine("=== TEST SUCCESSFULLY COMPLETED ===");
    }

    private string GetExamplesPath()
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string examplesPath = Path.Combine(baseDirectory, "Examples");

        TestContext.WriteLine($"Base directory: {baseDirectory}");
        TestContext.WriteLine($"Resolved examples path: {examplesPath}");

        if (!Directory.Exists(examplesPath))
        {
            throw new DirectoryNotFoundException($"The examples directory was not found: {examplesPath}");
        }

        return examplesPath;
    }
}
