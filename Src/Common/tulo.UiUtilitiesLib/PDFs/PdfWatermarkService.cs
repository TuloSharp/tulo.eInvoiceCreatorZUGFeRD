using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace tulo.UiUtilitiesLib.PDFs;

public class PdfWatermarkService : IPdfWatermarkService
{
    public MemoryStream AddWatermark(Stream pdfStream, string watermarkText)
    {
        PdfDocument input = PdfReader.Open(pdfStream, PdfDocumentOpenMode.Import);
        return AddWatermarkInternal(input, watermarkText);
    }

    public MemoryStream AddWatermark(MemoryStream pdfMemoryStream, string watermarkText)
    {
        pdfMemoryStream.Position = 0;
        PdfDocument input = PdfReader.Open(pdfMemoryStream, PdfDocumentOpenMode.Import);
        return AddWatermarkInternal(input, watermarkText);
    }

    private MemoryStream AddWatermarkInternal(PdfDocument inputPdfDocument, string watermarkText)
    {
        PdfDocument outPutPdfDocument = new();
        outPutPdfDocument.Version = inputPdfDocument.Version;

        foreach (PdfPage pdfPage in inputPdfDocument.Pages)
        {
            PdfPage newPdfPage = outPutPdfDocument.AddPage(pdfPage);

            using (XGraphics xGraphics = XGraphics.FromPdfPage(newPdfPage, XGraphicsPdfPageOptions.Prepend))
            {
                double w = newPdfPage.Width.Point;
                double h = newPdfPage.Height.Point;

                double fontSize = Math.Min(w, h) / 4.0;
                XFont MakeFont(double size)
                {
                    try { return new XFont("Arial", size, XFontStyleEx.Bold); }
                    catch { return new XFont("Helvetica", size, XFontStyleEx.Bold); }
                }

                XFont font = MakeFont(fontSize);

                double maxWidth = Math.Sqrt((w * w) + (h * h)) * 0.85; // 85% der Diagonale
                XSize textSize = xGraphics.MeasureString(watermarkText, font);

                while (textSize.Width > maxWidth && fontSize > 8)
                {
                    fontSize -= 2;
                    font = MakeFont(fontSize);
                    textSize = xGraphics.MeasureString(watermarkText, font);
                }

                XBrush brush = new XSolidBrush(XColor.FromArgb(20, 0, 0, 0));

                xGraphics.Save();
                xGraphics.TranslateTransform(w / 2, h / 2);
                xGraphics.RotateTransform(-60);

                xGraphics.DrawString(watermarkText, font, brush, 0, 0, XStringFormats.Center);
                xGraphics.Restore();
            }
        }

        MemoryStream memoryStream = new MemoryStream();
        outPutPdfDocument.Save(memoryStream, false);
        memoryStream.Position = 0;
        return memoryStream;
    }
}

//to use
//    IPdfWatermarkService watermarckService = new PdfWatermarkService();

//Variant 1: PDF from file
//using (FileStream fs = File.OpenRead("input.pdf"))
//{
//    MemoryStream watermarked = watermarckService.AddWatermark(fs, "COPY");
//}

//Variant 2: PDF from MemoryStream
//MemoryStream originalPdf = GetPdfFromSomewhere(); // z.B. Netzwerk oder vorherige Generierung
//MemoryStream watermarkedPdf = watermarckService.AddWatermark(originalPdf, "VERTRAULICH");
