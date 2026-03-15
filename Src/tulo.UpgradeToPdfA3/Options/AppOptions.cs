namespace tulo.UpgradeToPdfA3.Options
{
    public class AppOptions : IAppOptions
    {
        public PdfAOptions PdfA { get; set; } = new();
    }

    public class PdfAOptions
    {
        public string IccProfilePath { get; set; } = string.Empty;

        public string Creator { get; set; } = "tulo.UpgradeToPdfA3";
        public string CreatorTool { get; set; } = "tulo.UpgradeToPdfA3";
        public string Producer { get; set; } = "PdfSharp Extended";

        public string Title { get; set; } = "E-Invoice";
        public string Description { get; set; } = "E-Invoice as PDF/A-3";
        public string Author { get; set; } = "tulo.UpgradeToPdfA3";
        public string Language { get; set; } = "de-DE";

        public string Conformance { get; set; } = "B";
        public int Part { get; set; } = 3;

        public string AttachmentDescription { get; set; } = "Factur-X / ZUGFeRD invoice XML";
        public string AfRelationship { get; set; } = "Alternative";

        public string DocumentType { get; set; } = "INVOICE";
        public string FacturXVersion { get; set; } = "1.0";
        public string ConformanceLevel { get; set; } = "EXTENDED";
    }
}
