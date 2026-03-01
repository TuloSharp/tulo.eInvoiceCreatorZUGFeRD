using System.Xml;
using System.Xml.Schema;

namespace Tests;
public static class CiiSchemaValidator
{
    public static void ValidateCiiZugferd24Extended(string xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
            throw new ArgumentException("XML is null or empty", nameof(xml));

        var settings = new XmlReaderSettings
        {
            ValidationType = ValidationType.Schema,
            DtdProcessing = DtdProcessing.Prohibit
        };

        string baseDir = AppContext.BaseDirectory;
        string schemaDir = Path.Combine(baseDir, "Schemas");
        string mainXsd = Path.Combine(schemaDir, "FACTUR-X_EXTENDED.xsd");
        string ramXsd = Path.Combine(schemaDir, "FACTUR-X_EXTENDED_urn_un_unece_uncefact_data_standard_ReusableAggregateBusinessInformationEntity_100.xsd");
        string udtXsd = Path.Combine(schemaDir, "FACTUR-X_EXTENDED_urn_un_unece_uncefact_data_standard_UnqualifiedDataType_100.xsd");
        string qdtXsd = Path.Combine(schemaDir, "FACTUR-X_EXTENDED_urn_un_unece_uncefact_data_standard_QualifiedDataType_100.xsd");

        if (!File.Exists(mainXsd)) Assert.Fail($"XSD-Datei nicht gefunden: {mainXsd}");
        if (!File.Exists(ramXsd)) Assert.Fail($"XSD-Datei nicht gefunden: {ramXsd}");
        if (!File.Exists(udtXsd)) Assert.Fail($"XSD-Datei nicht gefunden: {udtXsd}");
        if (!File.Exists(qdtXsd)) Assert.Fail($"XSD-Datei nicht gefunden: {qdtXsd}");

        settings.Schemas.Add(null, mainXsd);
        settings.Schemas.Add(null, ramXsd);
        settings.Schemas.Add(null, udtXsd);
        settings.Schemas.Add(null, qdtXsd);

        settings.ValidationEventHandler += (sender, args) =>
        {
            if (args.Severity == XmlSeverityType.Error ||
                args.Severity == XmlSeverityType.Warning)
            {
                throw new AssertFailedException($"XSD-Validierung: {args.Message}");
            }
        };

        using var sr = new StringReader(xml);
        using var reader = XmlReader.Create(sr, settings);

        while (reader.Read()) { }
    }
}