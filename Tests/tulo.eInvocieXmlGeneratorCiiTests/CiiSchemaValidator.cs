using System.Xml;
using System.Xml.Schema;

namespace Tests;

public static class CiiSchemaValidator
{
    public static void ValidateCiiZugferd24Extended(string xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
            throw new ArgumentException("XML is null or empty", nameof(xml));

        // Base directory of the test assembly (bin\Debug\... or similar)
        string baseDir = AppContext.BaseDirectory;
        string schemaDir = Path.Combine(baseDir, "Schemas");

        // Factur-X schema files (must be copied to output directory)
        string mainXsd = Path.Combine(schemaDir, "FACTUR-X_EXTENDED.xsd");
        string ramXsd = Path.Combine(schemaDir, "FACTUR-X_EXTENDED_urn_un_unece_uncefact_data_standard_ReusableAggregateBusinessInformationEntity_100.xsd");
        string udtXsd = Path.Combine(schemaDir, "FACTUR-X_EXTENDED_urn_un_unece_uncefact_data_standard_UnqualifiedDataType_100.xsd");
        string qdtXsd = Path.Combine(schemaDir, "FACTUR-X_EXTENDED_urn_un_unece_uncefact_data_standard_QualifiedDataType_100.xsd");

        if (!File.Exists(mainXsd)) Assert.Fail($"XSD file not found: {mainXsd}");
        if (!File.Exists(ramXsd)) Assert.Fail($"XSD file not found: {ramXsd}");
        if (!File.Exists(udtXsd)) Assert.Fail($"XSD file not found: {udtXsd}");
        if (!File.Exists(qdtXsd)) Assert.Fail($"XSD file not found: {qdtXsd}");

        var settings = new XmlReaderSettings
        {
            ValidationType = ValidationType.Schema,
            DtdProcessing = DtdProcessing.Prohibit,
            ValidationFlags =
                XmlSchemaValidationFlags.ReportValidationWarnings |
                XmlSchemaValidationFlags.ProcessInlineSchema |
                XmlSchemaValidationFlags.ProcessSchemaLocation
        };

        // Add all involved schemas. targetNamespace is taken from the XSDs (you pass null).
        settings.Schemas.Add(null, mainXsd);
        settings.Schemas.Add(null, ramXsd);
        settings.Schemas.Add(null, udtXsd);
        settings.Schemas.Add(null, qdtXsd);

        // Resolver is useful in case of imports/includes
        settings.Schemas.XmlResolver = new XmlUrlResolver();

        settings.ValidationEventHandler += (sender, args) =>
        {
            // Treat both error and warning as test failures (can be relaxed if needed)
            if (args.Severity == XmlSeverityType.Error ||
                args.Severity == XmlSeverityType.Warning)
            {
                throw new AssertFailedException($"XSD validation: {args.Message}");
            }
        };

        using var sr = new StringReader(xml);
        using var reader = XmlReader.Create(sr, settings);

        // Reading the whole document triggers the validation
        while (reader.Read()) { }
    }
}