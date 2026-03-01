using System.Xml;
using tulo.CoreLib.Components.ResultPattern;

namespace tulo.eInvoice.eInvoiceViewer.Utilities;
public static class InvoiceTypeDetector
{
    public static Result<InvoiceType> DetectFromContent(string xmlInvoiceContent)
    {
        if (string.IsNullOrWhiteSpace(xmlInvoiceContent))
            return Result<InvoiceType>.Failure(new Error("XML content is empty.", ErrorCodes.InvalidInput));

        var s = xmlInvoiceContent.AsSpan().TrimStart();

        var doc = new XmlDocument();
        doc.LoadXml(xmlInvoiceContent);

        var root = doc.DocumentElement;

        if (root?.NamespaceURI == "urn:un:unece:uncefact:data:standard:CrossIndustryInvoice:100")
            return Result<InvoiceType>.Success(InvoiceType.CII);

        if (root?.NamespaceURI == "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2")
            return Result<InvoiceType>.Success(InvoiceType.UBL);

        return Result<InvoiceType>.Failure(new Error("Unknown invoice format. Supported formats: CII or UBL.", ErrorCodes.UnknownInvoiceFormat));
    }
}
