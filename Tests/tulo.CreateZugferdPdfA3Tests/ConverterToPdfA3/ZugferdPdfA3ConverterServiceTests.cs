using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Filespec;
using iText.Kernel.XMP;
using iText.Pdfa;
using s2industries.ZUGFeRD;
using System.Diagnostics;
using System.Security.Cryptography;
using tulo.CreateZugferdPdfA3.ConverterToPdfA3;

namespace tulo.DomainTests.ConverterToPdfA3;

[TestClass]
public class ZugferdPdfA3ConverterServiceTests
{  
    public TestContext TestContext { get; set; } = default!;
    
    [TestMethod]
    public async Task CreatesPdfA3_With_s2industries_ZUGFeRD_InExamplesProjectFolder()
    {
        var fileName = "ZF_Extended__Sammelrechnung_3_Bestellungen";
        // Arrange
        IZugferdPdfA3ConverterService service = new ZugferdPdfA3ConverterService();

        string baseDir = AppContext.BaseDirectory;

        var samplesDir = Path.Combine(baseDir, "Examples");

        Assert.IsTrue(Directory.Exists(samplesDir), $"Samples folder not found: {samplesDir}");

        string xmlPath = Path.Combine(baseDir, "Examples", fileName + ".xml");
        string pdfPath = Path.Combine(baseDir, "Examples", fileName + ".pdf");

        // Validate that the files actually exist
        Assert.IsTrue(File.Exists(pdfPath), $"PDF file not found: {pdfPath}");
        Assert.IsTrue(File.Exists(xmlPath), $"XML file not found: {xmlPath}");

        var examplesProjectDir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "Examples"));
        //var outputPath = Path.Combine(Path.GetTempPath(), $"{fileName}" + "_zugferd_pdfa3.pdf");
        var outputPath = Path.Combine(examplesProjectDir, $"{fileName}" + "_zugferd_pdfa3.pdf");

        // Act
        var result = await service.ConvertAsync(inputPdfPath: pdfPath, inputXmlPath: xmlPath, outputPdfPath: outputPath, zugferdVersion: ZUGFeRDVersion.Version23, profile: Profile.Extended, format: ZUGFeRDFormats.CII);

        // Assert
        Assert.IsNotNull(result);

        // If your Result<T> API differs, adapt these lines:
        Assert.IsTrue(result.IsSuccess, "Conversion failed: " + (result.Error.Message ?? "unknown error") + " (Code: " + (result.Error.Code ?? "n/a") + ")");

        Assert.IsFalse(string.IsNullOrWhiteSpace(result.Value), "Result value (output path) is empty.");
        Assert.IsTrue(File.Exists(result.Value), $"Output file not created: {result.Value}");

        var fileInfo = new FileInfo(result.Value);
        Assert.IsGreaterThan(0, fileInfo.Length, "Output file is empty.");

        // Now open outputPath in Acrobat to see if the Factur-X/ZUGFeRD badge appears.
        if (File.Exists(outputPath))
        {
            OpenWithDefaultPdfViewer(outputPath);
        }
    }

    [TestMethod]
    public void CreatePdfA3_With_FacturXXml_IText_InExamplesProjectFolder()
    {
        var fileName = "ZF_Extended__Sammelrechnung_3_Bestellungen";

        string baseDir = AppContext.BaseDirectory;
        string examplesDir = Path.Combine(baseDir, "Examples");
        string iccDir = Path.Combine(baseDir, "Assets");

        Assert.IsTrue(Directory.Exists(examplesDir), $"Examples folder not found: {examplesDir}");

        string inputPdfPath = Path.Combine(examplesDir, fileName + ".pdf");
        string inputXmlPath = Path.Combine(examplesDir, fileName + ".xml");
        string iccPath = Path.Combine(iccDir, "sRGB.icc");

        Assert.IsTrue(File.Exists(inputPdfPath), $"PDF file not found: {inputPdfPath}");
        Assert.IsTrue(File.Exists(inputXmlPath), $"XML file not found: {inputXmlPath}");
        Assert.IsTrue(File.Exists(iccPath), $"ICC profile not found: {iccPath} (add sRGB.icc to Examples)");

        var examplesProjectDir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "Examples"));
        //string outputPath = Path.Combine(Path.GetTempPath(), $"{fileName}_itext_pdfa3.pdf");
        string outputPath = Path.Combine(examplesProjectDir, $"{fileName}_itext_pdfa3.pdf");

        CreatePdfA3WithEmbeddedFacturXXml(inputPdfPath, inputXmlPath, iccPath, outputPath);

        Assert.IsTrue(File.Exists(outputPath), $"Output file not created: {outputPath}");
        Assert.IsGreaterThan(0, new FileInfo(outputPath).Length, "Output file is empty.");

        // Now open outputPath in Acrobat to see if the Factur-X/ZUGFeRD badge appears.
        if (File.Exists(outputPath))
        {
            OpenWithDefaultPdfViewer(outputPath);
        }
    }

    [TestMethod]
    public void OutputPdf_Should_Have_PdfA3_And_FacturX_Attachment()
    {
        string pdfPath = Path.Combine(AppContext.BaseDirectory, "Examples", "ZF_Extended__Sammelrechnung_3_Bestellungen_itext_pdfa3.pdf");

        Assert.IsTrue(File.Exists(pdfPath), $"PDF not found: {pdfPath}");

        using var reader = new PdfReader(pdfPath);
        using var pdf = new iText.Kernel.Pdf.PdfDocument(reader);

        iText.Kernel.Pdf.PdfDictionary catalog = pdf.GetCatalog().GetPdfObject();

        // A) Associated Files (/AF)
        var afArray = catalog.GetAsArray(iText.Kernel.Pdf.PdfName.AF);
        Assert.IsNotNull(afArray, "Catalog /AF is missing (Associated Files).");
        Assert.IsGreaterThan(0, afArray.Size(), "Catalog /AF exists but is empty.");

        // B) EmbeddedFiles in Names tree
        var namesDict = catalog.GetAsDictionary(iText.Kernel.Pdf.PdfName.Names);
        Assert.IsNotNull(namesDict, "Catalog /Names is missing.");

        var embeddedFilesDict = namesDict.GetAsDictionary(iText.Kernel.Pdf.PdfName.EmbeddedFiles);
        Assert.IsNotNull(embeddedFilesDict, "Catalog /Names/EmbeddedFiles is missing.");

        var namesArray = embeddedFilesDict.GetAsArray(iText.Kernel.Pdf.PdfName.Names);
        Assert.IsNotNull(namesArray, "Catalog /Names/EmbeddedFiles/Names is missing.");
        Assert.IsGreaterThanOrEqualTo(2, namesArray.Size(), "No entries in EmbeddedFiles/Names.");

        // Find XML attachment (prefer factur-x.xml)
        PdfFileSpec? xmlSpec = null;
        string? xmlName = null;

        for (int i = 0; i < namesArray.Size() - 1; i += 2)
        {
            var nameObj = namesArray.Get(i);
            var specObj = namesArray.Get(i + 1);

            var raw = nameObj?.ToString() ?? "";
            var cleaned = raw.Trim('(', ')');

            if (cleaned.Equals("factur-x.xml", StringComparison.OrdinalIgnoreCase) ||
                cleaned.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            {
                xmlName = cleaned;

                // Depending on iText version, either Wrap(...) or WrapFileSpecObject(...)
                xmlSpec = PdfFileSpec.WrapFileSpecObject(specObj);
                break;
            }
        }

        Assert.IsNotNull(xmlSpec, "No embedded XML file found (e.g. factur-x.xml).");
        TestContext.WriteLine($"Embedded XML: {xmlName}");

        // IMPORTANT: GetPdfObject returns PdfDictionary, but typed as PdfObject in some APIs.
        // Cast to PdfDictionary so GetAsName/GetAsDictionary are available. :contentReference[oaicite:1]{index=1}
        var specDictObj = xmlSpec.GetPdfObject();
        Assert.IsTrue(specDictObj is iText.Kernel.Pdf.PdfDictionary, "FileSpec object is not a PdfDictionary.");
        var specDict = (iText.Kernel.Pdf.PdfDictionary)specDictObj;

        // C) /AFRelationship must be /Alternative
        var afRel = specDict.GetAsName(iText.Kernel.Pdf.PdfName.AFRelationship);
        Assert.IsNotNull(afRel, "FileSpec /AFRelationship is missing.");
        Assert.AreEqual(iText.Kernel.Pdf.PdfName.Alternative, afRel, "FileSpec /AFRelationship must be /Alternative.");

        // D) EmbeddedFile stream exists
        var efDict = specDict.GetAsDictionary(iText.Kernel.Pdf.PdfName.EF);
        Assert.IsNotNull(efDict, "FileSpec /EF is missing.");

        var efStream = efDict.GetAsStream(iText.Kernel.Pdf.PdfName.F) ?? efDict.GetAsStream(iText.Kernel.Pdf.PdfName.UF);
        Assert.IsNotNull(efStream, "EmbeddedFile stream missing in /EF (/F or /UF).");


        // E) Checksum check (optional in PDF; validate only if present)
        var paramsDict = efStream.GetAsDictionary(iText.Kernel.Pdf.PdfName.Params);
        if (paramsDict == null)
        {
            TestContext.WriteLine("EmbeddedFile /Params: (missing) -> allowed. Skipping checksum validation.");
        }
        else
        {
            var declared = paramsDict.GetAsString(_pdfNameCheckSum)?.ToUnicodeString()?.Trim();

            // Log what we found
            var declaredSize = paramsDict.GetAsNumber(iText.Kernel.Pdf.PdfName.Size)?.IntValue();
            var declaredModDate = paramsDict.GetAsString(iText.Kernel.Pdf.PdfName.ModDate)?.ToUnicodeString();

            TestContext.WriteLine($"Declared /Params/CheckSum: {(string.IsNullOrWhiteSpace(declared) ? "(missing)" : declared)}");
            TestContext.WriteLine($"Declared /Params/Size    : {(declaredSize.HasValue ? declaredSize.Value.ToString() : "(missing)")}");
            TestContext.WriteLine($"Declared /Params/ModDate : {declaredModDate ?? "(missing)"}");

            if (string.IsNullOrWhiteSpace(declared))
            {
                TestContext.WriteLine("No /Params/CheckSum present -> allowed (optional). Skipping checksum validation.");
            }
            else
            {
                byte[] embeddedBytes = efStream.GetBytes();
                string actual = ComputeMd5Hex(embeddedBytes);

                TestContext.WriteLine($"Actual MD5              : {actual}");
                TestContext.WriteLine($"Embedded bytes length   : {embeddedBytes.Length}");

                Assert.AreEqual(declared, actual, "EmbeddedFile /Params/CheckSum is incorrect.");
            }
        }

        // F) PDF/A OutputIntent must contain ICC stream
        var outputIntents = catalog.GetAsArray(iText.Kernel.Pdf.PdfName.OutputIntents);
        Assert.IsNotNull(outputIntents, "Catalog /OutputIntents is missing (PDF/A requirement).");

        var oi = outputIntents.GetAsDictionary(0);
        Assert.IsNotNull(oi, "First OutputIntent is missing/invalid.");

        var destProfileObj = oi.Get(iText.Kernel.Pdf.PdfName.DestOutputProfile);
        Assert.IsNotNull(destProfileObj, "OutputIntent /DestOutputProfile is missing.");
        Assert.IsTrue(destProfileObj.IsStream(), "DestOutputProfile is not a stream (ICC profile) -> not valid PDF/A.");

        // G) XMP PDF/A marker (robust)
        var xmpMeta = pdf.GetXmpMetadata();
        Assert.IsNotNull(xmpMeta, "XMP metadata is missing.");

        // PDF/A identification namespace
        const string nsPdfAId = "http://www.aiim.org/pdfa/ns/id/";

        // Read pdfaid:part
        var partProp = xmpMeta.GetProperty(nsPdfAId, "part");
        Assert.IsNotNull(partProp, "XMP does not contain pdfaid:part.");

        string partValue = (partProp.GetValue() ?? "").Trim();
        TestContext.WriteLine($"pdfaid:part = {partValue}");

        Assert.AreEqual("3", partValue, "XMP pdfaid:part is not 3 (not PDF/A-3).");

        // Optional: read pdfaid:conformance (B/U/A)
        var confProp = xmpMeta.GetProperty(nsPdfAId, "conformance");
        if (confProp != null)
        {
            string confValue = (confProp.GetValue() ?? "").Trim();
            TestContext.WriteLine($"pdfaid:conformance = {confValue}");
        }
    }

    [TestMethod]
    [DataRow("ZF_Extended__Sammelrechnung_3_Bestellungen_zugferd_pdfa3.pdf")]
    [DataRow("ZF_Extended__Sammelrechnung_3_Bestellungen_itext_pdfa3.pdf")]
    public void EmbeddedFacturX_CheckSum_Should_Match_EmbeddedBytes(string fileNameInExamples)
    {
        // This PDF was produced by InvoicePdfProcessor.SaveToPdfAsync(...)
        string pdfPath = Path.Combine(AppContext.BaseDirectory, "Examples", fileNameInExamples);
        Assert.IsTrue(File.Exists(pdfPath), $"PDF not found: {pdfPath}");

        TestContext.WriteLine($"PDF path: {pdfPath}");
        TestContext.WriteLine($"File size: {new FileInfo(pdfPath).Length} bytes");

        using var reader = new PdfReader(pdfPath);
        using var pdf = new iText.Kernel.Pdf.PdfDocument(reader);

        var catalog = pdf.GetCatalog().GetPdfObject();
        Assert.IsNotNull(catalog, "Catalog is null.");

        // Navigate to /Names/EmbeddedFiles/Names
        var namesDict = catalog.GetAsDictionary(PdfName.Names);
        if (namesDict == null)
        {
            TestContext.WriteLine("Catalog /Names is missing.");
            Assert.Fail("Catalog /Names is missing.");
            return;
        }

        var embeddedFilesDict = namesDict.GetAsDictionary(PdfName.EmbeddedFiles);
        if (embeddedFilesDict == null)
        {
            TestContext.WriteLine("Catalog /Names/EmbeddedFiles is missing.");
            Assert.Fail("Catalog /Names/EmbeddedFiles is missing.");
            return;
        }

        var namesArray = embeddedFilesDict.GetAsArray(PdfName.Names);
        if (namesArray == null)
        {
            TestContext.WriteLine("Catalog /Names/EmbeddedFiles/Names is missing.");
            Assert.Fail("Catalog /Names/EmbeddedFiles/Names is missing.");
            return;
        }

        if (namesArray.Size() < 2)
        {
            TestContext.WriteLine("EmbeddedFiles/Names array exists but is empty.");
            Assert.Fail("No embedded files in PDF.");
            return;
        }

        // Log ALL embedded file names first (super helpful)
        TestContext.WriteLine("Embedded files found (Names tree):");
        for (int i = 0; i < namesArray.Size() - 1; i += 2)
        {
            var n = namesArray.GetAsString(i)?.ToUnicodeString() ?? "(null)";
            TestContext.WriteLine($"  - {n}");
        }

        // Find factur-x.xml (case-insensitive)
        PdfDictionary? fileSpecDict = null;
        string? matchedName = null;

        for (int i = 0; i < namesArray.Size() - 1; i += 2)
        {
            var name = namesArray.GetAsString(i)?.ToUnicodeString();
            if (name == null) continue;

            if (string.Equals(name, "factur-x.xml", StringComparison.OrdinalIgnoreCase))
            {
                fileSpecDict = namesArray.GetAsDictionary(i + 1);
                matchedName = name;
                break;
            }
        }

        Assert.IsNotNull(fileSpecDict, "factur-x.xml not found in EmbeddedFiles name tree.");
        TestContext.WriteLine($"Matched embedded XML: {matchedName}");

        // FileSpec details
        LogFileSpecBasics(fileSpecDict);

        // EF stream
        var efDict = fileSpecDict!.GetAsDictionary(PdfName.EF);
        Assert.IsNotNull(efDict, "FileSpec /EF is missing.");

        var efStream = efDict!.GetAsStream(PdfName.F) ?? efDict.GetAsStream(PdfName.UF);
        Assert.IsNotNull(efStream, "EmbeddedFile stream missing (/EF /F or /UF).");

        // Params & checksum
        var paramsDict = efStream!.GetAsDictionary(PdfName.Params);
        if (paramsDict == null)
        {
            TestContext.WriteLine("EmbeddedFile stream has no /Params dictionary.");
            Assert.Fail("EmbeddedFile /Params missing (cannot read /CheckSum).");
            return;
        }

        var declared = paramsDict.GetAsString(new PdfName("CheckSum"))?.ToUnicodeString();
        TestContext.WriteLine($"Declared /Params/CheckSum: {declared ?? "(missing)"}");

        // Also log Size/ModDate if present
        var declaredSize = paramsDict.GetAsNumber(PdfName.Size)?.IntValue();
        var declaredModDate = paramsDict.GetAsString(PdfName.ModDate)?.ToUnicodeString();
        TestContext.WriteLine($"Declared /Params/Size: {declaredSize?.ToString() ?? "(missing)"}");
        TestContext.WriteLine($"Declared /Params/ModDate: {declaredModDate ?? "(missing)"}");

        //checksum
        Assert.IsFalse(string.IsNullOrWhiteSpace(declared), "No /Params/CheckSum present.");

        // Compute actual checksum from embedded bytes
        byte[] embeddedBytes = efStream.GetBytes();
        string actual = ComputeMd5Hex(embeddedBytes);

        TestContext.WriteLine($"Actual MD5: {actual}");
        TestContext.WriteLine($"Embedded bytes length: {embeddedBytes.Length}");

        // Helpful: show first few bytes to prove it's not empty / see XML header
        var preview = PreviewAscii(embeddedBytes, 200);
        TestContext.WriteLine("Embedded content preview (first 200 chars, best-effort):");
        TestContext.WriteLine(preview);

        Assert.AreEqual(declared, actual, "EmbeddedFile /Params/CheckSum does not match the embedded bytes.");
    }

    #region Utilities
    private static readonly iText.Kernel.Pdf.PdfName _pdfNameCheckSum = new iText.Kernel.Pdf.PdfName("CheckSum");

    public static void CreatePdfA3WithEmbeddedFacturXXml(string inputPdfPath, string inputXmlPath, string iccProfilePath, string outputPdfPath)
    {
        if (string.IsNullOrWhiteSpace(inputPdfPath))
            throw new ArgumentException("inputPdfPath is empty.", nameof(inputPdfPath));

        if (string.IsNullOrWhiteSpace(inputXmlPath))
            throw new ArgumentException("inputXmlPath is empty.", nameof(inputXmlPath));

        if (string.IsNullOrWhiteSpace(iccProfilePath))
            throw new ArgumentException("iccProfilePath is empty.", nameof(iccProfilePath));

        if (string.IsNullOrWhiteSpace(outputPdfPath))
            throw new ArgumentException("outputPdfPath is empty.", nameof(outputPdfPath));

        if (!File.Exists(inputPdfPath))
            throw new FileNotFoundException("Input PDF not found.", inputPdfPath);

        if (!File.Exists(inputXmlPath))
            throw new FileNotFoundException("Input XML not found.", inputXmlPath);

        if (!File.Exists(iccProfilePath))
            throw new FileNotFoundException("ICC profile not found.", iccProfilePath);

        byte[] iccBytes = File.ReadAllBytes(iccProfilePath);
        byte[] xmlBytes = File.ReadAllBytes(inputXmlPath);

        Directory.CreateDirectory(Path.GetDirectoryName(outputPdfPath)!);

        using var srcPdf = new PdfDocument(new PdfReader(inputPdfPath));
        using var writer = new PdfWriter(outputPdfPath);

        // PDF/A requires an OutputIntent with an ICC profile stream
        var outputIntent = new PdfOutputIntent("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", new MemoryStream(iccBytes));

        using var pdfa = new PdfADocument(writer, PdfAConformance.PDF_A_3B, outputIntent);

        // Copy all pages from the source PDF into the new PDF/A document
        srcPdf.CopyPagesTo(1, srcPdf.GetNumberOfPages(), pdfa);

        // Factur-X/ZUGFeRD standard attachment name
        const string embeddedName = "factur-x.xml";

        // Create the embedded file spec as an Associated File (AFRelationship = Alternative)
        PdfFileSpec fs = PdfFileSpec.CreateEmbeddedFileSpec(pdfa, xmlBytes, "ZUGFeRD/Factur-X invoice XML", embeddedName, PdfName.Alternative);

        // Add attachment to the document
        pdfa.AddFileAttachment(embeddedName, fs);

        // Ensure the catalog has an /AF array and contains this file spec
        EnsureCatalogAssociatedFile(pdfa, fs);

        // Ensure the embedded file stream has a MIME subtype
        TrySetEmbeddedFileMimeSubtype(fs, "application/xml");

        // Ensure the checksum
        EnsureEmbeddedFileParamsChecksum(fs);

        // Write Factur-X + ZUGFeRD XMP markers (helps Acrobat recognition)
        SetFacturXAndZugferdXmp(pdfa, documentFileName: embeddedName, documentType: "INVOICE", conformanceLevel: "EXTENDED", version: "1.0");

        // No manual Close() needed; using will close properly
    }

    private static void EnsureCatalogAssociatedFile(PdfDocument pdf, PdfFileSpec fileSpec)
    {
        var catalogDict = pdf.GetCatalog().GetPdfObject();

        var afArray = catalogDict.GetAsArray(PdfName.AF);
        if (afArray == null)
        {
            afArray = new PdfArray();
            catalogDict.Put(PdfName.AF, afArray);
        }

        afArray.Add(fileSpec.GetPdfObject());
    }

    private static void TrySetEmbeddedFileMimeSubtype(PdfFileSpec fileSpec, string mimeType)
    {
        // fileSpec.GetPdfObject() is a PdfObject in some iText builds; we must cast to PdfDictionary
        var fsDict = fileSpec.GetPdfObject() as PdfDictionary;
        if (fsDict == null) return;

        var efDict = fsDict.GetAsDictionary(PdfName.EF);
        if (efDict == null) return;

        var efStream = efDict.GetAsStream(PdfName.F) ?? efDict.GetAsStream(PdfName.UF);
        if (efStream == null) return;

        // /Subtype in the EmbeddedFile stream is commonly set to "application/xml"
        efStream.Put(PdfName.Subtype, new PdfName(mimeType));
    }

    private static void SetFacturXAndZugferdXmp(PdfDocument pdf, string documentFileName, string documentType, string conformanceLevel, string version)
    {
        // Factur-X namespace used widely for Acrobat recognition
        const string nsFx = "urn:factur-x:pdfa:CrossIndustryDocument:invoice:1p0#";
        const string prefixFx = "fx";

        // ZUGFeRD namespace used by some tools
        const string nsZf = "urn:ferd:pdfa:CrossIndustryDocument:invoice:1p0#";
        const string prefixZf = "zf";

        // PDF/A extension schema namespaces (required to "declare" custom XMP properties)
        const string nsPdfaExt = "http://www.aiim.org/pdfa/ns/extension/";
        const string nsPdfaSchema = "http://www.aiim.org/pdfa/ns/schema#";
        const string nsPdfaProp = "http://www.aiim.org/pdfa/ns/property#";

        // Get existing XMP or create new
        XMPMeta xmp = pdf.GetXmpMetadata() ?? XMPMetaFactory.Create();

        // Register namespaces
        var registry = XMPMetaFactory.GetSchemaRegistry();
        registry.RegisterNamespace(nsFx, prefixFx);
        registry.RegisterNamespace(nsZf, prefixZf);
        registry.RegisterNamespace(nsPdfaExt, "pdfaExtension");
        registry.RegisterNamespace(nsPdfaSchema, "pdfaSchema");
        registry.RegisterNamespace(nsPdfaProp, "pdfaProperty");

        // 1) Ensure PDF/A Extension Schema description for Factur-X exists
        AppendFacturXExtensionSchemaIfMissing(xmp);

        // 2) Set Factur-X properties
        xmp.SetProperty(nsFx, "DocumentFileName", documentFileName);
        xmp.SetProperty(nsFx, "DocumentType", documentType);
        xmp.SetProperty(nsFx, "ConformanceLevel", conformanceLevel);
        xmp.SetProperty(nsFx, "Version", version);

        // 3) Set ZUGFeRD-compatible properties (same values)
        xmp.SetProperty(nsZf, "DocumentFileName", documentFileName);
        xmp.SetProperty(nsZf, "DocumentType", documentType);
        xmp.SetProperty(nsZf, "ConformanceLevel", conformanceLevel);
        xmp.SetProperty(nsZf, "Version", version);

        // Write back into the PDF
        pdf.SetXmpMetadata(xmp);
    }

    private static void AppendFacturXExtensionSchemaIfMissing(XMPMeta destXmp)
    {
        // If fx:DocumentFileName exists, we still might be missing the pdfaExtension schema block.
        // We don't try to "detect"; we just append a schema description template (idempotent in practice).

        const string extTemplate = @"<?xpacket begin=""﻿"" id=""W5M0MpCehiHzreSzNTczkc9d""?>
<x:xmpmeta xmlns:x=""adobe:ns:meta/"">
  <rdf:RDF xmlns:rdf=""http://www.w3.org/1999/02/22-rdf-syntax-ns#""
           xmlns:pdfaExtension=""http://www.aiim.org/pdfa/ns/extension/""
           xmlns:pdfaSchema=""http://www.aiim.org/pdfa/ns/schema#""
           xmlns:pdfaProperty=""http://www.aiim.org/pdfa/ns/property#"">
    <rdf:Description rdf:about="""">
      <pdfaExtension:schemas>
        <rdf:Bag>
          <rdf:li rdf:parseType=""Resource"">
            <pdfaSchema:schema>Factur-X PDFA Extension Schema</pdfaSchema:schema>
            <pdfaSchema:namespaceURI>urn:factur-x:pdfa:CrossIndustryDocument:invoice:1p0#</pdfaSchema:namespaceURI>
            <pdfaSchema:prefix>fx</pdfaSchema:prefix>
            <pdfaSchema:property>
              <rdf:Seq>
                <rdf:li rdf:parseType=""Resource"">
                  <pdfaProperty:name>DocumentFileName</pdfaProperty:name>
                  <pdfaProperty:valueType>Text</pdfaProperty:valueType>
                  <pdfaProperty:category>external</pdfaProperty:category>
                  <pdfaProperty:description>Filename of the embedded invoice XML.</pdfaProperty:description>
                </rdf:li>
                <rdf:li rdf:parseType=""Resource"">
                  <pdfaProperty:name>DocumentType</pdfaProperty:name>
                  <pdfaProperty:valueType>Text</pdfaProperty:valueType>
                  <pdfaProperty:category>external</pdfaProperty:category>
                  <pdfaProperty:description>Type of the embedded document (e.g. INVOICE).</pdfaProperty:description>
                </rdf:li>
                <rdf:li rdf:parseType=""Resource"">
                  <pdfaProperty:name>ConformanceLevel</pdfaProperty:name>
                  <pdfaProperty:valueType>Text</pdfaProperty:valueType>
                  <pdfaProperty:category>external</pdfaProperty:category>
                  <pdfaProperty:description>Factur-X/ZUGFeRD profile (e.g. BASIC, EN16931, EXTENDED).</pdfaProperty:description>
                </rdf:li>
                <rdf:li rdf:parseType=""Resource"">
                  <pdfaProperty:name>Version</pdfaProperty:name>
                  <pdfaProperty:valueType>Text</pdfaProperty:valueType>
                  <pdfaProperty:category>external</pdfaProperty:category>
                  <pdfaProperty:description>Factur-X version (e.g. 1.0).</pdfaProperty:description>
                </rdf:li>
              </rdf:Seq>
            </pdfaSchema:property>
          </rdf:li>
        </rdf:Bag>
      </pdfaExtension:schemas>
    </rdf:Description>
  </rdf:RDF>
</x:xmpmeta>
<?xpacket end=""w""?>";

        // Parse template and append into existing XMP
        // AppendProperties(source, dest, doAllProperties, replaceOldValues, deleteEmptyValues)
        var source = XMPMetaFactory.ParseFromString(extTemplate);
        XMPUtils.AppendProperties(source, destXmp, true, false, true);
    }
    private static string ComputeMd5Hex(byte[] data)
    {
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(data);
        return string.Concat(hash.Select(b => b.ToString("x2")));
    }

    private void LogFileSpecBasics(PdfDictionary fileSpecDict)
    {
        // AFRelationship if present
        var afRel = fileSpecDict.GetAsName(PdfName.AFRelationship);
        TestContext.WriteLine($"FileSpec /AFRelationship: {afRel?.GetValue() ?? "(missing)"}");

        // F / UF names if present
        var f = fileSpecDict.GetAsString(PdfName.F)?.ToUnicodeString();
        var uf = fileSpecDict.GetAsString(PdfName.UF)?.ToUnicodeString();
        TestContext.WriteLine($"FileSpec /F: {f ?? "(missing)"}");
        TestContext.WriteLine($"FileSpec /UF: {uf ?? "(missing)"}");
    }

    private static string PreviewAscii(byte[] data, int maxChars)
    {
        if (data == null || data.Length == 0) return "(empty)";

        int len = Math.Min(data.Length, maxChars);
        char[] chars = new char[len];

        for (int i = 0; i < len; i++)
        {
            byte b = data[i];

            // Make it readable: allow common whitespace + ASCII printable range
            if (b == 9 || b == 10 || b == 13) chars[i] = (char)b;       // \t \n \r
            else if (b >= 32 && b <= 126) chars[i] = (char)b;           // printable ASCII
            else chars[i] = '.';                                         // non-printable
        }

        return new string(chars);
    }

    private static void EnsureEmbeddedFileParamsChecksum(PdfFileSpec fileSpec)
    {
        var fsDict = fileSpec.GetPdfObject() as PdfDictionary;
        if (fsDict == null) return;

        var efDict = fsDict.GetAsDictionary(PdfName.EF);
        if (efDict == null) return;

        var efStream = efDict.GetAsStream(PdfName.F) ?? efDict.GetAsStream(PdfName.UF);
        if (efStream == null) return;

        // Read embedded bytes
        byte[] embeddedBytes = efStream.GetBytes();

        // Ensure /Params exists
        var paramsDict = efStream.GetAsDictionary(PdfName.Params) ?? new PdfDictionary();

        // Update Size + ModDate (optional but good)
        paramsDict.Put(PdfName.Size, new PdfNumber(embeddedBytes.Length));
        paramsDict.Put(PdfName.ModDate, new PdfDate(DateTime.UtcNow).GetPdfObject());

        // Compute MD5 checksum (lowercase hex)
        byte[] md5 = MD5.HashData(embeddedBytes);
        string md5Hex = string.Concat(md5.Select(b => b.ToString("x2")));

        paramsDict.Put(new PdfName("CheckSum"), new PdfString(md5Hex));

        efStream.Put(PdfName.Params, paramsDict);
    }

    private void OpenWithDefaultPdfViewer(string pdfPath)
    {
        var processInfo = new ProcessStartInfo(pdfPath)
        {
            UseShellExecute = true
        };
        Process.Start(processInfo);
    }
    #endregion
}
