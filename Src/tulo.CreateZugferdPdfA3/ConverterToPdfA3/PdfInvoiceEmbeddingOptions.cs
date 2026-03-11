namespace tulo.CreateZugferdPdfA3.ConverterToPdfA3;
public sealed class PdfInvoiceEmbeddingOptions
{
    public required string IccProfilePath { get; init; }
    public required string Creator { get; init; }
    public required string CreatorTool { get; init; }

    public string XmlAttachmentFileName { get; init; } = "factur-x.xml";
    public string DocumentType { get; init; } = "INVOICE";
    public string FacturXVersion { get; init; } = "1.0";
    public string ConformanceLevel { get; init; } = "EXTENDED";
}
