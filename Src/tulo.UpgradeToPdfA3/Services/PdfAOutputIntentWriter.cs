using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using tulo.UpgradeToPdfA3.Interfaces;
using tulo.UpgradeToPdfA3.Options;
using tulo.UpgradeToPdfA3.ResultPattern;

namespace tulo.UpgradeToPdfA3.Services;

public sealed class PdfAOutputIntentWriter : IPdfAOutputIntentWriter
{
    public OperationResult Write(PdfDocument pdfDocument, IAppOptions appOptions)
    {
        try
        {
            if (pdfDocument == null)
                return OperationResult.Fail("PDF document is required.");

            string iccProfilePath = appOptions.PdfA.IccProfilePath;
            if (string.IsNullOrWhiteSpace(iccProfilePath) || !File.Exists(iccProfilePath))
                return OperationResult.Fail($"ICC profile was not found: {iccProfilePath}");

            byte[] iccBytes = File.ReadAllBytes(iccProfilePath);

            PdfDictionary iccProfileStream = new PdfDictionary(pdfDocument);
            iccProfileStream.CreateStream(iccBytes);
            iccProfileStream.Elements["/N"] = new PdfInteger(3);

            pdfDocument.Internals.AddObject(iccProfileStream);
            PdfReference iccProfileReference = iccProfileStream.Reference!;

            PdfDictionary outputIntent = new PdfDictionary(pdfDocument);
            outputIntent.Elements["/Type"] = new PdfName("/OutputIntent");

            // Keep this consistent for the current implementation.
            // The writer currently only provides the classic PDF/A output intent entry.
            outputIntent.Elements["/S"] = new PdfName("/GTS_PDFA1");

            outputIntent.Elements["/OutputConditionIdentifier"] = new PdfString("sRGB IEC61966-2.1");
            outputIntent.Elements["/Info"] = new PdfString("sRGB IEC61966-2.1");
            outputIntent.Elements["/DestOutputProfile"] = iccProfileReference;

            PdfArray outputIntents = new PdfArray(pdfDocument);
            outputIntents.Elements.Add(outputIntent);

            pdfDocument.Internals.Catalog.Elements["/OutputIntents"] = outputIntents;

            return OperationResult.Ok();
        }
        catch (Exception ex)
        {
            return OperationResult.Fail($"Failed to write output intent: {ex.Message}");
        }
    }
}
