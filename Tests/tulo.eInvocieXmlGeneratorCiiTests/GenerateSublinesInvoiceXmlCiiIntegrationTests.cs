using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using tulo.eInvoiceXmlGeneratorCii.Mappers;
using tulo.eInvoiceXmlGeneratorCii.Models;
using tulo.eInvoiceXmlGeneratorCii.Services;

namespace Tests;

[TestClass]
public class GenerateSubInvoiceLinesXmlCiiIntegrationTests
{
    private ICiiMapper _mapper = null!;
    private IXmlObjectCleaner _cleaner = null!;
    private IXmlCiiExporter _exporter = null!;

    [TestInitialize]
    public void Setup()
    {
        _cleaner = new XmlObjectCleaner();
        _mapper = new CiiMapper();
        _exporter = new XmlCiiExporter(_cleaner);
    }

    [TestMethod(DisplayName = "Verifies it creates ZUGFeRD invoice equal to official extended sub invoice lines sample")]
    public void Generated_invoice_matches_official_extended_sub_invoice_lines_sample()
    {
        // Arrange
        var invoice = BuildInvoiceMatchingOfficialSubInvoiceLinesSample();

        // Act + Assert
        AssertGeneratedInvoiceMatchesOfficialSample(invoice);
    }

    [TestMethod(DisplayName = "Verifies it creates ZUGFeRD invoice equal to official extended sub invoice lines sample from JSON example")]
    public void Generated_invoice_from_json_matches_official_extended_sub_invoice_lines_sample()
    {
        // Arrange
        var invoice = LoadInvoiceFromJsonExample();

        // Act + Assert
        AssertGeneratedInvoiceMatchesOfficialSample(invoice);
    }

    private void AssertGeneratedInvoiceMatchesOfficialSample(Invoice invoice)
    {
        var cii = _mapper.Map(invoice);
        string xml = _exporter.ToXml(cii);

        CiiSchemaValidator.ValidateCiiZugferd24Extended(xml);
        SaveXmlFile(invoice, xml);

        string baseDir = AppContext.BaseDirectory;
        string samplePath = Path.Combine(baseDir, "Examples", "Extended___SubInvoiceLines_Buero_Material_Bsp3__.xml");

        Assert.IsTrue(File.Exists(samplePath), $"Example XML was not found: {samplePath}");

        string sampleXml = File.ReadAllText(samplePath);

        var expected = XDocument.Parse(sampleXml);
        var actual = XDocument.Parse(xml);

        AssertHeader(expected, actual);
        AssertHeaderNotes(expected, actual);
        AssertHeaderTradeAgreement(expected, actual);
        AssertHeaderTradeDelivery(expected, actual);
        AssertHeaderTradeSettlement(expected, actual);
        AssertHeaderTaxes(expected, actual);
        AssertLines(expected, actual);
    }

    private static Invoice LoadInvoiceFromJsonExample()
    {
        string baseDir = AppContext.BaseDirectory;
        string jsonPath = Path.Combine(baseDir, "JsonExamples", "Extended___SubInvoiceLines_Buero_Material_Bsp3__.json");

        Assert.IsTrue(File.Exists(jsonPath), $"Example JSON was not found: {jsonPath}");

        var json = File.ReadAllText(jsonPath);
        var invoice = JsonSerializer.Deserialize<Invoice>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.IsNotNull(invoice, "Failed to deserialize invoice JSON example.");

        invoice!.Notes ??= new List<InvoiceNote>();
        invoice.Lines ??= new List<InvoiceLine>();
        invoice.Payment ??= new PaymentDetails();
        invoice.Payment.Terms ??= new List<PaymentTermDetails>();

        foreach (var line in invoice.Lines)
        {
            line.Notes ??= new List<InvoiceNote>();
        }

        return invoice;
    }

    #region Utilities Extended___SubInvoiceLines_Buero_Material_Bsp3__
    private static Invoice BuildInvoiceMatchingOfficialSubInvoiceLinesSample()
    {
        var invoice = new Invoice
        {
            InvoiceNumber = "99877",
            InvoiceDate = new DateTime(2026, 05, 30),
            Currency = "EUR",
            DocumentTypeCode = "380",
            BuyerReference = "Kundenref. BT-10",
            SellerOrderReferencedId = "G12042-1-01",
            BuyerOrderReferencedId = "BT-13",
            ContractReferencedId = "Vertragsnr. BT-12",

            HeaderChargeTotalAmount = 0.00m,
            HeaderAllowanceTotalAmount = 0.00m,
            HeaderTotalPrepaidAmount = 0.00m,
            HeaderDuePayableAmount = 1671.95m,

            Seller = new Party
            {
                ID = "998877",
                Name = "Musterbetrieb Systemhaus AG ",
                Street = "August-Müller-Strasse 222",
                Zip = "37079",
                City = "Göttingen",
                CountryCode = "DE",
                VatId = "DE09687654321",
                FiscalId = "HRA 45678",
                ContactPersonName = "Kontaktperson",
                ContactPhone = "5578",
                ContactEmail = "absender@musterberieb.de"
            },

            Buyer = new Party
            {
                ID = "330145",
                Name = "Auftraggeber Firmenkunde GmbH",
                Street = "Musterstraße 1212",
                Zip = "37073",
                City = "Göttingen",
                CountryCode = "DE",
                VatId = "DE1234567890",
                ContactPersonName = "Herr Thomas Auftraggeber",
                ContactPhone = "+49 321 456789",
                ContactEmail = "thomas.auftraggeber@Firmenkunde.de"
            },

            Payment = new PaymentDetails
            {
                PaymentMeansTypeCode = "58",
                Iban = "DE75512108001245126199",
                Bic = "PBNKDEFF",
                AccountName = "Musterbetrieb Kontoname",
                Terms = new List<PaymentTermDetails>
                {
                    new PaymentTermDetails
                    {
                        Description = "Bei Zahlung bis zum 06.06.2026 zahlen Sie mit 2,00 % Skonto € 1638,51 €",
                        DueDate = new DateTime(2026, 06, 06),
                        DiscountTerms = new PaymentDiscountTermsDetails
                        {
                            BasisAmount = 1671.95m,
                            CalculationPercent = 2.00m
                        }
                    },
                    new PaymentTermDetails
                    {
                        Description = "Bis zum zum 13.06.2026 ohne Abzug",
                        DueDate = new DateTime(2026, 06, 13)
                    }
                }
            }
        };

        invoice.Notes.AddRange(new[]
        {
            new InvoiceNote
            {
                Content = "Geschäftsführer: Herr Geschäftsführer , Muster Bau GmbH etc.",
                SubjectCode = "REG"
            },
            new InvoiceNote
            {
                Content = "Es bestehen Vereinbarungen, aus denen sich Minderungen des Entgelts ergeben können.",
                SubjectCode = "AAI"
            },
            new InvoiceNote
            {
                Content = "ZUGFeRD vers 2.4.0 Extended",
                SubjectCode = "ACB"
            },
            new InvoiceNote
            {
                Content = "Dies ist eine Beispiel-Rechnung zur empfohlenen Darstellung von Unterpositionen",
                SubjectCode = "ACB"
            }
        });

        invoice.Lines.Add(new InvoiceLine
        {
            LineId = "0101",
            ParentLineId = "01",
            LineStatusReasonCode = "DETAIL",
            Description = "Laser printer B/W",
            ProductDescription = "Schwarzweiß Laserdrucker",
            GlobalId = "88888886349852",
            GlobalIdSchemeId = "0160",
            SellerAssignedId = "123456789",
            BuyerAssignedId = "987654321",
            Quantity = 2m,
            UnitCode = "H87",
            UnitPrice = 300.00m,
            TaxPercent = 19m,
            TaxCategory = "S",
            ForcedLineTotalAmount = 600.00m
        });

        invoice.Lines.Add(new InvoiceLine
        {
            LineId = "0102",
            ParentLineId = "01",
            LineStatusReasonCode = "DETAIL",
            Description = "Ink printer color",
            ProductDescription = "Farbdrucker",
            GlobalId = "77777776349852",
            GlobalIdSchemeId = "0160",
            SellerAssignedId = "2345678910",
            BuyerAssignedId = "876543219",
            Quantity = 3m,
            UnitCode = "H87",
            UnitPrice = 150.00m,
            TaxPercent = 19m,
            TaxCategory = "S",
            ForcedLineTotalAmount = 450.00m
        });

        invoice.Lines.Add(new InvoiceLine
        {
            LineId = "0103",
            ParentLineId = "01",
            LineStatusReasonCode = "DETAIL",
            Description = "Allowance",
            ProductDescription = "Abschlagsposition",
            GlobalId = "0000006349852",
            GlobalIdSchemeId = "0160",
            SellerAssignedId = "99992345678910",
            BuyerAssignedId = "88888876543219",
            Quantity = -1m,
            UnitCode = "H87",
            UnitPrice = 50.00m,
            TaxPercent = 19m,
            TaxCategory = "S",
            ForcedLineTotalAmount = -50.00m
        });

        invoice.Lines.Add(new InvoiceLine
        {
            LineId = "01",
            LineStatusReasonCode = "GROUP",
            Description = "Subtotal hardware",
            ProductDescription = "Hardware Gesamt",
            GlobalId = "6666656349852",
            GlobalIdSchemeId = "0160",
            SellerAssignedId = "345678912",
            BuyerAssignedId = "765432198",
            Quantity = 1m,
            UnitCode = "H87",
            UnitPrice = 1000.00m,
            TaxPercent = 19m,
            TaxCategory = "S",
            ForcedLineTotalAmount = 1000.00m,
            OmitNetPriceBasisQuantity = true
        });

        invoice.Lines.Add(new InvoiceLine
        {
            LineId = "0201",
            ParentLineId = "02",
            LineStatusReasonCode = "DETAIL",
            Description = "Toner",
            ProductDescription = "Toner",
            GlobalId = "55555556349852",
            GlobalIdSchemeId = "0160",
            SellerAssignedId = "456789123",
            BuyerAssignedId = "654321987",
            Quantity = 3m,
            UnitCode = "H87",
            UnitPrice = 120.00m,
            TaxPercent = 19m,
            TaxCategory = "S",
            ForcedLineTotalAmount = 360.00m
        });

        invoice.Lines.Add(new InvoiceLine
        {
            LineId = "0202",
            ParentLineId = "02",
            LineStatusReasonCode = "DETAIL",
            Description = "PAPER",
            ProductDescription = "Kopierpapier",
            GlobalId = "5555556349852",
            GlobalIdSchemeId = "0160",
            SellerAssignedId = "567891234",
            BuyerAssignedId = "543219876",
            Quantity = 10m,
            UnitCode = "H87",
            UnitPrice = 9.00m,
            TaxPercent = 19m,
            TaxCategory = "S",
            ForcedLineTotalAmount = 90.00m
        });

        invoice.Lines.Add(new InvoiceLine
        {
            LineId = "0203",
            ParentLineId = "02",
            LineStatusReasonCode = "DETAIL",
            Description = "Allowance",
            ProductDescription = "Abschlagsposition 10% von 450,- ",
            GlobalId = "0000006349852",
            GlobalIdSchemeId = "0160",
            SellerAssignedId = "99992345678910",
            BuyerAssignedId = "88888876543219",
            Quantity = -1m,
            UnitCode = "H87",
            UnitPrice = 45.00m,
            TaxPercent = 19m,
            TaxCategory = "S",
            ForcedLineTotalAmount = -45.00m
        });

        invoice.Lines.Add(new InvoiceLine
        {
            LineId = "02",
            LineStatusReasonCode = "GROUP",
            Description = "Subtotal Accessories",
            ProductDescription = "Zubehör Gesamt",
            GlobalId = "2222256349852",
            GlobalIdSchemeId = "0160",
            SellerAssignedId = "9345678912",
            BuyerAssignedId = "9765432198",
            Quantity = 1m,
            UnitCode = "H87",
            UnitPrice = 405.00m,
            TaxPercent = 19m,
            TaxCategory = "S",
            ForcedLineTotalAmount = 405.00m,
            OmitNetPriceBasisQuantity = true
        });

        return invoice;
    }

    private static void AssertHeader(XDocument expected, XDocument actual)
    {
        AssertNodeValue(expected, actual, "ExchangedDocumentContext/GuidelineSpecifiedDocumentContextParameter/ID", "Guideline ID");
        AssertNodeValue(expected, actual, "ExchangedDocument/ID", "Invoice number");
        AssertNodeValue(expected, actual, "ExchangedDocument/Name", "Document name");
        AssertNodeValue(expected, actual, "ExchangedDocument/TypeCode", "Document type code");
        AssertNodeValue(expected, actual, "ExchangedDocument/IssueDateTime/DateTimeString", "Invoice issue date");
    }

    private static void AssertHeaderNotes(XDocument expected, XDocument actual)
    {
        var expNotes = DescPath(expected.Root!, "ExchangedDocument", "IncludedNote").ToList();
        var actNotes = DescPath(actual.Root!, "ExchangedDocument", "IncludedNote").ToList();

        Assert.AreEqual(expNotes.Count, actNotes.Count, "Header note count is different.");

        for (int i = 0; i < expNotes.Count; i++)
        {
            Assert.AreEqual(ChildValue(expNotes[i], "Content"), ChildValue(actNotes[i], "Content"), $"Header note content at index {i + 1} is different.");
            Assert.AreEqual(ChildValue(expNotes[i], "SubjectCode"), ChildValue(actNotes[i], "SubjectCode"), $"Header note subject code at index {i + 1} is different.");
            Assert.AreEqual(ChildValue(expNotes[i], "ContentCode"), ChildValue(actNotes[i], "ContentCode"), $"Header note content code at index {i + 1} is different.");
        }
    }

    private static void AssertHeaderTradeAgreement(XDocument expected, XDocument actual)
    {
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeAgreement/BuyerReference", "BuyerReference");

        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeAgreement/SellerTradeParty/ID", "Seller ID");
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeAgreement/SellerTradeParty/GlobalID", "Seller GlobalID");
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeAgreement/SellerTradeParty/Name", "Seller name");
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeAgreement/SellerTradeParty/SpecifiedLegalOrganization/ID", "Seller legal organization ID");
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeAgreement/SellerTradeParty/DefinedTradeContact/PersonName", "Seller contact person");
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeAgreement/SellerTradeParty/DefinedTradeContact/TelephoneUniversalCommunication/CompleteNumber", "Seller contact phone");
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeAgreement/SellerTradeParty/DefinedTradeContact/EmailURIUniversalCommunication/URIID", "Seller contact email");
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeAgreement/SellerTradeParty/PostalTradeAddress/PostcodeCode", "Seller postcode");
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeAgreement/SellerTradeParty/PostalTradeAddress/LineOne", "Seller street");
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeAgreement/SellerTradeParty/PostalTradeAddress/CityName", "Seller city");
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeAgreement/SellerTradeParty/PostalTradeAddress/CountryID", "Seller country");
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeAgreement/SellerTradeParty/URIUniversalCommunication/URIID", "Seller general email / electronic address");
        AssertTaxRegistrations(
            DescPath(expected.Root!, "SupplyChainTradeTransaction", "ApplicableHeaderTradeAgreement", "SellerTradeParty", "SpecifiedTaxRegistration").ToList(),
            DescPath(actual.Root!, "SupplyChainTradeTransaction", "ApplicableHeaderTradeAgreement", "SellerTradeParty", "SpecifiedTaxRegistration").ToList(),
            "Seller");

        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeAgreement/BuyerTradeParty/ID", "Buyer ID");
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeAgreement/BuyerTradeParty/GlobalID", "Buyer GlobalID");
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeAgreement/BuyerTradeParty/Name", "Buyer name");
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeAgreement/BuyerTradeParty/SpecifiedLegalOrganization/ID", "Buyer legal organization ID");
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeAgreement/BuyerTradeParty/DefinedTradeContact/PersonName", "Buyer contact person");
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeAgreement/BuyerTradeParty/DefinedTradeContact/TelephoneUniversalCommunication/CompleteNumber", "Buyer contact phone");
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeAgreement/BuyerTradeParty/DefinedTradeContact/EmailURIUniversalCommunication/URIID", "Buyer contact email");
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeAgreement/BuyerTradeParty/PostalTradeAddress/PostcodeCode", "Buyer postcode");
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeAgreement/BuyerTradeParty/PostalTradeAddress/LineOne", "Buyer street");
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeAgreement/BuyerTradeParty/PostalTradeAddress/CityName", "Buyer city");
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeAgreement/BuyerTradeParty/PostalTradeAddress/CountryID", "Buyer country");
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeAgreement/BuyerTradeParty/URIUniversalCommunication/URIID", "Buyer general email / electronic address");
        AssertTaxRegistrations(
            DescPath(expected.Root!, "SupplyChainTradeTransaction", "ApplicableHeaderTradeAgreement", "BuyerTradeParty", "SpecifiedTaxRegistration").ToList(),
            DescPath(actual.Root!, "SupplyChainTradeTransaction", "ApplicableHeaderTradeAgreement", "BuyerTradeParty", "SpecifiedTaxRegistration").ToList(),
            "Buyer");

        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeAgreement/SellerOrderReferencedDocument/IssuerAssignedID", "Seller order reference");
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeAgreement/BuyerOrderReferencedDocument/IssuerAssignedID", "Buyer order reference");
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeAgreement/ContractReferencedDocument/IssuerAssignedID", "Contract reference");
    }

    private static void AssertHeaderTradeDelivery(XDocument expected, XDocument actual)
    {
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeDelivery/ShipToTradeParty/Name", "ShipTo name");
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeDelivery/ShipToTradeParty/PostalTradeAddress/PostcodeCode", "ShipTo postcode");
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeDelivery/ShipToTradeParty/PostalTradeAddress/LineOne", "ShipTo street");
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeDelivery/ShipToTradeParty/PostalTradeAddress/CityName", "ShipTo city");
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeDelivery/ShipToTradeParty/PostalTradeAddress/CountryID", "ShipTo country");
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeDelivery/ActualDeliverySupplyChainEvent/OccurrenceDateTime/DateTimeString", "Actual delivery date");
    }

    private static void AssertHeaderTradeSettlement(XDocument expected, XDocument actual)
    {
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeSettlement/PaymentReference", "Payment reference");
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeSettlement/InvoiceCurrencyCode", "Invoice currency code");

        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeSettlement/SpecifiedTradeSettlementPaymentMeans/TypeCode", "Payment means type code");
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeSettlement/SpecifiedTradeSettlementPaymentMeans/Information", "Payment means information");
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeSettlement/SpecifiedTradeSettlementPaymentMeans/PayeePartyCreditorFinancialAccount/IBANID", "IBAN");
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeSettlement/SpecifiedTradeSettlementPaymentMeans/PayeePartyCreditorFinancialAccount/AccountName", "Account name");
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeSettlement/SpecifiedTradeSettlementPaymentMeans/PayeeSpecifiedCreditorFinancialInstitution/BICID", "BIC");

        var expTerms = DescPath(expected.Root!, "SupplyChainTradeTransaction", "ApplicableHeaderTradeSettlement", "SpecifiedTradePaymentTerms").ToList();
        var actTerms = DescPath(actual.Root!, "SupplyChainTradeTransaction", "ApplicableHeaderTradeSettlement", "SpecifiedTradePaymentTerms").ToList();

        Assert.AreEqual(expTerms.Count, actTerms.Count, "Payment terms count is different.");

        for (int i = 0; i < expTerms.Count; i++)
        {
            Assert.AreEqual(ChildValue(expTerms[i], "Description"), ChildValue(actTerms[i], "Description"), $"Payment term description at index {i + 1} is different.");
            Assert.AreEqual(DescValue(expTerms[i], "DueDateDateTime", "DateTimeString"), DescValue(actTerms[i], "DueDateDateTime", "DateTimeString"), $"Payment term due date at index {i + 1} is different.");
            Assert.AreEqual(DescValue(expTerms[i], "ApplicableTradePaymentDiscountTerms", "BasisDateTime", "DateTimeString"), DescValue(actTerms[i], "ApplicableTradePaymentDiscountTerms", "BasisDateTime", "DateTimeString"), $"Payment discount basis date at index {i + 1} is different.");
            Assert.AreEqual(DescValue(expTerms[i], "ApplicableTradePaymentDiscountTerms", "BasisPeriodMeasure"), DescValue(actTerms[i], "ApplicableTradePaymentDiscountTerms", "BasisPeriodMeasure"), $"Payment discount period at index {i + 1} is different.");
            Assert.AreEqual(DescValue(expTerms[i], "ApplicableTradePaymentDiscountTerms", "BasisAmount"), DescValue(actTerms[i], "ApplicableTradePaymentDiscountTerms", "BasisAmount"), $"Payment discount basis amount at index {i + 1} is different.");
            Assert.AreEqual(DecimalFromString(DescValue(expTerms[i], "ApplicableTradePaymentDiscountTerms", "CalculationPercent")), DecimalFromString(DescValue(actTerms[i], "ApplicableTradePaymentDiscountTerms", "CalculationPercent")), $"Payment discount percent at index {i + 1} is different.");
            Assert.AreEqual(DescValue(expTerms[i], "ApplicableTradePaymentDiscountTerms", "ActualDiscountAmount"), DescValue(actTerms[i], "ApplicableTradePaymentDiscountTerms", "ActualDiscountAmount"), $"Payment discount amount at index {i + 1} is different.");
        }

        AssertDecimalNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeSettlement/SpecifiedTradeSettlementHeaderMonetarySummation/LineTotalAmount", "Header LineTotalAmount");
        AssertDecimalNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeSettlement/SpecifiedTradeSettlementHeaderMonetarySummation/ChargeTotalAmount", "Header ChargeTotalAmount");
        AssertDecimalNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeSettlement/SpecifiedTradeSettlementHeaderMonetarySummation/AllowanceTotalAmount", "Header AllowanceTotalAmount");
        AssertDecimalNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeSettlement/SpecifiedTradeSettlementHeaderMonetarySummation/TaxBasisTotalAmount", "Header TaxBasisTotalAmount");
        AssertDecimalNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeSettlement/SpecifiedTradeSettlementHeaderMonetarySummation/TaxTotalAmount", "Header TaxTotalAmount");
        AssertDecimalNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeSettlement/SpecifiedTradeSettlementHeaderMonetarySummation/GrandTotalAmount", "Header GrandTotalAmount");
        AssertDecimalNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeSettlement/SpecifiedTradeSettlementHeaderMonetarySummation/TotalPrepaidAmount", "Header TotalPrepaidAmount");
        AssertDecimalNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeSettlement/SpecifiedTradeSettlementHeaderMonetarySummation/DuePayableAmount", "Header DuePayableAmount");
    }

    private static void AssertHeaderTaxes(XDocument expected, XDocument actual)
    {
        var expTaxes = DescPath(expected.Root!, "SupplyChainTradeTransaction", "ApplicableHeaderTradeSettlement", "ApplicableTradeTax").ToList();
        var actTaxes = DescPath(actual.Root!, "SupplyChainTradeTransaction", "ApplicableHeaderTradeSettlement", "ApplicableTradeTax").ToList();

        Assert.AreEqual(expTaxes.Count, actTaxes.Count, "Header tax group count is different.");

        foreach (var expTax in expTaxes)
        {
            decimal expRate = DecimalFromString(ChildValue(expTax, "RateApplicablePercent"));

            var actTax = actTaxes.FirstOrDefault(t => DecimalFromString(ChildValue(t, "RateApplicablePercent")) == expRate);

            Assert.IsNotNull(actTax, $"No header tax group found for rate {expRate:0.##}.");

            Assert.AreEqual(ChildValue(expTax, "TypeCode"), ChildValue(actTax!, "TypeCode"), $"Header tax TypeCode at {expRate:0.##}% is different.");
            Assert.AreEqual(ChildValue(expTax, "CategoryCode"), ChildValue(actTax!, "CategoryCode"), $"Header tax CategoryCode at {expRate:0.##}% is different.");
            Assert.AreEqual(DecimalFromString(ChildValue(expTax, "BasisAmount")), DecimalFromString(ChildValue(actTax!, "BasisAmount")), $"Header tax BasisAmount at {expRate:0.##}% is different.");
            Assert.AreEqual(DecimalFromString(ChildValue(expTax, "CalculatedAmount")), DecimalFromString(ChildValue(actTax!, "CalculatedAmount")), $"Header tax CalculatedAmount at {expRate:0.##}% is different.");
        }
    }

    private static void AssertLines(XDocument expected, XDocument actual)
    {
        var expLines = DescPath(expected.Root!, "SupplyChainTradeTransaction", "IncludedSupplyChainTradeLineItem").ToList();
        var actLines = DescPath(actual.Root!, "SupplyChainTradeTransaction", "IncludedSupplyChainTradeLineItem").ToList();

        Assert.AreEqual(expLines.Count, actLines.Count, "Line count is different.");

        for (int i = 0; i < expLines.Count; i++)
        {
            var e = expLines[i];
            var a = actLines[i];

            Assert.AreEqual(DescValue(e, "AssociatedDocumentLineDocument", "LineID"), DescValue(a, "AssociatedDocumentLineDocument", "LineID"), $"LineID in row {i + 1} is different.");
            Assert.AreEqual(DescValue(e, "AssociatedDocumentLineDocument", "ParentLineID"), DescValue(a, "AssociatedDocumentLineDocument", "ParentLineID"), $"ParentLineID in row {i + 1} is different.");
            Assert.AreEqual(DescValue(e, "AssociatedDocumentLineDocument", "LineStatusReasonCode"), DescValue(a, "AssociatedDocumentLineDocument", "LineStatusReasonCode"), $"LineStatusReasonCode in row {i + 1} is different.");

            Assert.AreEqual(DescValue(e, "SpecifiedTradeProduct", "GlobalID"), DescValue(a, "SpecifiedTradeProduct", "GlobalID"), $"GlobalID in row {i + 1} is different.");
            Assert.AreEqual(DescAttrValue(e, new[] { "SpecifiedTradeProduct", "GlobalID" }, "schemeID"), DescAttrValue(a, new[] { "SpecifiedTradeProduct", "GlobalID" }, "schemeID"), $"GlobalID schemeID in row {i + 1} is different.");
            Assert.AreEqual(DescValue(e, "SpecifiedTradeProduct", "SellerAssignedID"), DescValue(a, "SpecifiedTradeProduct", "SellerAssignedID"), $"SellerAssignedID in row {i + 1} is different.");
            Assert.AreEqual(DescValue(e, "SpecifiedTradeProduct", "BuyerAssignedID"), DescValue(a, "SpecifiedTradeProduct", "BuyerAssignedID"), $"BuyerAssignedID in row {i + 1} is different.");
            Assert.AreEqual(DescValue(e, "SpecifiedTradeProduct", "Name"), DescValue(a, "SpecifiedTradeProduct", "Name"), $"Product name in row {i + 1} is different.");
            Assert.AreEqual(DescValue(e, "SpecifiedTradeProduct", "Description"), DescValue(a, "SpecifiedTradeProduct", "Description"), $"Product description in row {i + 1} is different.");

            Assert.AreEqual(DescValue(e, "SpecifiedLineTradeAgreement", "BuyerOrderReferencedDocument", "IssuerAssignedID"), DescValue(a, "SpecifiedLineTradeAgreement", "BuyerOrderReferencedDocument", "IssuerAssignedID"), $"Buyer order reference in row {i + 1} is different.");
            Assert.AreEqual(DescValue(e, "SpecifiedLineTradeAgreement", "BuyerOrderReferencedDocument", "FormattedIssueDateTime", "DateTimeString"), DescValue(a, "SpecifiedLineTradeAgreement", "BuyerOrderReferencedDocument", "FormattedIssueDateTime", "DateTimeString"), $"Buyer order date in row {i + 1} is different.");
            Assert.AreEqual(DecimalFromString(DescValue(e, "SpecifiedLineTradeAgreement", "GrossPriceProductTradePrice", "ChargeAmount")), DecimalFromString(DescValue(a, "SpecifiedLineTradeAgreement", "GrossPriceProductTradePrice", "ChargeAmount")), $"Gross price in row {i + 1} is different.");
            Assert.AreEqual(DecimalFromString(DescValue(e, "SpecifiedLineTradeAgreement", "GrossPriceProductTradePrice", "BasisQuantity")), DecimalFromString(DescValue(a, "SpecifiedLineTradeAgreement", "GrossPriceProductTradePrice", "BasisQuantity")), $"Gross price basis quantity in row {i + 1} is different.");
            Assert.AreEqual(DecimalFromString(DescValue(e, "SpecifiedLineTradeAgreement", "NetPriceProductTradePrice", "ChargeAmount")), DecimalFromString(DescValue(a, "SpecifiedLineTradeAgreement", "NetPriceProductTradePrice", "ChargeAmount")), $"Net price in row {i + 1} is different.");
            Assert.AreEqual(DecimalFromString(DescValue(e, "SpecifiedLineTradeAgreement", "NetPriceProductTradePrice", "BasisQuantity")), DecimalFromString(DescValue(a, "SpecifiedLineTradeAgreement", "NetPriceProductTradePrice", "BasisQuantity")), $"Net price basis quantity in row {i + 1} is different.");

            Assert.AreEqual(DecimalFromString(DescValue(e, "SpecifiedLineTradeDelivery", "BilledQuantity")), DecimalFromString(DescValue(a, "SpecifiedLineTradeDelivery", "BilledQuantity")), $"Quantity in row {i + 1} is different.");
            Assert.AreEqual(DescAttrValue(e, new[] { "SpecifiedLineTradeDelivery", "BilledQuantity" }, "unitCode"), DescAttrValue(a, new[] { "SpecifiedLineTradeDelivery", "BilledQuantity" }, "unitCode"), $"Unit code in row {i + 1} is different.");
            Assert.AreEqual(DescValue(e, "SpecifiedLineTradeDelivery", "DeliveryNoteReferencedDocument", "IssuerAssignedID"), DescValue(a, "SpecifiedLineTradeDelivery", "DeliveryNoteReferencedDocument", "IssuerAssignedID"), $"Delivery note number in row {i + 1} is different.");
            Assert.AreEqual(DescValue(e, "SpecifiedLineTradeDelivery", "DeliveryNoteReferencedDocument", "LineID"), DescValue(a, "SpecifiedLineTradeDelivery", "DeliveryNoteReferencedDocument", "LineID"), $"Delivery note line ID in row {i + 1} is different.");
            Assert.AreEqual(DescValue(e, "SpecifiedLineTradeDelivery", "DeliveryNoteReferencedDocument", "FormattedIssueDateTime", "DateTimeString"), DescValue(a, "SpecifiedLineTradeDelivery", "DeliveryNoteReferencedDocument", "FormattedIssueDateTime", "DateTimeString"), $"Delivery note date in row {i + 1} is different.");

            Assert.AreEqual(DescValue(e, "SpecifiedLineTradeSettlement", "ApplicableTradeTax", "TypeCode"), DescValue(a, "SpecifiedLineTradeSettlement", "ApplicableTradeTax", "TypeCode"), $"Tax type in row {i + 1} is different.");
            Assert.AreEqual(DescValue(e, "SpecifiedLineTradeSettlement", "ApplicableTradeTax", "CategoryCode"), DescValue(a, "SpecifiedLineTradeSettlement", "ApplicableTradeTax", "CategoryCode"), $"Tax category in row {i + 1} is different.");
            Assert.AreEqual(DecimalFromString(DescValue(e, "SpecifiedLineTradeSettlement", "ApplicableTradeTax", "RateApplicablePercent")), DecimalFromString(DescValue(a, "SpecifiedLineTradeSettlement", "ApplicableTradeTax", "RateApplicablePercent")), $"Tax percent in row {i + 1} is different.");
            Assert.AreEqual(DescValue(e, "SpecifiedLineTradeSettlement", "BillingSpecifiedPeriod", "EndDateTime", "DateTimeString"), DescValue(a, "SpecifiedLineTradeSettlement", "BillingSpecifiedPeriod", "EndDateTime", "DateTimeString"), $"Billing period end date in row {i + 1} is different.");
            Assert.AreEqual(DecimalFromString(DescValue(e, "SpecifiedLineTradeSettlement", "SpecifiedTradeSettlementLineMonetarySummation", "LineTotalAmount")), DecimalFromString(DescValue(a, "SpecifiedLineTradeSettlement", "SpecifiedTradeSettlementLineMonetarySummation", "LineTotalAmount")), $"Line total amount in row {i + 1} is different.");
            Assert.AreEqual(DecimalFromString(DescValue(e, "SpecifiedLineTradeSettlement", "SpecifiedTradeSettlementLineMonetarySummation", "TotalAllowanceChargeAmount")), DecimalFromString(DescValue(a, "SpecifiedLineTradeSettlement", "SpecifiedTradeSettlementLineMonetarySummation", "TotalAllowanceChargeAmount")), $"Line allowance/charge amount in row {i + 1} is different.");
            Assert.AreEqual(DescValue(e, "SpecifiedLineTradeSettlement", "AdditionalReferencedDocument", "IssuerAssignedID"), DescValue(a, "SpecifiedLineTradeSettlement", "AdditionalReferencedDocument", "IssuerAssignedID"), $"Additional referenced document ID in row {i + 1} is different.");
            Assert.AreEqual(DescValue(e, "SpecifiedLineTradeSettlement", "AdditionalReferencedDocument", "TypeCode"), DescValue(a, "SpecifiedLineTradeSettlement", "AdditionalReferencedDocument", "TypeCode"), $"Additional referenced document TypeCode in row {i + 1} is different.");
            Assert.AreEqual(DescValue(e, "SpecifiedLineTradeSettlement", "AdditionalReferencedDocument", "ReferenceTypeCode"), DescValue(a, "SpecifiedLineTradeSettlement", "AdditionalReferencedDocument", "ReferenceTypeCode"), $"Additional referenced document ReferenceTypeCode in row {i + 1} is different.");

            var expLineNotes = DescPath(e, "AssociatedDocumentLineDocument", "IncludedNote").ToList();
            var actLineNotes = DescPath(a, "AssociatedDocumentLineDocument", "IncludedNote").ToList();

            Assert.AreEqual(expLineNotes.Count, actLineNotes.Count, $"Line note count in row {i + 1} is different.");

            for (int n = 0; n < expLineNotes.Count; n++)
            {
                Assert.AreEqual(ChildValue(expLineNotes[n], "Content"), ChildValue(actLineNotes[n], "Content"), $"Line note content in row {i + 1}, note {n + 1} is different.");
                Assert.AreEqual(ChildValue(expLineNotes[n], "SubjectCode"), ChildValue(actLineNotes[n], "SubjectCode"), $"Line note subject code in row {i + 1}, note {n + 1} is different.");
            }
        }
    }

    private static void AssertTaxRegistrations(List<XElement> expected, List<XElement> actual, string label)
    {
        Assert.AreEqual(expected.Count, actual.Count, $"{label} tax registration count is different.");

        for (int i = 0; i < expected.Count; i++)
        {
            Assert.AreEqual(ChildValue(expected[i], "ID"), ChildValue(actual[i], "ID"), $"{label} tax registration ID at index {i + 1} is different.");
            Assert.AreEqual(
                expected[i].Elements().FirstOrDefault(e => e.Name.LocalName == "ID")?.Attributes().FirstOrDefault(a => a.Name.LocalName == "schemeID")?.Value ?? string.Empty,
                actual[i].Elements().FirstOrDefault(e => e.Name.LocalName == "ID")?.Attributes().FirstOrDefault(a => a.Name.LocalName == "schemeID")?.Value ?? string.Empty,
                $"{label} tax registration schemeID at index {i + 1} is different.");
        }
    }

    private static void AssertDecimalNodeValue(XDocument expected, XDocument actual, string slashPath, string label)
    {
        decimal expectedValue = GetPathDecimalValue(expected.Root!, slashPath);
        decimal actualValue = GetPathDecimalValue(actual.Root!, slashPath);

        Assert.AreEqual(decimal.Round(expectedValue, 2), decimal.Round(actualValue, 2), $"{label} is different.");
    }

    private static decimal GetPathDecimalValue(XElement start, string slashPath)
    {
        string raw = GetPathValue(start, slashPath);

        if (string.IsNullOrWhiteSpace(raw))
            return 0m;

        return decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var value)
            ? value
            : 0m;
    }

    private static decimal DecimalFromString(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return 0m;

        return decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var value)
            ? decimal.Round(value, 2)
            : 0m;
    }

    private static void AssertNodeValue(XDocument expected, XDocument actual, string slashPath, string label)
    {
        string expectedValue = GetPathValue(expected.Root!, slashPath);
        string actualValue = GetPathValue(actual.Root!, slashPath);

        Assert.AreEqual(expectedValue, actualValue, $"{label} is different.");
    }

    private static string GetPathValue(XElement start, string slashPath)
    {
        var parts = slashPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        XElement? current = start;

        foreach (var part in parts)
        {
            current = current.Elements().FirstOrDefault(e => e.Name.LocalName == part);
            if (current == null)
                return string.Empty;
        }

        return NormalizeValue(current.Value);
    }

    private static IEnumerable<XElement> DescPath(XElement start, params string[] parts)
    {
        XElement? current = start;

        foreach (var part in parts.Take(parts.Length - 1))
        {
            current = current?.Elements().FirstOrDefault(e => e.Name.LocalName == part);
            if (current == null)
                return Enumerable.Empty<XElement>();
        }

        return current?.Elements().Where(e => e.Name.LocalName == parts.Last()) ?? Enumerable.Empty<XElement>();
    }

    private static string DescValue(XElement start, params string[] parts)
    {
        XElement? current = start;

        foreach (var part in parts)
        {
            current = current?.Elements().FirstOrDefault(e => e.Name.LocalName == part);
            if (current == null)
                return string.Empty;
        }

        return NormalizeValue(current.Value);
    }

    private static string DescAttrValue(XElement start, string[] parts, string attributeName)
    {
        XElement? current = start;

        foreach (var part in parts)
        {
            current = current?.Elements().FirstOrDefault(e => e.Name.LocalName == part);
            if (current == null)
                return string.Empty;
        }

        return NormalizeValue(current.Attributes().FirstOrDefault(a => a.Name.LocalName == attributeName)?.Value);
    }

    private static string ChildValue(XElement element, string childName)
    {
        return NormalizeValue(element.Elements().FirstOrDefault(e => e.Name.LocalName == childName)?.Value);
    }

    private static string NormalizeValue(string? value)
    {
        return (value ?? string.Empty).Trim();
    }

    private static string SaveXmlFile(Invoice invoice, string xml)
    {
        string tempDir = Path.GetTempPath();
        string fileName = $"XmlCii_{invoice.InvoiceNumber}_{Guid.NewGuid():N}.xml";
        string filePath = Path.Combine(tempDir, fileName);

        File.WriteAllText(filePath, xml, Encoding.UTF8);
        return filePath;
    }
    #endregion
}

