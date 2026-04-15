using System.Diagnostics;
using System.Xml;
using System.Xml.Linq;

namespace Tests;

public static class CiiSchematronValidator
{
    //public static void ValidateCiiExtendedSchematron(string xml)
    //{
    //    if (string.IsNullOrWhiteSpace(xml))
    //        throw new ArgumentException("XML is null or empty", nameof(xml));

    //    // Base directory of the test assembly (bin\Debug\...\net8.0)
    //    string baseDir = AppContext.BaseDirectory;
    //    string schemaDir = Path.Combine(baseDir, "Schemas");
    //    string schematronXslt = Path.Combine(schemaDir, "FACTUR-X_EXTENDED.xslt");

    //    if (!File.Exists(schematronXslt))
    //        Assert.Fail($"Schematron XSLT not found: {schematronXslt}");

    //    // Create Saxon processor (XSLT 3 / XPath 3 engine)
    //    var processor = new Processor();

    //    // Compile the Schematron XSLT
    //    // The base URI of the stylesheet is the file path, so document('FACTUR-X_EXTENDED_codedb.xml')
    //    // will resolve relative to the Schemas directory.
    //    var compiler = processor.NewXsltCompiler();
    //    XsltExecutable exec = compiler.Compile(new Uri(schematronXslt));

    //    XsltTransformer transformer = exec.Load();

    //    // Build input XML document from the string
    //    var builder = processor.NewDocumentBuilder();

    //    // BaseUri for the input document is not critical here, but set a dummy valid URI
    //    builder.BaseUri = new Uri("file:///dummy.xml");

    //    using var stringReader = new StringReader(xml);
    //    using var xmlReader = XmlReader.Create(stringReader);
    //    XdmNode inputDoc = builder.Build(xmlReader);

    //    // Set the initial context node (the Factur-X invoice)
    //    transformer.InitialContextNode = inputDoc;

    //    // Run transformation to SVRL (Schematron Validation Report Language)
    //    var sw = new StringWriter();
    //    Serializer serializer = processor.NewSerializer(sw);

    //    transformer.Run(serializer);

    //    string svrlResult = sw.ToString();

    //    // Parse SVRL and check for failed assertions
    //    var svrlDoc = XDocument.Parse(svrlResult);
    //    XNamespace svrl = "http://purl.oclc.org/dsdl/svrl";

    //    var failedAsserts = svrlDoc.Descendants(svrl + "failed-assert").ToList();

    //    if (failedAsserts.Any())
    //    {
    //        var messages = string.Join(Environment.NewLine,
    //            failedAsserts.Select(f =>
    //                f.Element(svrl + "text")?.Value?.Trim() ?? f.ToString()));

    //        Assert.Fail("Schematron validation failed:" + Environment.NewLine + messages);
    //    }
    //}
}
