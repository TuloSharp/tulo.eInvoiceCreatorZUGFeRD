using tulo.XMLeInvoiceToPdf.Services;

namespace tulo.XMLeInvoiceToPdf.LookupTable;
public interface IGetEinvoiceServiceByName
{
    IPdfGeneratorFromInvoice? GetServiceByName(string name);
}