namespace tulo.UiUtilitiesLib.PDFs
{
    public interface IPdfWatermarkService
    {
        /// <summary>
        /// Adds a text watermark to every page of a PDF from a Stream.
        /// </summary>
        /// <param name="pdfStream">Input PDF as a Stream.</param>
        /// <param name="watermarkText">Text to use for the watermark.</param>
        /// <returns>A MemoryStream containing the PDF with the watermark applied.</returns>
        MemoryStream AddWatermark(Stream pdfStream, string watermarkText);

        /// <summary>
        /// Adds a text watermark to every page of an existing PDF in a MemoryStream.
        /// </summary>
        /// <param name="pdfMemoryStream">Input PDF as a MemoryStream.</param>
        /// <param name="watermarkText">Text to use for the watermark.</param>
        /// <returns>A MemoryStream containing the PDF with the watermark applied.</returns>
        MemoryStream AddWatermark(MemoryStream pdfMemoryStream, string watermarkText);
    }
}
