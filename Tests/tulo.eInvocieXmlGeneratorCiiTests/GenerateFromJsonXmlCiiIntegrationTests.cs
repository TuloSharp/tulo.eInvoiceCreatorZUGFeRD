using System.Security;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using tulo.eInvoiceXmlGeneratorCii.Mappers;
using tulo.eInvoiceXmlGeneratorCii.Models;
using tulo.eInvoiceXmlGeneratorCii.Services;
using Zugferd24.Extended;

namespace Tests;

[TestClass]
public class GenerateFromJsonXmlCiiIntegrationTests
{
    private ICiiMapper _mapper = null!;
    private IXmlObjectCleaner _cleaner = null!;
    private IXmlCiiExporter _exporter = null!;
    private string _tempDir = string.Empty;

    [TestInitialize]
    public void Setup()
    {
        _cleaner = new XmlObjectCleaner();
        _mapper = new CiiMapper();
        _exporter = new XmlCiiExporter(_cleaner);
        _tempDir = Path.GetTempPath();
    }

    [TestMethod(DisplayName = "Create XML from EN16931_Gutschrift.json and verify it against example XML")]
    public void Can_generate_xml_from_json_EN16931_Gutschrift()
    {
        AssertJsonRoundtripMatchesExample("EN16931_Gutschrift.json", "EN16931_Gutschrift.xml", true);
    }

    [TestMethod(DisplayName = "Create XML from EXTENDED_Kostenrechnung.json and verify it against example XML")]
    public void Can_generate_xml_from_json_EXTENDED_Kostenrechnung()
    {
        AssertJsonRoundtripMatchesExample("EXTENDED_Kostenrechnung.json", "EXTENDED_Kostenrechnung.xml", false);
    }

    [TestMethod(DisplayName = "Create XML from EXTENDED_Kleinunternehmer_ohneUStId.json and verify it against example XML")]
    public void Can_generate_xml_from_json_EXTENDED_Kleinunternehmer_ohneUStId()
    {
        AssertJsonRoundtripMatchesExample("EXTENDED_Kleinunternehmer_ohneUStId.json", "EXTENDED_Kleinunternehmer_ohneUStId.xml", true);
    }

    [TestMethod(DisplayName = "Create XML from ZF_Extended__Abschlagsrechnung_SubInvoiceLine_u_LV_Nr_.json and verify it against example XML")]
    public void Can_generate_xml_from_json_ZF_Extended__Abschlagsrechnung_SubInvoiceLine_u_LV_Nr_()
    {
        AssertJsonRoundtripMatchesExample("ZF_Extended__Abschlagsrechnung_SubInvoiceLine_u_LV_Nr_.json", "ZUGFeRD_Extended__Abschlagsrechnung_SubInvoiceLine_u_LV_Nr_.xml", true);
    }

    private void AssertJsonRoundtripMatchesExample(string jsonFileName, string exampleXmlFileName, bool enableToTest)
    {
        // JSON laden
        string baseDir = AppContext.BaseDirectory;
        string jsonPath = Path.Combine(baseDir, "JsonExamples", jsonFileName);
        Assert.IsTrue(File.Exists(jsonPath), $"JSON file not found: {jsonPath}");

        string json = File.ReadAllText(jsonPath, Encoding.UTF8);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };

        var invoice = JsonSerializer.Deserialize<Invoice>(json, options);
        Assert.IsNotNull(invoice, "Invoice could not be loaded from JSON.");

        // Safety defaults
        invoice!.Seller ??= new Party();
        invoice.Buyer ??= new Party();
        invoice.Payment ??= new PaymentDetails();
        invoice.Lines ??= new List<InvoiceLine>();
        invoice.Notes ??= new List<InvoiceNote>();
        invoice.Payment.Terms ??= new List<PaymentTermDetails>();

        // Invoice -> CII -> XML
        var cii = _mapper.Map(invoice);
        string xml = _exporter.ToXml(cii);

        // XML speichern (nur Debug/Artefakt)
        string fileName = $"XmlCii_{invoice.InvoiceNumber}_{DateTime.Now:yyyyMMddHHmmssfff}.xml";
        string filePath = Path.Combine(_tempDir, fileName);
        SaveXmlFile(invoice, xml, filePath);

        // Mini-Checks
        Assert.IsFalse(string.IsNullOrWhiteSpace(xml), "XML must not be empty.");
        StringAssert.Contains(xml, "CrossIndustryInvoice");
        ContainsEscaped(xml, invoice.InvoiceNumber);
        ContainsEscapedAll(xml, invoice.Seller.Name, invoice.Buyer.Name);

        // Gegen echte Beispiel-XML prüfen
        string expectedPath = Path.Combine(baseDir, "Examples", exampleXmlFileName);
        Assert.IsTrue(File.Exists(expectedPath), $"Example XML not found: {expectedPath}");

        string expectedXml = File.ReadAllText(expectedPath, Encoding.UTF8);

        var serializer = new XmlSerializer(typeof(CrossIndustryInvoiceType));

        CrossIndustryInvoiceType expected;
        CrossIndustryInvoiceType actual;

        using (var sr = new StringReader(expectedXml))
        {
            expected = (CrossIndustryInvoiceType)serializer.Deserialize(sr)!;
        }

        using (var sr = new StringReader(xml))
        {
            actual = (CrossIndustryInvoiceType)serializer.Deserialize(sr)!;
        }

        AssertBasicInvoiceEquality(expected, actual, enableToTest);
    }

    private static void AssertBasicInvoiceEquality(CrossIndustryInvoiceType expected, CrossIndustryInvoiceType actual, bool enableToTest)
    {
        var expectedGuideline = expected.ExchangedDocumentContext.GuidelineSpecifiedDocumentContextParameter.ID.Value;
        var actualGuideline = actual.ExchangedDocumentContext.GuidelineSpecifiedDocumentContextParameter.ID.Value;

        Assert.IsFalse(string.IsNullOrWhiteSpace(actualGuideline), "Guideline ID must not be empty.");

        // Minimal tolerant check:
        // generated XML may contain the full profile identifier while the example XML only contains the EN16931 base URN
        if (!string.IsNullOrWhiteSpace(expectedGuideline))
        {
            StringAssert.Contains(actualGuideline, expectedGuideline, "Guideline ID does not contain expected base guideline.");
        }

        Assert.AreEqual(
            expected.ExchangedDocument.ID.Value,
            actual.ExchangedDocument.ID.Value,
            "Invoice number is different.");

        var expIssue = expected.ExchangedDocument.IssueDateTime.Item;
        var actIssue = actual.ExchangedDocument.IssueDateTime.Item;
        Assert.AreEqual(expIssue.Value, actIssue.Value, "Invoice date is different.");

        Assert.AreEqual(
            expected.SupplyChainTradeTransaction.ApplicableHeaderTradeAgreement.SellerTradeParty.Name.Value,
            actual.SupplyChainTradeTransaction.ApplicableHeaderTradeAgreement.SellerTradeParty.Name.Value,
            "Seller name is different.");

        Assert.AreEqual(
            expected.SupplyChainTradeTransaction.ApplicableHeaderTradeAgreement.BuyerTradeParty.Name.Value,
            actual.SupplyChainTradeTransaction.ApplicableHeaderTradeAgreement.BuyerTradeParty.Name.Value,
            "Buyer name is different.");

        var expSum = expected.SupplyChainTradeTransaction
            .ApplicableHeaderTradeSettlement
            .SpecifiedTradeSettlementHeaderMonetarySummation;

        var actSum = actual.SupplyChainTradeTransaction
            .ApplicableHeaderTradeSettlement
            .SpecifiedTradeSettlementHeaderMonetarySummation;

        if (enableToTest)
        {
            Assert.AreEqual(expSum.LineTotalAmount.Value, actSum.LineTotalAmount.Value, "LineTotalAmount is different.");
            Assert.AreEqual(expSum.TaxBasisTotalAmount.Value, actSum.TaxBasisTotalAmount.Value, "TaxBasisTotalAmount is different.");
            Assert.AreEqual(expSum.GrandTotalAmount.Value, actSum.GrandTotalAmount.Value, "GrandTotalAmount is different.");
            Assert.AreEqual(expSum.DuePayableAmount.Value, actSum.DuePayableAmount.Value, "DuePayableAmount is different.");

            decimal expTaxTotal = expSum.TaxTotalAmount?.Sum(a => a.Value) ?? 0m;
            decimal actTaxTotal = actSum.TaxTotalAmount?.Sum(a => a.Value) ?? 0m;
            Assert.AreEqual(expTaxTotal, actTaxTotal, "TaxTotalAmount is different.");
        }

        var expLines = expected.SupplyChainTradeTransaction.IncludedSupplyChainTradeLineItem
            ?? Array.Empty<SupplyChainTradeLineItemType>();
        var actLines = actual.SupplyChainTradeTransaction.IncludedSupplyChainTradeLineItem
            ?? Array.Empty<SupplyChainTradeLineItemType>();

        Assert.AreEqual(expLines.Length, actLines.Length, "Line count is different.");

        for (int i = 0; i < expLines.Length; i++)
        {
            var e = expLines[i];
            var a = actLines[i];

            Assert.AreEqual(
                e.SpecifiedTradeProduct.Name?.Value,
                a.SpecifiedTradeProduct.Name?.Value,
                $"Product name in line {i + 1} is different.");

            var eLineTotal = e.SpecifiedLineTradeSettlement?
                .SpecifiedTradeSettlementLineMonetarySummation?
                .LineTotalAmount?.Value ?? 0m;

            var aLineTotal = a.SpecifiedLineTradeSettlement?
                .SpecifiedTradeSettlementLineMonetarySummation?
                .LineTotalAmount?.Value ?? 0m;

            Assert.AreEqual(eLineTotal, aLineTotal, $"LineTotalAmount in line {i + 1} is different.");

            var eTax = e.SpecifiedLineTradeSettlement?.ApplicableTradeTax?.FirstOrDefault();
            var aTax = a.SpecifiedLineTradeSettlement?.ApplicableTradeTax?.FirstOrDefault();

            decimal eRate = eTax?.RateApplicablePercent?.Value ?? 0m;
            decimal aRate = aTax?.RateApplicablePercent?.Value ?? 0m;

            Assert.AreEqual(eRate, aRate, $"Tax percent in line {i + 1} is different.");
        }
    }

    #region Utilities
    private static void SaveXmlFile(Invoice invoice, string xml, string filePath)
    {
        File.WriteAllText(filePath, xml, Encoding.UTF8);
    }

    public static void ContainsEscaped(string xml, string? expectedText, string? message = null)
    {
        Assert.IsFalse(string.IsNullOrWhiteSpace(xml), "XML is empty.");

        if (string.IsNullOrWhiteSpace(expectedText))
            return;

        string escaped = SecurityElement.Escape(expectedText) ?? string.Empty;

        if (string.IsNullOrEmpty(escaped))
            return;

        StringAssert.Contains(xml, escaped, message ?? $"XML does not contain expected value: {expectedText}");
    }

    public static void ContainsEscapedAll(string xml, params string?[] expectedTexts)
    {
        foreach (var t in expectedTexts)
            ContainsEscaped(xml, t);
    }
    #endregion
}