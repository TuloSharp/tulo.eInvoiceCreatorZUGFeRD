namespace tulo.XMLeInvoiceToPdf.Options;
public class AppOptions : IAppOptions
{
    public PdfAOptions PdfA { get; set; } = new();

    public class PdfAOptions
    {
        public string IccProfilePath { get; set; } = string.Empty;
        public string Creator { get; set; } = "tulo.XMLeInvoiceToPdf";
        public string CreatorTool { get; set; } = "tulo.XMLeInvoiceToPdf";
        public string Producer { get; set; } = "PdfSharp";
        public string Title { get; set; } = "Invoice";
        public string Description { get; set; } = "Invoice document";
        public string Author { get; set; } = "tulo.XMLeInvoiceToPdf";
        public string Language { get; set; } = "de-DE";

        /// <summary>
        /// PDF/A conformance target. For now keep this at "B".
        /// </summary>
        public string Conformance { get; set; } = "B";

        /// <summary>
        /// PDF/A part target. For now keep this at 2 if you want archive-PDF without attachments.
        /// </summary>
        public int Part { get; set; } = 2;

        /// <summary>
        /// Description of the embedded XML attachment.
        /// </summary>
        public string AttachmentDescription { get; set; } = "Factur-X / ZUGFeRD invoice XML";

        /// <summary>
        /// AFRelationship value, usually Alternative.
        /// </summary>
        public string AfRelationship { get; set; } = "Alternative";

        /// <summary>
        /// Factur-X / ZUGFeRD document type.
        /// </summary>
        public string DocumentType { get; set; } = "INVOICE";

        /// <summary>
        /// Factur-X / ZUGFeRD version.
        /// </summary>
        public string FacturXVersion { get; set; } = "1.0";

        /// <summary>
        /// Factur-X / ZUGFeRD profile, e.g. EN 16931.
        /// </summary>
        public string ConformanceLevel { get; set; } = "EN 16931";
    }
}
