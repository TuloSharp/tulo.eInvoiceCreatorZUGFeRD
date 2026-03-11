using PdfSharp.Pdf;
using System.Text;
using tulo.XMLeInvoiceToPdf.Options;
using tulo.XMLeInvoiceToPdf.ResultPattern;

namespace tulo.XMLeInvoiceToPdf.Services;
public class ToPdfAConverterService : IToPdfAConverterService
{
    public OperationResult ApplyPdfA(PdfDocument pdfDocument, IAppOptions appOptions)
    {
        if (appOptions == null)
            return OperationResult.Fail("App options are missing.");

        if (pdfDocument == null)
            return OperationResult.Fail("PDF document is missing.");

        if (appOptions.PdfA == null)
            return OperationResult.Fail("PdfA options are missing.");

        if (string.IsNullOrWhiteSpace(appOptions.PdfA.IccProfilePath))
            return OperationResult.Fail("ICC profile path is missing.");

        if (!File.Exists(appOptions.PdfA.IccProfilePath))
            return OperationResult.Fail($"ICC profile not found: {appOptions.PdfA.IccProfilePath}");

        if (appOptions.PdfA.Part < 1 || appOptions.PdfA.Part > 3)
            return OperationResult.Fail("PDF/A part must be 1, 2, or 3.");

        if (string.IsNullOrWhiteSpace(appOptions.PdfA.Conformance))
            return OperationResult.Fail("PDF/A conformance is missing.");

        try
        {
            OperationResult infoResult = ApplyDocumentInfo(pdfDocument, appOptions);
            if (!infoResult.Success)
                return infoResult;

            OperationResult languageResult = AddLanguage(pdfDocument, appOptions.PdfA.Language);
            if (!languageResult.Success)
                return languageResult;

            OperationResult metadataResult = AddMetadata(pdfDocument, appOptions);
            if (!metadataResult.Success)
                return metadataResult;

            OperationResult outputIntentResult = AddOutputIntent(pdfDocument, appOptions);
            if (!outputIntentResult.Success)
                return outputIntentResult;

            return OperationResult.Ok("PDF/A metadata applied successfully.");
        }
        catch (Exception ex)
        {
            return OperationResult.Fail($"Failed to apply PDF/A: {ex.Message}");
        }

        //pdfDocument.Options.ManualXmpGeneration = true;

        // Intentionally NO embedded files, NO /AF, NO Factur-X metadata here.
        // This is PDF/A only, not PDF/A-3.
    }

    private OperationResult ApplyDocumentInfo(PdfDocument pdfDocument, IAppOptions options)
    {
        try
        {
            var now = DateTime.UtcNow;
            pdfDocument.Info.Title = options.PdfA.Title ?? string.Empty;
            pdfDocument.Info.Subject = options.PdfA.Description ?? string.Empty;
            pdfDocument.Info.Author = options.PdfA.Author ?? string.Empty;
            pdfDocument.Info.Creator = options.PdfA.Creator ?? string.Empty;
            //pdfDocument.Info.Producer = options.PdfA.Producer ?? string.Empty;
            pdfDocument.Info.CreationDate = now;
            pdfDocument.Info.ModificationDate = now;
            return OperationResult.Ok();
        }
        catch (Exception ex)
        {
            return OperationResult.Fail($"Failed to apply PDF info: {ex.Message}");
        }
    }

    private OperationResult AddLanguage(PdfDocument pdfDocument, string? language)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(language))
                pdfDocument.Internals.Catalog.Elements["/Lang"] = new PdfString(language);

            return OperationResult.Ok();
        }
        catch (Exception ex)
        {
            return OperationResult.Fail($"Failed to set PDF language: {ex.Message}");
        }
    }

    private OperationResult AddMetadata(PdfDocument pdfDocument, IAppOptions appOptions)
    {
        try
        {
            string metadataXml = BuildXmpMetadata(appOptions);
            byte[] metadataBytes = Encoding.UTF8.GetBytes(metadataXml);

            PdfDictionary metadata = new PdfDictionary();
            metadata.CreateStream(metadataBytes);
            metadata.Elements.Add("/Type", new PdfName("/Metadata"));
            metadata.Elements.Add("/Subtype", new PdfName("/XML"));

            pdfDocument.Internals.AddObject(metadata);
            pdfDocument.Internals.Catalog.Elements.Add("/Metadata", metadata.Reference);
            return OperationResult.Ok();
        }
        catch (Exception ex)
        {
            return OperationResult.Fail($"Failed to add XMP metadata: {ex.Message}");
        }
    }

    private string BuildXmpMetadata(IAppOptions appOptions)
    {
        string now = FormatXmpDateTime(DateTimeOffset.UtcNow);

        string title = EscapeXml(appOptions.PdfA.Title ?? string.Empty);
        string description = EscapeXml(appOptions.PdfA.Description ?? string.Empty);
        string author = EscapeXml(appOptions.PdfA.Author ?? string.Empty);
        string creator = EscapeXml(appOptions.PdfA.Creator ?? string.Empty);
        string creatorTool = EscapeXml(appOptions.PdfA.CreatorTool ?? string.Empty);
        string producer = EscapeXml(appOptions.PdfA.Producer ?? string.Empty);
        string lang = EscapeXml(string.IsNullOrWhiteSpace(appOptions.PdfA.Language) ? "x-default" : appOptions.PdfA.Language);
        string conformance = EscapeXml(appOptions.PdfA.Conformance.ToUpperInvariant());

        return
$@"<?xpacket begin=""﻿"" id=""W5M0MpCehiHzreSzNTczkc9d""?>
<x:xmpmeta xmlns:x=""adobe:ns:meta/"" x:xmptk=""{creatorTool}"">
  <rdf:RDF xmlns:rdf=""http://www.w3.org/1999/02/22-rdf-syntax-ns#"">

    <rdf:Description rdf:about=""""
      xmlns:pdfaid=""http://www.aiim.org/pdfa/ns/id/"">
      <pdfaid:part>{appOptions.PdfA.Part}</pdfaid:part>
      <pdfaid:conformance>{conformance}</pdfaid:conformance>
    </rdf:Description>

    <rdf:Description rdf:about=""""
      xmlns:dc=""http://purl.org/dc/elements/1.1/"">
      <dc:title>
        <rdf:Alt>
          <rdf:li xml:lang=""{lang}"">{title}</rdf:li>
        </rdf:Alt>
      </dc:title>
      <dc:description>
        <rdf:Alt>
          <rdf:li xml:lang=""{lang}"">{description}</rdf:li>
        </rdf:Alt>
      </dc:description>
      <dc:creator>
        <rdf:Seq>
          <rdf:li>{author}</rdf:li>
        </rdf:Seq>
      </dc:creator>
    </rdf:Description>

    <rdf:Description rdf:about=""""
      xmlns:xmp=""http://ns.adobe.com/xap/1.0/"">
      <xmp:CreateDate>{now}</xmp:CreateDate>
      <xmp:ModifyDate>{now}</xmp:ModifyDate>
      <xmp:MetadataDate>{now}</xmp:MetadataDate>
      <xmp:CreatorTool>{creatorTool}</xmp:CreatorTool>
    </rdf:Description>

    <rdf:Description rdf:about=""""
      xmlns:pdf=""http://ns.adobe.com/pdf/1.3/"">
      <pdf:Producer>{producer}</pdf:Producer>
      <pdf:Keywords>PDF/A</pdf:Keywords>
    </rdf:Description>

    <rdf:Description rdf:about=""""
      xmlns:xmpMM=""http://ns.adobe.com/xap/1.0/mm/"">
      <xmpMM:DocumentID>uuid:{Guid.NewGuid()}</xmpMM:DocumentID>
      <xmpMM:InstanceID>uuid:{Guid.NewGuid()}</xmpMM:InstanceID>
    </rdf:Description>

  </rdf:RDF>
</x:xmpmeta>
<?xpacket end=""w""?>";
    }

    private OperationResult AddOutputIntent(PdfDocument pdfDocument, IAppOptions appOptions)
    {
        try
        {
            byte[] iccProfileBytes = File.ReadAllBytes(appOptions.PdfA.IccProfilePath);
            if (iccProfileBytes.Length == 0)
                throw new InvalidOperationException("ICC profile is empty.");

            PdfDictionary iccStream = new PdfDictionary();
            iccStream.CreateStream(iccProfileBytes);
            iccStream.Elements.Add("/N", new PdfInteger(3));
            pdfDocument.Internals.AddObject(iccStream);

            PdfDictionary outputIntent = new PdfDictionary();
            outputIntent.Elements.Add("/Type", new PdfName("/OutputIntent"));
            outputIntent.Elements.Add("/S", new PdfName("/GTS_PDFA1"));
            outputIntent.Elements.Add("/OutputConditionIdentifier", new PdfString("sRGB IEC61966-2.1"));
            outputIntent.Elements.Add("/Info", new PdfString("sRGB IEC61966-2.1"));
            outputIntent.Elements.Add("/DestOutputProfile", iccStream.Reference);
            pdfDocument.Internals.AddObject(outputIntent);

            PdfArray outputIntents = new PdfArray();
            outputIntents.Elements.Add(outputIntent.Reference!);
            pdfDocument.Internals.AddObject(outputIntents);

            pdfDocument.Internals.Catalog.Elements.Add("/OutputIntents", outputIntents.Reference);
            return OperationResult.Ok();
        }
        catch (Exception ex)
        {
            return OperationResult.Fail($"Failed to add output intent: {ex.Message}");
        }
    }

    private string FormatXmpDateTime(DateTimeOffset value)
    {
        return value.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
    }

    private string EscapeXml(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }
}
