using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.IO;
using System.Security.Cryptography;
using System.Text;
using tulo.XMLeInvoiceToPdf.Options;
using tulo.XMLeInvoiceToPdf.ResultPattern;

namespace tulo.XMLeInvoiceToPdf.Services;
public class ToPdfA3UpgradeService : IToPdfA3UpgradeService
{
    public OperationResult UpgradeToPdfA3(string inputPdfAPath, string outputPdfA3Path, string xmlFileName, byte[] xmlBytes, IAppOptions appOptions)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(inputPdfAPath))
                return OperationResult.Fail("Input PDF/A path is required.");

            if (!File.Exists(inputPdfAPath))
                return OperationResult.Fail($"Input PDF/A file not found: {inputPdfAPath}");

            if (string.IsNullOrWhiteSpace(outputPdfA3Path))
                return OperationResult.Fail("Output PDF/A-3 path is required.");

            if (string.IsNullOrWhiteSpace(xmlFileName))
                return OperationResult.Fail("XML file name is required.");

            if (xmlBytes is null || xmlBytes.Length == 0)
                return OperationResult.Fail("XML content is required.");

            if (appOptions is null)
                return OperationResult.Fail("PDF/A options are required.");

            using PdfDocument document = PdfReader.Open(inputPdfAPath, PdfDocumentOpenMode.Modify);

            PdfReference fileSpecRef = AddEmbeddedXml(document, xmlFileName, xmlBytes, appOptions);
            AddAssociatedFileEntry(document, fileSpecRef);
            AddOrReplacePdfA3Metadata(document, xmlFileName, appOptions);

            document.Save(outputPdfA3Path);

            return OperationResult.Ok($"PDF/A-3 created successfully: {outputPdfA3Path}");
        }
        catch (Exception ex)
        {
            return OperationResult.Fail($"Failed to upgrade PDF/A to PDF/A-3: {ex.Message}");
        }
    }

    private PdfReference AddEmbeddedXml(PdfDocument document, string xmlFileName, byte[] xmlBytes, IAppOptions appOptions)
    {
        PdfDictionary embeddedFileStream = new PdfDictionary(document);
        embeddedFileStream.Elements["/Type"] = new PdfName("/EmbeddedFile");
        embeddedFileStream.Elements["/Subtype"] = new PdfName("/text#2Fxml");
        embeddedFileStream.Elements["/Params"] = BuildEmbeddedFileParams(document, xmlBytes);

        embeddedFileStream.CreateStream(xmlBytes);

        document.Internals.AddObject(embeddedFileStream);
        PdfReference embeddedFileRef = embeddedFileStream.Reference!;

        PdfDictionary fileSpec = new PdfDictionary(document);
        fileSpec.Elements["/Type"] = new PdfName("/Filespec");
        fileSpec.Elements["/F"] = new PdfString(xmlFileName);
        fileSpec.Elements["/UF"] = new PdfString(xmlFileName, PdfStringEncoding.Unicode);
        fileSpec.Elements["/Desc"] = new PdfString(appOptions.PdfA.AttachmentDescription);
        fileSpec.Elements["/AFRelationship"] = new PdfName("/" + appOptions.PdfA.AfRelationship);

        PdfDictionary efDictionary = new PdfDictionary(document);
        efDictionary.Elements["/F"] = embeddedFileRef;
        efDictionary.Elements["/UF"] = embeddedFileRef;
        fileSpec.Elements["/EF"] = efDictionary;

        document.Internals.AddObject(fileSpec);
        PdfReference fileSpecRef = fileSpec.Reference!;

        PdfDictionary? names = document.Internals.Catalog.Elements.GetDictionary("/Names");
        if (names == null)
        {
            names = new PdfDictionary(document);
            document.Internals.Catalog.Elements["/Names"] = names;
        }

        PdfDictionary? embeddedFiles = names.Elements.GetDictionary("/EmbeddedFiles");
        if (embeddedFiles == null)
        {
            embeddedFiles = new PdfDictionary(document);
            names.Elements["/EmbeddedFiles"] = embeddedFiles;
        }

        PdfArray namesArray = new PdfArray(document);
        namesArray.Elements.Add(new PdfString(xmlFileName, PdfStringEncoding.Unicode));
        namesArray.Elements.Add(fileSpecRef);

        embeddedFiles.Elements["/Names"] = namesArray;

        return fileSpecRef;
    }

    private static PdfDictionary BuildEmbeddedFileParams(PdfDocument document, byte[] xmlBytes)
    {
        PdfDictionary parameters = new PdfDictionary(document);
        parameters.Elements["/Size"] = new PdfInteger(xmlBytes.Length);
        parameters.Elements["/ModDate"] = new PdfString(ToPdfDate(DateTimeOffset.Now));

        byte[] md5 = MD5.HashData(xmlBytes);

        // Manche PdfSharp-Versionen mögen hier keinen byte[]-Ctor
        string md5Hex = BitConverter.ToString(md5).Replace("-", "").ToLowerInvariant();
        parameters.Elements["/CheckSum"] = new PdfString(md5Hex);

        return parameters;
    }

    private static void AddAssociatedFileEntry(PdfDocument document, PdfReference fileSpecRef)
    {
        PdfArray afArray = new PdfArray(document);
        afArray.Elements.Add(fileSpecRef);
        document.Internals.Catalog.Elements["/AF"] = afArray;
    }

    private static void AddOrReplacePdfA3Metadata(PdfDocument document, string xmlFileName, IAppOptions appOptions)
    {
        string title = EscapeXml(appOptions.PdfA.Title);
        string author = EscapeXml(appOptions.PdfA.Author);
        string creatorTool = EscapeXml(appOptions.PdfA.CreatorTool);
        string producer = EscapeXml(appOptions.PdfA.Producer);
        string description = EscapeXml(appOptions.PdfA.Description);
        string language = EscapeXml(appOptions.PdfA.Language);
        string documentType = EscapeXml(appOptions.PdfA.DocumentType);
        string facturXVersion = EscapeXml(appOptions.PdfA.FacturXVersion);
        string conformanceLevel = EscapeXml(appOptions.PdfA.ConformanceLevel);
        string escapedXmlFileName = EscapeXml(xmlFileName);
        string now = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

        string xmp = $"""
<?xpacket begin="﻿" id="W5M0MpCehiHzreSzNTczkc9d"?>
<x:xmpmeta xmlns:x="adobe:ns:meta/">
  <rdf:RDF xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#">
    <rdf:Description rdf:about="" xmlns:pdfaid="http://www.aiim.org/pdfa/ns/id/">
      <pdfaid:part>3</pdfaid:part>
      <pdfaid:conformance>{EscapeXml(appOptions.PdfA.Conformance)}</pdfaid:conformance>
    </rdf:Description>

    <rdf:Description rdf:about="" xmlns:dc="http://purl.org/dc/elements/1.1/">
      <dc:title>
        <rdf:Alt>
          <rdf:li xml:lang="x-default">{title}</rdf:li>
        </rdf:Alt>
      </dc:title>
      <dc:creator>
        <rdf:Seq>
          <rdf:li>{author}</rdf:li>
        </rdf:Seq>
      </dc:creator>
      <dc:description>
        <rdf:Alt>
          <rdf:li xml:lang="x-default">{description}</rdf:li>
        </rdf:Alt>
      </dc:description>
      <dc:language>
        <rdf:Bag>
          <rdf:li>{language}</rdf:li>
        </rdf:Bag>
      </dc:language>
    </rdf:Description>

    <rdf:Description rdf:about="" xmlns:pdf="http://ns.adobe.com/pdf/1.3/">
      <pdf:Producer>{producer}</pdf:Producer>
    </rdf:Description>

    <rdf:Description rdf:about="" xmlns:xmp="http://ns.adobe.com/xap/1.0/">
      <xmp:CreatorTool>{creatorTool}</xmp:CreatorTool>
      <xmp:CreateDate>{now}</xmp:CreateDate>
      <xmp:ModifyDate>{now}</xmp:ModifyDate>
      <xmp:MetadataDate>{now}</xmp:MetadataDate>
    </rdf:Description>

    <rdf:Description rdf:about="" xmlns:fx="urn:factur-x:pdfa:CrossIndustryDocument:invoice:1p0#">
      <fx:DocumentType>{documentType}</fx:DocumentType>
      <fx:DocumentFileName>{escapedXmlFileName}</fx:DocumentFileName>
      <fx:Version>{facturXVersion}</fx:Version>
      <fx:ConformanceLevel>{conformanceLevel}</fx:ConformanceLevel>
    </rdf:Description>
  </rdf:RDF>
</x:xmpmeta>
<?xpacket end="w"?>
""";

        PdfDictionary metadata = new PdfDictionary(document);
        metadata.Elements["/Type"] = new PdfName("/Metadata");
        metadata.Elements["/Subtype"] = new PdfName("/XML");

        metadata.CreateStream(Encoding.UTF8.GetBytes(xmp));

        document.Internals.AddObject(metadata);
        PdfReference metadataRef = metadata.Reference!;

        document.Internals.Catalog.Elements["/Metadata"] = metadataRef;
    }

    private static string ToPdfDate(DateTimeOffset value)
    {
        string sign = value.Offset < TimeSpan.Zero ? "-" : "+";
        TimeSpan offset = value.Offset.Duration();
        return $"D:{value:yyyyMMddHHmmss}{sign}{offset.Hours:00}'{offset.Minutes:00}'";
    }

    private static string EscapeXml(string? value)
    {
        return (value ?? string.Empty)
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }
}