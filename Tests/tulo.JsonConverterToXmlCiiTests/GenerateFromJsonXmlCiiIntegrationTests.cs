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

    [TestMethod(DisplayName = "Create an xml form ZF_Extended__Sammelrechnung_3_Bestellungen.json file and verifies it")]
    public void Can_generate_xml_from_json_ZF_Extended__Sammelrechnung_3_Bestellungen()
    {
        // JSON File find & read
        string baseDir = AppContext.BaseDirectory;
        string jsonPath = Path.Combine(baseDir, "JsonExamples", "ZF_Extended__Sammelrechnung_3_Bestellungen.json");

        Assert.IsTrue(File.Exists(jsonPath), $"JSON File isnot found: {jsonPath}");

        string json = File.ReadAllText(jsonPath, Encoding.UTF8);

        // JSON → Deserialize invoice
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

        // Invoice → CII → XML
        var cii = _mapper.Map(invoice);
        string xml = _exporter.ToXml(cii);

        // Save
        string fileName = $"ZF_Extended__Sammelrechnung_3_Bestellungen_{invoice.InvoiceNumber}_{DateTime.Now:yyyyMMddHHmmssfff}.xml";
        string filePath = Path.Combine(_tempDir, fileName);
        SaveXmlFile(invoice, xml, filePath);

        // Mini-Assertions
        Assert.IsFalse(string.IsNullOrWhiteSpace(xml), "XML must not be empty.");
        StringAssert.Contains(xml, "CrossIndustryInvoice");
        ContainsEscaped(xml, invoice.InvoiceNumber);
        ContainsEscapedAll(xml, invoice.Seller.Name, invoice.Buyer.Name);

        Assert.IsTrue(File.Exists(filePath), $"Example-XML is not found: {filePath}");

        string expectedXml = File.ReadAllText(filePath, Encoding.UTF8);

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

        Assert.AreEqual(expected.ExchangedDocumentContext.GuidelineSpecifiedDocumentContextParameter.ID.Value,
                        actual.ExchangedDocumentContext.GuidelineSpecifiedDocumentContextParameter.ID.Value,
                        "Guideline ID is different (Profil).");

        Assert.AreEqual(expected.ExchangedDocument.ID.Value,
                        actual.ExchangedDocument.ID.Value,
                        "Invoice number is different.");

        var expIssue = expected.ExchangedDocument.IssueDateTime.Item;
        var actIssue = actual.ExchangedDocument.IssueDateTime.Item;
        Assert.AreEqual(expIssue.Value, actIssue.Value, "Invoice date is different.");

        Assert.AreEqual(expected.SupplyChainTradeTransaction.ApplicableHeaderTradeAgreement.SellerTradeParty.Name.Value,
                        actual.SupplyChainTradeTransaction.ApplicableHeaderTradeAgreement.SellerTradeParty.Name.Value,
                        "Seller-Name is different.");

        Assert.AreEqual(expected.SupplyChainTradeTransaction.ApplicableHeaderTradeAgreement.BuyerTradeParty.Name.Value,
                        actual.SupplyChainTradeTransaction.ApplicableHeaderTradeAgreement.BuyerTradeParty.Name.Value,
                        "Buyer-Name is different.");

        // Header-Summation
        var expSum = expected.SupplyChainTradeTransaction
            .ApplicableHeaderTradeSettlement
            .SpecifiedTradeSettlementHeaderMonetarySummation;

        var actSum = actual.SupplyChainTradeTransaction.ApplicableHeaderTradeSettlement.SpecifiedTradeSettlementHeaderMonetarySummation;

        Assert.AreEqual(expSum.LineTotalAmount.Value, actSum.LineTotalAmount.Value, "LineTotalAmount (Header) ist different.");
        Assert.AreEqual(expSum.TaxBasisTotalAmount.Value, actSum.TaxBasisTotalAmount.Value, "TaxBasisTotalAmount (Header) is different.");
        Assert.AreEqual(expSum.GrandTotalAmount.Value, actSum.GrandTotalAmount.Value, "GrandTotalAmount (Header) is different.");
        Assert.AreEqual(expSum.DuePayableAmount.Value, actSum.DuePayableAmount.Value, "DuePayableAmount (Header) is different.");

        decimal expTaxTotal = expSum.TaxTotalAmount?.Sum(a => a.Value) ?? 0m;
        decimal actTaxTotal = actSum.TaxTotalAmount?.Sum(a => a.Value) ?? 0m;
        Assert.AreEqual(expTaxTotal, actTaxTotal, "TaxTotalAmount (Header) is different.");

        // Compare header control groups (e.g., 19% + 7%)
        var expTaxes = expected.SupplyChainTradeTransaction.ApplicableHeaderTradeSettlement.ApplicableTradeTax;
        var actTaxes = actual.SupplyChainTradeTransaction.ApplicableHeaderTradeSettlement.ApplicableTradeTax;

        Assert.HasCount(expTaxes.Length, actTaxes, "Quantity Header control groups is different.");

        foreach (var expTax in expTaxes)
        {
            var actTax = actTaxes.FirstOrDefault(t =>
                t.RateApplicablePercent.Value == expTax.RateApplicablePercent.Value);

            Assert.IsNotNull(actTax, $"NO header group with {expTax.RateApplicablePercent.Value}% in generated document.");

            Assert.AreEqual(expTax.BasisAmount.Value, actTax.BasisAmount.Value, $"BasisAmount at {expTax.RateApplicablePercent.Value}% is different.");
            Assert.AreEqual(expTax.CalculatedAmount.Value, actTax.CalculatedAmount.Value, $"CalculatedAmount at {expTax.RateApplicablePercent.Value}% is different.");
        }

        // Positions lines
        var expLines = expected.SupplyChainTradeTransaction.IncludedSupplyChainTradeLineItem;
        var actLines = actual.SupplyChainTradeTransaction.IncludedSupplyChainTradeLineItem;

        Assert.HasCount(expLines.Length, actLines, "Position lines quantity is different.");

        // Compare all lines: ProductName + LineTotalAmount
        for (int i = 0; i < expLines.Length; i++)
        {
            var e = expLines[i];
            var a = actLines[i];

            Assert.AreEqual(e.SpecifiedTradeProduct.Name.Value,
                            a.SpecifiedTradeProduct.Name.Value,
                            $"Product name in row {i + 1} are different.");

            var eSum = e.SpecifiedLineTradeSettlement.SpecifiedTradeSettlementLineMonetarySummation.LineTotalAmount.Value;

            var aSum = a.SpecifiedLineTradeSettlement.SpecifiedTradeSettlementLineMonetarySummation.LineTotalAmount.Value;

            Assert.AreEqual(eSum, aSum, $"LineTotalAmount in row {i + 1} is different.");
        }
    }

    [TestMethod(DisplayName = "Create an xml form EN16931_Gutschrift.json file and verifies it")]
    public void Can_generate_xml_from_json_EN16931_Gutschrift()
    {
        // JSON File find & read
        string baseDir = AppContext.BaseDirectory;
        string jsonPath = Path.Combine(baseDir, "JsonExamples", "EN16931_Gutschrift.json");

        Assert.IsTrue(File.Exists(jsonPath), $"JSON File isnot found: {jsonPath}");

        string json = File.ReadAllText(jsonPath, Encoding.UTF8);

        // JSON → Deserialize invoice
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

        // Invoice → CII → XML
        var cii = _mapper.Map(invoice);
        string xml = _exporter.ToXml(cii);

        // Save
        string fileName = $"XmlCii_{invoice.InvoiceNumber}_{DateTime.Now:yyyyMMddHHmmssfff}.xml";
        string filePath = Path.Combine(_tempDir, fileName);
        SaveXmlFile(invoice, xml, filePath);

        // Mini-Assertions
        Assert.IsFalse(string.IsNullOrWhiteSpace(xml), "XML must not be empty.");
        StringAssert.Contains(xml, "CrossIndustryInvoice");
        ContainsEscaped(xml, invoice.InvoiceNumber);
        ContainsEscapedAll(xml, invoice.Seller.Name, invoice.Buyer.Name);
    }

    [TestMethod(DisplayName = "Create an xml form EXTENDED_Kostenrechnung.json file and verifies it")]
    public void Can_generate_xml_from_json_EXTENDED_Kostenrechnung()
    {
        // JSON File find & read
        string baseDir = AppContext.BaseDirectory;
        string jsonPath = Path.Combine(baseDir, "JsonExamples", "EXTENDED_Kostenrechnung.json");

        Assert.IsTrue(File.Exists(jsonPath), $"JSON File isnot found: {jsonPath}");

        string json = File.ReadAllText(jsonPath, Encoding.UTF8);

        // JSON → Deserialize invoice
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

        // Invoice → CII → XML
        var cii = _mapper.Map(invoice);
        string xml = _exporter.ToXml(cii);

        // Save
        string fileName = $"XmlCii_{invoice.InvoiceNumber}_{DateTime.Now:yyyyMMddHHmmssfff}.xml";
        string filePath = Path.Combine(_tempDir, fileName);
        SaveXmlFile(invoice, xml, filePath);

        // Mini-Assertions
        Assert.IsFalse(string.IsNullOrWhiteSpace(xml), "XML must not be empty.");
        StringAssert.Contains(xml, "CrossIndustryInvoice");
        ContainsEscaped(xml, invoice.InvoiceNumber);
        ContainsEscapedAll(xml, invoice.Seller.Name, invoice.Buyer.Name);
    }

    [TestMethod(DisplayName = "Create an xml formEXTENDED_Kleinunternehmer_ohneUStId.json file and verifies it")]
    public void Can_generate_xml_from_json_EXTENDED_Kleinunternehmer_ohneUStId()
    {
        // JSON File find & read
        string baseDir = AppContext.BaseDirectory;
        string jsonPath = Path.Combine(baseDir, "JsonExamples", "EXTENDED_Kleinunternehmer_ohneUStId.json");

        Assert.IsTrue(File.Exists(jsonPath), $"JSON File isnot found: {jsonPath}");

        string json = File.ReadAllText(jsonPath, Encoding.UTF8);

        // JSON → Deserialize invoice
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

        // Invoice → CII → XML
        var cii = _mapper.Map(invoice);
        string xml = _exporter.ToXml(cii);

        // Save
        string fileName = $"EXTENDED_Kleinunternehmer_ohneUStId_{invoice.InvoiceNumber}_{DateTime.Now:yyyyMMddHHmmssfff}.xml";
        string filePath = Path.Combine(_tempDir, fileName);
        SaveXmlFile(invoice, xml, filePath);

        // Mini-Assertions
        Assert.IsFalse(string.IsNullOrWhiteSpace(xml), "XML must not be empty.");
        StringAssert.Contains(xml, "CrossIndustryInvoice");
        ContainsEscaped(xml, invoice.InvoiceNumber);
        ContainsEscapedAll(xml, invoice.Seller.Name, invoice.Buyer.Name);

        Assert.IsTrue(File.Exists(filePath), $"Example-XML is not found: {filePath}");

        string expectedXml = File.ReadAllText(filePath, Encoding.UTF8);

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

        // Asserts
        Assert.AreEqual(expected.ExchangedDocumentContext.GuidelineSpecifiedDocumentContextParameter.ID.Value, actual.ExchangedDocumentContext.GuidelineSpecifiedDocumentContextParameter.ID.Value, "Guideline ID is different (Profil).");

        Assert.AreEqual(expected.ExchangedDocument.ID.Value, actual.ExchangedDocument.ID.Value, "Invoice number is different.");

        var expIssue = expected.ExchangedDocument.IssueDateTime.Item;
        var actIssue = actual.ExchangedDocument.IssueDateTime.Item;
        Assert.AreEqual(expIssue.Value, actIssue.Value, "Invoice date is different.");

        Assert.AreEqual(expected.SupplyChainTradeTransaction.ApplicableHeaderTradeAgreement.SellerTradeParty.Name.Value, actual.SupplyChainTradeTransaction.ApplicableHeaderTradeAgreement.SellerTradeParty.Name.Value, "Seller-Name is different.");

        Assert.AreEqual(expected.SupplyChainTradeTransaction.ApplicableHeaderTradeAgreement.BuyerTradeParty.Name.Value, actual.SupplyChainTradeTransaction.ApplicableHeaderTradeAgreement.BuyerTradeParty.Name.Value, "Buyer-Name is different.");

        // Summation
        var expSum = expected.SupplyChainTradeTransaction
            .ApplicableHeaderTradeSettlement
            .SpecifiedTradeSettlementHeaderMonetarySummation;

        var actSum = actual.SupplyChainTradeTransaction
            .ApplicableHeaderTradeSettlement
            .SpecifiedTradeSettlementHeaderMonetarySummation;

        Assert.AreEqual(expSum.LineTotalAmount.Value, actSum.LineTotalAmount.Value, "LineTotalAmount (Header) is different.");
        Assert.AreEqual(expSum.TaxBasisTotalAmount.Value, actSum.TaxBasisTotalAmount.Value, "TaxBasisTotalAmount (Header) is different.");
        Assert.AreEqual(expSum.GrandTotalAmount.Value, actSum.GrandTotalAmount.Value, "GrandTotalAmount (Header) is different.");
        Assert.AreEqual(expSum.DuePayableAmount.Value, actSum.DuePayableAmount.Value, "DuePayableAmount (Header) is different.");

        decimal expTaxTotal = expSum.TaxTotalAmount?.Sum(a => a.Value) ?? 0m;
        decimal actTaxTotal = actSum.TaxTotalAmount?.Sum(a => a.Value) ?? 0m;
        Assert.AreEqual(expTaxTotal, actTaxTotal, "TaxTotalAmount (Header) is different.");

        // tax groups (0% )
        var expTaxes = expected.SupplyChainTradeTransaction.ApplicableHeaderTradeSettlement.ApplicableTradeTax ?? System.Array.Empty<TradeTaxType>();
        var actTaxes = actual.SupplyChainTradeTransaction.ApplicableHeaderTradeSettlement.ApplicableTradeTax ?? System.Array.Empty<TradeTaxType>();

        Assert.HasCount(expTaxes.Length, actTaxes, "Quantity Header tax groups is different.");

        foreach (var expTax in expTaxes)
        {
            var actTax = actTaxes.FirstOrDefault(t =>
                t.RateApplicablePercent?.Value == expTax.RateApplicablePercent?.Value &&
                t.CategoryCode?.Value == expTax.CategoryCode?.Value &&
                t.TypeCode?.Value == expTax.TypeCode?.Value);

            Assert.IsNotNull(actTax, $"NO header tax group with {expTax.RateApplicablePercent?.Value}% / {expTax.CategoryCode?.Value} found in generated document.");

            // Basis/Calculated (bei 0% kann CalculatedAmount 0 sein)
            if (expTax.BasisAmount != null && actTax!.BasisAmount != null)
                Assert.AreEqual(expTax.BasisAmount.Value, actTax.BasisAmount.Value, $"BasisAmount at {expTax.RateApplicablePercent?.Value}% is different.");

            if (expTax.CalculatedAmount != null && actTax!.CalculatedAmount != null)
                Assert.AreEqual(expTax.CalculatedAmount.Value, actTax.CalculatedAmount.Value, $"CalculatedAmount at {expTax.RateApplicablePercent?.Value}% is different.");
        }

        // Lines
        var expLines = expected.SupplyChainTradeTransaction.IncludedSupplyChainTradeLineItem ?? System.Array.Empty<SupplyChainTradeLineItemType>();
        var actLines = actual.SupplyChainTradeTransaction.IncludedSupplyChainTradeLineItem ?? System.Array.Empty<SupplyChainTradeLineItemType>();

        Assert.HasCount(expLines.Length, actLines, "Position lines quantity is different.");

        for (int i = 0; i < expLines.Length; i++)
        {
            var e = expLines[i];
            var a = actLines[i];

            Assert.AreEqual(e.SpecifiedTradeProduct.Name.Value, a.SpecifiedTradeProduct.Name.Value, $"Product name in row {i + 1} are different.");

            var eSumLine = e.SpecifiedLineTradeSettlement.SpecifiedTradeSettlementLineMonetarySummation.LineTotalAmount.Value;
            var aSumLine = a.SpecifiedLineTradeSettlement.SpecifiedTradeSettlementLineMonetarySummation.LineTotalAmount.Value;

            Assert.AreEqual(eSumLine, aSumLine, $"LineTotalAmount in row {i + 1} is different.");

            var eTax = e.SpecifiedLineTradeSettlement.ApplicableTradeTax?.FirstOrDefault();
            var aTax = a.SpecifiedLineTradeSettlement.ApplicableTradeTax?.FirstOrDefault();

            if (eTax?.RateApplicablePercent != null && aTax?.RateApplicablePercent != null)
            {
                Assert.AreEqual(
                    eTax.RateApplicablePercent.Value,
                    aTax.RateApplicablePercent.Value,
                    $"Line {i + 1} Tax% different.");
            }
        }
    }

    #region Utilities
    private static void SaveXmlFile(Invoice invoice, string xml, string filePath)
    {
        File.WriteAllText(filePath, xml);
    }

    public static void ContainsEscaped(string xml, string expectedText, string? message = null)
    {
        Assert.IsFalse(string.IsNullOrWhiteSpace(xml), "XML is empty.");
        Assert.IsNotNull(expectedText, "ExpectedText is null.");

        // XML Escape: & -> &amp; , < -> &lt; , > -> &gt; , " -> &quot; , ' -> &apos;
        string escaped = SecurityElement.Escape(expectedText) ?? string.Empty;

        // If escaped becomes empty (e.g., expectedText = “”), do not check
        if (string.IsNullOrEmpty(escaped))
            return;

        StringAssert.Contains(xml, escaped, message ?? $"XML does not contain the expected value: {expectedText}");
    }

    // Checks multiple strings at once
    public static void ContainsEscapedAll(string xml, params string[] expectedTexts)
    {
        foreach (var t in expectedTexts)
            ContainsEscaped(xml, t);
    }
    #endregion
}
