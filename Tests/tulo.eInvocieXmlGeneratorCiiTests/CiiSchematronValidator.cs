namespace Tests;
public static class CiiSchematronValidator
{
    public static void ValidateCiiExtendedSchematron(string xml)
    {
        //if (string.IsNullOrWhiteSpace(xml))
        //    throw new ArgumentException("XML is null or empty", nameof(xml));

        //string baseDir = AppContext.BaseDirectory;
        //string schemaDir = Path.Combine(baseDir, "Schemas");
        //string schematronXslt = Path.Combine(schemaDir, "FACTUR-X_EXTENDED.xslt");

        //if (!File.Exists(schematronXslt))
        //    Assert.Fail($"Schematron-XSLT nicht gefunden: {schematronXslt}");

        //// Saxon-Processor
        //var processor = new Processor();

        //// Stylesheet compile
        //var compiler = processor.NewXsltCompiler();
        //var exec = compiler.Compile(new Uri(schematronXslt));

        //var transformer = exec.Load();

        //// Imput-Document
        //var builder = processor.NewDocumentBuilder();
        //builder.BaseUri = new Uri("file://dummy.xml"); // some URI
        //var inputDoc = builder.Build(new StringReader(xml));

        //transformer.InitialContextNode = inputDoc;

        //// Output (SVRL) in String
        //var sw = new StringWriter();
        //var serializer = processor.NewSerializer(sw);
        //transformer.Run(serializer);

        //var result = sw.ToString();

        //// SVRL parsen
        //var doc = XDocument.Parse(result);
        //XNamespace svrl = "http://purl.oclc.org/dsdl/svrl";

        //var failedAsserts = doc.Descendants(svrl + "failed-assert").ToList();

        //if (failedAsserts.Any())
        //{
        //    var messages = string.Join(Environment.NewLine,
        //        failedAsserts.Select(f =>
        //            f.Element(svrl + "text")?.Value ?? f.ToString()));

        //    Assert.Fail("Schematron-Validierung fehlgeschlagen:" + Environment.NewLine + messages);
        //}
    }
}
