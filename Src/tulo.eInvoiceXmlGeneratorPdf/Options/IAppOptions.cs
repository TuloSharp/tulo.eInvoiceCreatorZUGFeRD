using static tulo.XMLeInvoiceToPdf.Options.AppOptions;

namespace tulo.XMLeInvoiceToPdf.Options;

public interface IAppOptions
{
    PdfAOptions PdfA { get; set; }
}