using tulo.XMLeInvoiceToPdf.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tulo.XMLeInvoiceToPdf.LookupTable;
public class GetEinvoiceServiceByName : IGetEinvoiceServiceByName
{
    private readonly IEnumerable<IPdfGeneratorFromInvoice> _pdfGeneratorFromInvoice;

    /// <inheritdoc />
    public GetEinvoiceServiceByName(IEnumerable<IPdfGeneratorFromInvoice> pdfGeneratorFromInvoice)
    {
        _pdfGeneratorFromInvoice = pdfGeneratorFromInvoice;
    }

    /// <inheritdoc />
    public IPdfGeneratorFromInvoice? GetServiceByName(string name)
    {
        return _pdfGeneratorFromInvoice.FirstOrDefault(sv => string.Equals(sv.Name, name, StringComparison.OrdinalIgnoreCase));
    }
}
