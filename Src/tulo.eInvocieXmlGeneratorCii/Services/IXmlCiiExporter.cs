using Zugferd24.Extended;

namespace tulo.eInvoiceXmlGeneratorCii.Services;

public interface IXmlCiiExporter
{
    string ToXml(CrossIndustryInvoiceType invoice);
}
