using Microsoft.Extensions.Configuration;

namespace tulo.eInvoice.eInvoiceViewer.Options;
internal class AppOptions : IAppOptions
{
    public LanguageOptions Language { get; set; } = new();
}

public sealed class LanguageOptions
{
    public string Culture { get; set; } = string.Empty;
}
