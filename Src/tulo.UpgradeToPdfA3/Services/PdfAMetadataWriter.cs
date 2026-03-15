using PdfSharp.Pdf;
using System.Text;
using tulo.UpgradeToPdfA3.Interfaces;
using tulo.UpgradeToPdfA3.Options;
using tulo.UpgradeToPdfA3.ResultPattern;

namespace tulo.UpgradeToPdfA3.Services;

public sealed class PdfAMetadataWriter : IPdfAMetadataWriter
{
    public OperationResult WritePdfA(PdfDocument pdfDocument, IAppOptions appOptions)
    {
        try
        {
            string title = EscapeXml(pdfDocument.Info.Title);
            string subject = EscapeXml(pdfDocument.Info.Subject);
            string author = EscapeXml(pdfDocument.Info.Author);
            string creator = EscapeXml(pdfDocument.Info.Creator);
            string producer = EscapeXml(pdfDocument.Info.Producer);
            string keywords = EscapeXml(pdfDocument.Info.Keywords);
            string language = EscapeXml(appOptions.PdfA.Language);
            string now = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

            string template = LoadTemplate("PdfAMetadataTemplate.xml");

            string pdfKeywordsElement = string.IsNullOrWhiteSpace(pdfDocument.Info.Keywords)
                ? string.Empty
                : $"<pdf:Keywords>{keywords}</pdf:Keywords>";

            string xmp = template
                .Replace("{{PdfAPart}}", appOptions.PdfA.Part.ToString())
                .Replace("{{PdfAConformance}}", EscapeXml(appOptions.PdfA.Conformance))
                .Replace("{{DocumentTitle}}", title)
                .Replace("{{DocumentDescription}}", subject)
                .Replace("{{DocumentAuthor}}", author)
                .Replace("{{DocumentLanguage}}", language)
                .Replace("{{Producer}}", producer)
                .Replace("{{PdfKeywordsElement}}", pdfKeywordsElement)
                .Replace("{{CreatorTool}}", creator)
                .Replace("{{CreationDate}}", now)
                .Replace("{{ModificationDate}}", now)
                .Replace("{{MetadataDate}}", now);

            WriteMetadataStream(pdfDocument, xmp);
            return OperationResult.Ok();
        }
        catch (Exception ex)
        {
            return OperationResult.Fail($"Failed to write PDF/A metadata: {ex.Message}");
        }
    }

    public OperationResult WritePdfA3(PdfDocument pdfDocument, string xmlFileName, IAppOptions appOptions)
    {
        try
        {
            string title = EscapeXml(pdfDocument.Info.Title);
            string subject = EscapeXml(pdfDocument.Info.Subject);
            string author = EscapeXml(pdfDocument.Info.Author);
            string creator = EscapeXml(pdfDocument.Info.Creator);
            string producer = EscapeXml(pdfDocument.Info.Producer);
            string keywords = EscapeXml(pdfDocument.Info.Keywords);
            string language = EscapeXml(appOptions.PdfA.Language);
            string documentType = EscapeXml(appOptions.PdfA.DocumentType);
            string facturXVersion = EscapeXml(appOptions.PdfA.FacturXVersion);
            string conformanceLevel = EscapeXml(appOptions.PdfA.ConformanceLevel);
            string escapedXmlFileName = EscapeXml(xmlFileName);
            string now = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

            string template = LoadTemplate("PdfA3MetadataTemplate.xml");

            string pdfKeywordsElement = string.IsNullOrWhiteSpace(pdfDocument.Info.Keywords)
                ? string.Empty
                : $"<pdf:Keywords>{keywords}</pdf:Keywords>";

            string xmp = template
                .Replace("{{PdfAConformance}}", EscapeXml(appOptions.PdfA.Conformance))
                .Replace("{{DocumentTitle}}", title)
                .Replace("{{DocumentDescription}}", subject)
                .Replace("{{DocumentAuthor}}", author)
                .Replace("{{DocumentLanguage}}", language)
                .Replace("{{Producer}}", producer)
                .Replace("{{PdfKeywordsElement}}", pdfKeywordsElement)
                .Replace("{{CreatorTool}}", creator)
                .Replace("{{CreationDate}}", now)
                .Replace("{{ModificationDate}}", now)
                .Replace("{{MetadataDate}}", now)
                .Replace("{{InvoiceFilename}}", escapedXmlFileName)
                .Replace("{{DocumentType}}", documentType)
                .Replace("{{Version}}", facturXVersion)
                .Replace("{{ConformanceLevel}}", conformanceLevel);

            WriteMetadataStream(pdfDocument, xmp);
            return OperationResult.Ok();
        }
        catch (Exception ex)
        {
            return OperationResult.Fail($"Failed to write PDF/A-3 metadata: {ex.Message}");
        }
    }

    private static string LoadTemplate(string fileName)
    {
        string templatePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "Templates",
            fileName);

        if (!File.Exists(templatePath))
            throw new FileNotFoundException("Metadata template file was not found.", templatePath);

        return File.ReadAllText(templatePath, Encoding.UTF8);
    }

    private static void WriteMetadataStream(PdfDocument pdfDocument, string xmp)
    {
        PdfDictionary metadata = new PdfDictionary(pdfDocument);
        metadata.Elements["/Type"] = new PdfName("/Metadata");
        metadata.Elements["/Subtype"] = new PdfName("/XML");
        metadata.CreateStream(Encoding.UTF8.GetBytes(xmp));

        pdfDocument.Internals.AddObject(metadata);
        pdfDocument.Internals.Catalog.Elements["/Metadata"] = metadata.Reference;
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
