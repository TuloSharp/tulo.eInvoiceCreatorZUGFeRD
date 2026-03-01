namespace tulo.eInvoice.eInvoiceViewer.Utilities;
public sealed class StartupFileContext : IStartupFileContext
{
    public string? FilePath { get; }

    public StartupFileContext(string? filePath)
    {
        FilePath = filePath;
    }
}
