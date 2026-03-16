using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using System.Security.Cryptography;
using tulo.UpgradeToPdfA3.Interfaces;
using tulo.UpgradeToPdfA3.Options;
using tulo.UpgradeToPdfA3.ResultPattern;

namespace tulo.UpgradeToPdfA3.Services;

public sealed class PdfA3AttachmentWriter : IPdfA3AttachmentWriter
{
    public OperationResult AddXmlAttachment(PdfDocument document, string xmlFileName, byte[] xmlBytes, IUpgradeToPdfA3Options appOptions)
    {
        try
        {
            PdfDictionary embeddedFileStream = new PdfDictionary(document);
            embeddedFileStream.Elements["/Type"] = new PdfName("/EmbeddedFile");
            embeddedFileStream.Elements["/Subtype"] = new PdfName("/text/xml");
            embeddedFileStream.Elements["/Params"] = BuildEmbeddedFileParams(document, xmlBytes);
            embeddedFileStream.CreateStream(xmlBytes);

            document.Internals.AddObject(embeddedFileStream);
            PdfReference embeddedFileReference = embeddedFileStream.Reference!;

            PdfDictionary fileSpecification = new PdfDictionary(document);
            fileSpecification.Elements["/Type"] = new PdfName("/Filespec");
            fileSpecification.Elements["/F"] = new PdfString(xmlFileName);
            fileSpecification.Elements["/UF"] = new PdfString(xmlFileName, PdfStringEncoding.Unicode);
            fileSpecification.Elements["/Desc"] = new PdfString(appOptions.PdfA3.AttachmentDescription);
            fileSpecification.Elements["/AFRelationship"] = new PdfName("/" + appOptions.PdfA3.AfRelationship);

            PdfDictionary embeddedFileDictionary = new PdfDictionary(document);
            embeddedFileDictionary.Elements["/F"] = embeddedFileReference;
            embeddedFileDictionary.Elements["/UF"] = embeddedFileReference;
            fileSpecification.Elements["/EF"] = embeddedFileDictionary;

            document.Internals.AddObject(fileSpecification);
            PdfReference fileSpecificationReference = fileSpecification.Reference!;

            PdfArray namesArray = new PdfArray(document);
            namesArray.Elements.Add(new PdfString(xmlFileName));
            namesArray.Elements.Add(fileSpecificationReference);

            PdfDictionary embeddedFilesDictionary = new PdfDictionary(document);
            embeddedFilesDictionary.Elements["/Names"] = namesArray;

            PdfDictionary namesDictionary = new PdfDictionary(document);
            namesDictionary.Elements["/EmbeddedFiles"] = embeddedFilesDictionary;
            document.Internals.Catalog.Elements["/Names"] = namesDictionary;

            PdfArray associatedFilesArray = new PdfArray(document);
            associatedFilesArray.Elements.Add(fileSpecificationReference);
            document.Internals.Catalog.Elements["/AF"] = associatedFilesArray;

            return OperationResult.Ok();
        }
        catch (Exception ex)
        {
            return OperationResult.Fail($"Failed to add XML attachment: {ex.Message}");
        }
    }

    private static PdfDictionary BuildEmbeddedFileParams(PdfDocument document, byte[] xmlBytes)
    {
        PdfDictionary parameters = new PdfDictionary(document);
        parameters.Elements["/Size"] = new PdfInteger(xmlBytes.Length);
        parameters.Elements["/ModDate"] = new PdfString(ToPdfDate(DateTimeOffset.UtcNow));

        byte[] md5 = MD5.HashData(xmlBytes);
        string md5Hex = BitConverter.ToString(md5).Replace("-", "").ToLowerInvariant();
        parameters.Elements["/CheckSum"] = new PdfString(md5Hex);

        return parameters;
    }

    private static string ToPdfDate(DateTimeOffset value)
    {
        string sign = value.Offset < TimeSpan.Zero ? "-" : "+";
        TimeSpan offset = value.Offset.Duration();
        return $"D:{value:yyyyMMddHHmmss}{sign}{offset.Hours:00}'{offset.Minutes:00}'";
    }
}