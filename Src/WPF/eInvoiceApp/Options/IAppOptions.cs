namespace tulo.eInvoice.eInvoiceApp.Options;
public interface IAppOptions
{
    LocalizationOptions Localization { get; set; }
    InvoiceOptions Invoice { get; set; }
    ArchiveOptions Archive { get; set; }
    VatsOptions Vats { get; set; }
}
