using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Zugferd24.Extended;

namespace tulo.eInvoiceXmlGeneratorCii.Services;
public class XmlCiiExporter : IXmlCiiExporter
{
    private readonly IXmlObjectCleaner _cleaner;

    public XmlCiiExporter(IXmlObjectCleaner cleaner)
    {
        _cleaner = cleaner ?? throw new ArgumentNullException(nameof(cleaner));
    }

    public string ToXml(CrossIndustryInvoiceType invoice)
    {
        if (invoice == null) throw new ArgumentNullException(nameof(invoice));

        // Remove empty nodes so that no empty tags are created
        _cleaner.RemoveEmptyNodes(invoice);

        var settings = new XmlWriterSettings
        {
            Encoding = new UTF8Encoding(false),
            Indent = true,
            OmitXmlDeclaration = false
        };

        var serializer = new XmlSerializer(typeof(CrossIndustryInvoiceType));

        // Set prefixes as in the example:
        var ns = new XmlSerializerNamespaces();
        ns.Add("rsm", "urn:un:unece:uncefact:data:standard:CrossIndustryInvoice:100");
        ns.Add("ram", "urn:un:unece:uncefact:data:standard:ReusableAggregateBusinessInformationEntity:100");
        ns.Add("udt", "urn:un:unece:uncefact:data:standard:UnqualifiedDataType:100");
        ns.Add("qdt", "urn:un:unece:uncefact:data:standard:QualifiedDataType:100");

        using var sw = new Utf8StringWriter();
        serializer.Serialize(sw, invoice, ns);
        return sw.ToString();
    }

    private sealed class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }
}
