using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using tulo.UpgradeToPdfA3.Options;
using tulo.UpgradeToPdfA3.Interfaces;
using tulo.UpgradeToPdfA3.ResultPattern;

namespace tulo.UpgradeToPdfA3.Services;

public sealed class ToPdfA3UpgradeService : IToPdfA3UpgradeService
{
    private const string EmbeddedInvoiceXmlFileName = "factur-x.xml";

    private readonly IPdfA3UpgradeValidator _validator;
    private readonly IPdfA3AttachmentWriter _attachmentWriter;
    private readonly IPdfAMetadataWriter _metadataWriter;

    public ToPdfA3UpgradeService(
        IPdfA3UpgradeValidator validator,
        IPdfA3AttachmentWriter attachmentWriter,
        IPdfAMetadataWriter metadataWriter)
    {
        _validator = validator;
        _attachmentWriter = attachmentWriter;
        _metadataWriter = metadataWriter;
    }

    public OperationResult UpgradeToPdfA3(
        string inputPdfAPath,
        string outputPdfA3Path,
        string xmlFileName,
        byte[] xmlBytes,
        IUpgradeToPdfA3Options appOptions)
    {
        OperationResult validationResult = _validator.Validate(
            inputPdfAPath,
            outputPdfA3Path,
            xmlFileName,
            xmlBytes,
            appOptions);

        if (!validationResult.Success)
            return validationResult;

        try
        {
            using PdfDocument sourceDocument = PdfReader.Open(inputPdfAPath, PdfDocumentOpenMode.Import);
            using PdfDocument outputDocument = new PdfDocument();

            TryEnableManualXmpGeneration(outputDocument);

            for (int i = 0; i < sourceDocument.PageCount; i++)
            {
                outputDocument.AddPage(sourceDocument.Pages[i]);
            }

            CopyDocumentInfo(sourceDocument, outputDocument);
            CopyViewerPreferences(sourceDocument, outputDocument);
            SetLanguage(outputDocument, appOptions.PdfA3.Language);
            AddMarkInfo(outputDocument);
            AddStructTreeRoot(outputDocument);
            AddOutputIntent(outputDocument, appOptions.PdfA3.IccProfilePath);

            string embeddedXmlFileName = EmbeddedInvoiceXmlFileName;

            OperationResult attachmentResult = _attachmentWriter.AddXmlAttachment(
                outputDocument,
                embeddedXmlFileName,
                xmlBytes,
                appOptions);

            if (!attachmentResult.Success)
                return attachmentResult;

            OperationResult metadataResult = _metadataWriter.WritePdfA3(
                outputDocument,
                embeddedXmlFileName,
                appOptions);

            if (!metadataResult.Success)
                return metadataResult;

            outputDocument.Save(outputPdfA3Path);
            return OperationResult.Ok($"PDF/A-3 created successfully: {outputPdfA3Path}");
        }
        catch (Exception ex)
        {
            return OperationResult.Fail($"Failed to upgrade PDF/A to PDF/A-3: {ex.Message}");
        }
    }

    private static void TryEnableManualXmpGeneration(PdfDocument document)
    {
        try
        {
            document.Options.ManualXmpGeneration = true;
        }
        catch
        {
        }
    }

    private static void CopyDocumentInfo(PdfDocument sourceDocument, PdfDocument outputDocument)
    {
        outputDocument.Info.Title = sourceDocument.Info.Title;
        outputDocument.Info.Subject = sourceDocument.Info.Subject;
        outputDocument.Info.Author = sourceDocument.Info.Author;
        outputDocument.Info.Creator = sourceDocument.Info.Creator;
        outputDocument.Info.Keywords = sourceDocument.Info.Keywords;
        outputDocument.Info.CreationDate = sourceDocument.Info.CreationDate;
        outputDocument.Info.ModificationDate = DateTime.UtcNow;
    }

    private static void CopyViewerPreferences(PdfDocument sourceDocument, PdfDocument outputDocument)
    {
        if (sourceDocument.Internals.Catalog.Elements.ContainsKey("/ViewerPreferences"))
        {
            outputDocument.Internals.Catalog.Elements["/ViewerPreferences"] =
                sourceDocument.Internals.Catalog.Elements["/ViewerPreferences"];
        }
    }

    private static void SetLanguage(PdfDocument document, string? language)
    {
        if (!string.IsNullOrWhiteSpace(language))
            document.Internals.Catalog.Elements["/Lang"] = new PdfString(language);
    }

    private static void AddMarkInfo(PdfDocument document)
    {
        PdfDictionary markInfoDictionary = new PdfDictionary(document);
        markInfoDictionary.Elements["/Marked"] = new PdfBoolean(true);
        document.Internals.Catalog.Elements["/MarkInfo"] = markInfoDictionary;
    }

    private static void AddStructTreeRoot(PdfDocument document)
    {
        PdfDictionary structTreeRoot = new PdfDictionary(document);
        structTreeRoot.Elements["/Type"] = new PdfName("/StructTreeRoot");
        document.Internals.Catalog.Elements["/StructTreeRoot"] = structTreeRoot;
    }

    private static void AddOutputIntent(PdfDocument document, string iccProfilePath)
    {
        if (string.IsNullOrWhiteSpace(iccProfilePath) || !File.Exists(iccProfilePath))
            return;

        byte[] iccBytes = File.ReadAllBytes(iccProfilePath);

        PdfDictionary rgbProfileDictionary = new PdfDictionary(document);
        rgbProfileDictionary.CreateStream(iccBytes);
        rgbProfileDictionary.Elements["/N"] = new PdfInteger(3);
        document.Internals.AddObject(rgbProfileDictionary);

        PdfDictionary outputIntentDictionary = new PdfDictionary(document);
        outputIntentDictionary.Elements["/Type"] = new PdfName("/OutputIntent");
        outputIntentDictionary.Elements["/S"] = new PdfName("/GTS_PDFA1");
        outputIntentDictionary.Elements["/OutputConditionIdentifier"] = new PdfString("sRGB IEC61966-2.1");
        outputIntentDictionary.Elements["/Info"] = new PdfString("sRGB IEC61966-2.1");
        outputIntentDictionary.Elements["/DestOutputProfile"] = rgbProfileDictionary.Reference;
        document.Internals.AddObject(outputIntentDictionary);

        PdfArray outputIntentsArray = new PdfArray(document);
        outputIntentsArray.Elements.Add(outputIntentDictionary.Reference!);
        document.Internals.Catalog.Elements["/OutputIntents"] = outputIntentsArray;
    }
}
