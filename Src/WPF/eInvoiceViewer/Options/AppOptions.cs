namespace tulo.eInvoice.eInvoiceViewer.Options;
internal class AppOptions : IAppOptions
{
    public LocalizationOptions Localization { get; set; } = new();
}

public sealed class LocalizationOptions
{
    public string DefaultCulture { get; set; } = "de-DE";
    public string[] SupportedCultures { get; set; } = Array.Empty<string>();
}
