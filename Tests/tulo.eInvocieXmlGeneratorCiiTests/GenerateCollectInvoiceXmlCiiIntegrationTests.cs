using System.Globalization;
using System.Text.Json;
using System.Xml.Linq;
using tulo.eInvoiceXmlGeneratorCii.Mappers;
using tulo.eInvoiceXmlGeneratorCii.Models;
using tulo.eInvoiceXmlGeneratorCii.Services;

namespace Tests;

[TestClass]
public class GenerateCollectInvoiceXmlCiiIntegrationTests
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

    [TestMethod(DisplayName = "Verifies it creates ZUGFeRD invoice equal to official extended collect invoice sample")]
    [DoNotParallelize]
    public void Generated_invoice_matches_official_extended_collective_invoice()
    {
        // Arrange
        var invoice = BuildInvoiceMatchingOfficialSample();

        // Act + Assert
        AssertGeneratedInvoiceMatchesOfficialSample(invoice);
    }

    [TestMethod(DisplayName = "Verifies it creates ZUGFeRD invoice equal to official extended collect invoice sample from JSON example")]
    [DoNotParallelize]
    public void Generated_invoice_from_json_matches_official_extended_collective_invoice()
    {
        // Arrange
        var invoice = LoadInvoiceFromJsonExample("ZF_Extended__Sammelrechnung_3_Bestellungen.json");

        // Act + Assert
        AssertGeneratedInvoiceMatchesOfficialSample(invoice);
    }

    #region Utilities ZF_Extended__Sammelrechnung_3_Bestellungen
    private void AssertGeneratedInvoiceMatchesOfficialSample(Invoice invoice)
    {
        var cii = _mapper.Map(invoice);
        string xml = _exporter.ToXml(cii);

        CiiSchemaValidator.ValidateCiiZugferd24Extended(xml);
        SaveXmlFile(invoice, xml);

        string sampleXml = File.ReadAllText(GetExampleFilePath("Examples", "ZF_Extended__Sammelrechnung_3_Bestellungen.xml"));

        var expected = XDocument.Parse(sampleXml);
        var actual = XDocument.Parse(xml);

        AssertHeader(expected, actual);
        AssertHeaderNotes(expected, actual);
        AssertHeaderTradeAgreement(expected, actual);
        AssertHeaderTradeSettlement(expected, actual);
        AssertHeaderTaxes(expected, actual);
        AssertLines(expected, actual);
    }

    private static Invoice LoadInvoiceFromJsonExample(string fileName)
    {
        string json = File.ReadAllText(GetExampleFilePath("JsonExamples", fileName));

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var invoice = JsonSerializer.Deserialize<Invoice>(json, options);

        Assert.IsNotNull(invoice, $"Invoice JSON could not be deserialized: {fileName}");

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

    private static string GetExampleFilePath(params string[] segments)
    {
        string baseDir = AppContext.BaseDirectory;
        string path = Path.Combine(new[] { baseDir }.Concat(segments).ToArray());

        Assert.IsTrue(File.Exists(path), $"Example file was not found: {path}");

        return path;
    }

    private static Invoice BuildInvoiceMatchingOfficialSample()
    {
        var invoice = new Invoice
        {
            InvoiceNumber = "6063636771001",
            InvoiceDate = new DateTime(2026, 08, 27),
            Currency = "EUR",
            DocumentName = "RECHNUNG",
            DocumentTypeCode = "380",
            BuyerReference = "BUYERREFERENCE BT-10",

            HeaderChargeTotalAmount = 0.00m,
            HeaderAllowanceTotalAmount = 0.00m,
            HeaderTotalPrepaidAmount = 0.00m,
            HeaderDuePayableAmount = 655.38m,

            Seller = new Party
            {
                ID = "10737",
                GlobalId = "0001231000000",
                GlobalIdSchemeId = "0088",
                Name = "Lieferant GmbH & Co. KG",
                Street = "Lieferanten-Strasse.12-17",
                Zip = "98765",
                City = "Lieferantenstadt",
                CountryCode = "DE",
                VatId = "DE946280061",
                TaxRegistrationFcId = "88888/00072",
                GeneralEmail = "info@lieferant.com",
                ContactPersonName = "Tim Kleine",
                ContactPhone = "0170 66677788",
                ContactEmail = "tim.kleine@lieferant.com"
            },

            Buyer = new Party
            {
                ID = "9900880077",
                Name = "Musterkunde GmbH & Co Name 2 Musterkunde",
                Street = "Musterstrasse 44",
                Zip = "40789",
                City = "Musterstadt",
                CountryCode = "DE",
                VatId = "DE2012129398",
                ContactPersonName = "Herr Test Monteur",
                ContactPhone = "02173 9364",
                ContactEmail = "mike.maier@lieferant.com"
            },

            Payment = new PaymentDetails
            {
                PaymentReference = "Kundennummer:. 9900880077 Rechnungsnummer:. 6063636771001",
                PaymentMeansTypeCode = "58",
                PaymentMeansInformation = "Bezahlung per SEPA Überweisung",
                Iban = "DE33600501010001234567",
                Bic = "SOLADEST600",
                AccountName = "",
                Terms = new List<PaymentTermDetails>
                    {
                        new PaymentTermDetails
                        {
                            Description = "Bis zum 06.09.2026 erhalten Sie 2,000 % Skonto",
                            DiscountTerms = new PaymentDiscountTermsDetails
                            {
                                BasisDate = new DateTime(2026, 08, 27),
                                BasisPeriodDays = 10,
                                BasisAmount = 655.38m,
                                CalculationPercent = 2.000m,
                                ActualDiscountAmount = 13.11m
                            }
                        },
                        new PaymentTermDetails
                        {
                            Description = "Bis zum 16.09.2026 ohne Abzug",
                            DueDate = new DateTime(2026, 09, 16)
                        }
                    }
            }
        };

        invoice.Notes.AddRange(new[]
        {
                new InvoiceNote
                {
                    Content = "Bank: LBBW Stuttgart BLZ: 999 888 77 Kto.Nr.: 1234567, IBAN: DE33 9998 8877 0001 2345 67 BIC / Swift-Code: SOLADEST600",
                    SubjectCode = "REG"
                },
                new InvoiceNote
                {
                    Content = "Lieferant GmbH & Co. KG - 98765 Lieferantenstadt - T +49 (0)7891 11-0 - F +49(0)7891 11-1000 - info@lieferant.com - www.lieferant.com Hausanschrift: Lieferanten-Strasse 12-17 - 98765 Lieferantenstadt - Sitz Lieferantenstadt, Amtsgericht Stuttgart HRA 123456 Geschäftsführer: ",
                    SubjectCode = "REG"
                },
                new InvoiceNote
                {
                    Content = "Wenn nicht anders angegeben entspricht das Leistungsdatum dem Rechnungsdatum.",
                    SubjectCode = "AAI"
                },
                new InvoiceNote
                {
                    Content = "Haben Sie Fragen zur Rechnung? Gerne hilft Ihnen Ihr zuständiger Lieferant Verkäufer weiter. Die Lieferung erfolgte zu unseren bekannten Verkaufs- und Lieferbedingungen. Bitte beachten Sie hierzu unsere allgemeinen Geschäftsbedingungen unter www.lieferant.de/agb.",
                    SubjectCode = "AAI"
                },
                new InvoiceNote
                {
                    Content = "Zahlungsavis/Aufstellung/Auflistung zur Zahlung bitte an: E-mail: zahlungseingang@lieferant.com oder Fax +49 7891 11-59333",
                    SubjectCode = "PMT"
                },
                new InvoiceNote
                {
                    Content = "ZUGFeRD vers 2.4.0 Extended",
                    SubjectCode = "ACB"
                },
                new InvoiceNote
                {
                    Content = "Collective Invoice",
                    SubjectCode = "ACB"
                }
            });

        // Row 1
        invoice.Lines.Add(new InvoiceLine
        {
            LineId = "000001",
            Description = "GWDSTG-DIN976-A-4.8-(A2K)-M10X1000",
            ProductDescription = "Gewindestange",
            GlobalId = "7711231873598",
            GlobalIdSchemeId = "0160",
            SellerAssignedId = "0595810 25",
            BuyerOrderReferencedId = "Abholung 1",
            BuyerOrderDate = new DateTime(2026, 08, 27),
            DeliveryNoteNumber = "8408230045",
            DeliveryNoteLineId = "000010",
            DeliveryNoteDate = new DateTime(2026, 08, 27),
            Quantity = 25m,
            UnitCode = "H87",
            UnitPrice = 2.06m,
            TaxPercent = 19m,
            TaxCategory = "S",
            BillingPeriodEndDate = new DateTime(2026, 08, 27),
            AdditionalReferencedDocumentId = "2156307416",
            AdditionalReferencedDocumentTypeCode = "130",
            AdditionalReferencedDocumentReferenceTypeCode = "VN",
            ForcedLineTotalAmount = 51.50m
        });

        // Row 2
        invoice.Lines.Add(new InvoiceLine
        {
            LineId = "000002",
            Description = "MUELLSACK-EXTRASTARK-BLAU-700X1100X0,07",
            ProductDescription = "Müllsack, -beutel",
            GlobalId = "7748539263943",
            GlobalIdSchemeId = "0160",
            SellerAssignedId = "05899800555 150",
            BuyerAssignedId = "KD-MAT POS 1 BT-156",
            BuyerOrderReferencedId = "Abholung 1",
            BuyerOrderDate = new DateTime(2026, 08, 27),
            DeliveryNoteNumber = "8408230045",
            DeliveryNoteLineId = "000020",
            DeliveryNoteDate = new DateTime(2026, 08, 27),
            Quantity = 150m,
            UnitCode = "H87",
            UnitPrice = 0.4929m,
            PriceBasisQuantity = 100m,
            TaxPercent = 19m,
            TaxCategory = "S",
            BillingPeriodEndDate = new DateTime(2026, 08, 27),
            AdditionalReferencedDocumentId = "2156307416",
            AdditionalReferencedDocumentTypeCode = "130",
            AdditionalReferencedDocumentReferenceTypeCode = "VN",
            ForcedLineTotalAmount = 73.94m
        });

        // Row 3
        invoice.Lines.Add(new InvoiceLine
        {
            LineId = "000003",
            Description = "SHR-AW30-(A2K)-7,5X152",
            ProductDescription = "Abstandsmontageschraube Rahmen",
            GlobalId = "7738898142591",
            GlobalIdSchemeId = "0160",
            SellerAssignedId = "05234830152 200",
            BuyerOrderReferencedId = "Abholung 2",
            BuyerOrderDate = new DateTime(2026, 08, 27),
            DeliveryNoteNumber = "8408230046",
            DeliveryNoteLineId = "000010",
            DeliveryNoteDate = new DateTime(2026, 08, 27),
            Quantity = 400m,
            UnitCode = "H87",
            PriceBasisQuantity = 100m,
            UnitPrice = 0.3276m,
            TaxPercent = 19m,
            TaxCategory = "S",
            BillingPeriodEndDate = new DateTime(2026, 08, 27),
            AdditionalReferencedDocumentId = "2156307417",
            AdditionalReferencedDocumentTypeCode = "130",
            AdditionalReferencedDocumentReferenceTypeCode = "VN",
            ForcedLineTotalAmount = 131.04m
        });

        // Row 4
        invoice.Lines.Add(new InvoiceLine
        {
            LineId = "000004",
            Description = "MU-6KT-DIN934-I8I-SW17-(A2K)-M10",
            ProductDescription = "Sechskantmutter",
            GlobalId = "7711231333337",
            GlobalIdSchemeId = "0160",
            SellerAssignedId = "0531710 100",
            BuyerAssignedId = "KD-MAT POS 1 BT-156",
            BuyerOrderReferencedId = "Abholung 2",
            BuyerOrderDate = new DateTime(2026, 08, 27),
            DeliveryNoteNumber = "8408230046",
            DeliveryNoteLineId = "000020",
            DeliveryNoteDate = new DateTime(2026, 08, 27),
            Quantity = 500m,
            UnitCode = "H87",
            UnitPrice = 0.0916m,
            PriceBasisQuantity = 100m,
            TaxPercent = 19m,
            TaxCategory = "S",
            BillingPeriodEndDate = new DateTime(2026, 08, 27),
            AdditionalReferencedDocumentId = "2156307417",
            AdditionalReferencedDocumentTypeCode = "130",
            AdditionalReferencedDocumentReferenceTypeCode = "VN",
            ForcedLineTotalAmount = 45.80m,
            Notes = new List<InvoiceNote> { new InvoiceNote { Content = "Test" } }
        });

        // Row 5
        invoice.Lines.Add(new InvoiceLine
        {
            LineId = "000005",
            Description = "MU-6KT-DIN934-I8I-SW19-(A2K)-M12",
            ProductDescription = "Sechskantmutter",
            GlobalId = "7711231333498",
            GlobalIdSchemeId = "0160",
            SellerAssignedId = "0531712 100",
            BuyerAssignedId = "KD-MAT POS 2 BT-156",
            BuyerOrderReferencedId = "Abholung 2",
            BuyerOrderDate = new DateTime(2026, 08, 27),
            DeliveryNoteNumber = "8408230046",
            DeliveryNoteLineId = "000030",
            DeliveryNoteDate = new DateTime(2026, 08, 27),
            Quantity = 500m,
            UnitCode = "H87",
            UnitPrice = 0.1332m,
            PriceBasisQuantity = 100m,
            TaxPercent = 19m,
            TaxCategory = "S",
            BillingPeriodEndDate = new DateTime(2026, 08, 27),
            AdditionalReferencedDocumentId = "2156307417",
            AdditionalReferencedDocumentTypeCode = "130",
            AdditionalReferencedDocumentReferenceTypeCode = "VN",
            ForcedLineTotalAmount = 66.60m
        });

        // Row 6
        invoice.Lines.Add(new InvoiceLine
        {
            LineId = "000006",
            Description = "MUELLSACK-EXTRASTARK-BLAU-700X1100X0,07",
            ProductDescription = "Müllsack, -beutel",
            GlobalId = "7748539263943",
            GlobalIdSchemeId = "0160",
            SellerAssignedId = "05899800555 150",
            BuyerAssignedId = "KD-MAT POS 1 BT-156",
            BuyerOrderReferencedId = "Abholung 3",
            BuyerOrderDate = new DateTime(2026, 08, 27),
            DeliveryNoteNumber = "8408230047",
            DeliveryNoteLineId = "000010",
            DeliveryNoteDate = new DateTime(2026, 08, 27),
            Quantity = 300m,
            UnitCode = "H87",
            UnitPrice = 0.4929m,
            PriceBasisQuantity = 100m,
            TaxPercent = 19m,
            TaxCategory = "S",
            BillingPeriodEndDate = new DateTime(2026, 08, 27),
            AdditionalReferencedDocumentId = "2156307418",
            AdditionalReferencedDocumentTypeCode = "130",
            AdditionalReferencedDocumentReferenceTypeCode = "VN",
            ForcedLineTotalAmount = 147.87m
        });

        // Zeile 7
        invoice.Lines.Add(new InvoiceLine
        {
            LineId = "000007",
            Description = "KAFFEE-ESPRESSO-GANZE-BOHNEN-1KG",
            ProductDescription = "Kaffee",
            GlobalId = "7765233128651",
            GlobalIdSchemeId = "0160",
            SellerAssignedId = "05988013679 10",
            BuyerOrderReferencedId = "Abholung 3",
            BuyerOrderDate = new DateTime(2026, 08, 27),
            DeliveryNoteNumber = "8408230047",
            DeliveryNoteLineId = "000020",
            DeliveryNoteDate = new DateTime(2026, 08, 27),
            Quantity = 2m,
            UnitCode = "H87",
            UnitPrice = 18.90m,
            TaxPercent = 7m,
            TaxCategory = "S",
            BillingPeriodEndDate = new DateTime(2026, 08, 27),
            AdditionalReferencedDocumentId = "2156307418",
            AdditionalReferencedDocumentTypeCode = "130",
            AdditionalReferencedDocumentReferenceTypeCode = "VN",
            ForcedLineTotalAmount = 37.80m
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
            Assert.AreEqual(
                ChildValue(expNotes[i], "Content"),
                ChildValue(actNotes[i], "Content"),
                $"Header note content at index {i + 1} is different.");

            Assert.AreEqual(
                ChildValue(expNotes[i], "SubjectCode"),
                ChildValue(actNotes[i], "SubjectCode"),
                $"Header note subject code at index {i + 1} is different.");

            Assert.AreEqual(
                ChildValue(expNotes[i], "ContentCode"),
                ChildValue(actNotes[i], "ContentCode"),
                $"Header note content code at index {i + 1} is different.");
        }
    }

    private static void AssertHeaderTradeAgreement(XDocument expected, XDocument actual)
    {
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeAgreement/BuyerReference", "BuyerReference");

        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeAgreement/SellerTradeParty/ID", "Seller ID");
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeAgreement/SellerTradeParty/GlobalID", "Seller GlobalID");
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeAgreement/SellerTradeParty/Name", "Seller name");
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeAgreement/SellerTradeParty/DefinedTradeContact/PersonName", "Seller contact person");
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeAgreement/SellerTradeParty/DefinedTradeContact/TelephoneUniversalCommunication/CompleteNumber", "Seller contact phone");
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeAgreement/SellerTradeParty/DefinedTradeContact/EmailURIUniversalCommunication/URIID", "Seller contact email");
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeAgreement/SellerTradeParty/PostalTradeAddress/PostcodeCode", "Seller postcode");
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeAgreement/SellerTradeParty/PostalTradeAddress/LineOne", "Seller street");
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeAgreement/SellerTradeParty/PostalTradeAddress/CityName", "Seller city");
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeAgreement/SellerTradeParty/PostalTradeAddress/CountryID", "Seller country");
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeAgreement/SellerTradeParty/URIUniversalCommunication/URIID", "Seller general email");

        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeAgreement/BuyerTradeParty/ID", "Buyer ID");
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeAgreement/BuyerTradeParty/Name", "Buyer name");
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeAgreement/BuyerTradeParty/DefinedTradeContact/PersonName", "Buyer contact person");
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeAgreement/BuyerTradeParty/DefinedTradeContact/TelephoneUniversalCommunication/CompleteNumber", "Buyer contact phone");
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeAgreement/BuyerTradeParty/DefinedTradeContact/EmailURIUniversalCommunication/URIID", "Buyer contact email");
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeAgreement/BuyerTradeParty/PostalTradeAddress/PostcodeCode", "Buyer postcode");
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeAgreement/BuyerTradeParty/PostalTradeAddress/LineOne", "Buyer street");
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeAgreement/BuyerTradeParty/PostalTradeAddress/CityName", "Buyer city");
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeAgreement/BuyerTradeParty/PostalTradeAddress/CountryID", "Buyer country");
    }

    private static void AssertHeaderTradeSettlement(XDocument expected, XDocument actual)
    {
        AssertNodeValue(expected, actual, "SupplyChainTradeTransaction/ApplicableHeaderTradeSettlement/PaymentReference", "Payment reference");

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
            Assert.AreEqual(
                ChildValue(expTerms[i], "Description"),
                ChildValue(actTerms[i], "Description"),
                $"Payment term description at index {i + 1} is different.");

            Assert.AreEqual(
                DescValue(expTerms[i], "DueDateDateTime", "DateTimeString"),
                DescValue(actTerms[i], "DueDateDateTime", "DateTimeString"),
                $"Payment term due date at index {i + 1} is different.");

            Assert.AreEqual(
                DescValue(expTerms[i], "ApplicableTradePaymentDiscountTerms", "BasisDateTime", "DateTimeString"),
                DescValue(actTerms[i], "ApplicableTradePaymentDiscountTerms", "BasisDateTime", "DateTimeString"),
                $"Payment discount basis date at index {i + 1} is different.");

            Assert.AreEqual(
                DescValue(expTerms[i], "ApplicableTradePaymentDiscountTerms", "BasisPeriodMeasure"),
                DescValue(actTerms[i], "ApplicableTradePaymentDiscountTerms", "BasisPeriodMeasure"),
                $"Payment discount period at index {i + 1} is different.");

            Assert.AreEqual(
                DescValue(expTerms[i], "ApplicableTradePaymentDiscountTerms", "BasisAmount"),
                DescValue(actTerms[i], "ApplicableTradePaymentDiscountTerms", "BasisAmount"),
                $"Payment discount basis amount at index {i + 1} is different.");

            Assert.AreEqual(
               ParseDecimal(DescValue(expTerms[i], "ApplicableTradePaymentDiscountTerms", "CalculationPercent")),
               ParseDecimal(DescValue(actTerms[i], "ApplicableTradePaymentDiscountTerms", "CalculationPercent")),
               $"Payment discount percent at index {i + 1} is different.");

            Assert.AreEqual(
                DescValue(expTerms[i], "ApplicableTradePaymentDiscountTerms", "ActualDiscountAmount"),
                DescValue(actTerms[i], "ApplicableTradePaymentDiscountTerms", "ActualDiscountAmount"),
                $"Payment discount amount at index {i + 1} is different.");
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

    private static void AssertDecimalNodeValue(XDocument expected, XDocument actual, string slashPath, string label)
    {
        decimal expectedValue = GetPathDecimalValue(expected.Root!, slashPath);
        decimal actualValue = GetPathDecimalValue(actual.Root!, slashPath);

        Assert.AreEqual(
            decimal.Round(expectedValue, 2),
            decimal.Round(actualValue, 2),
            $"{label} is different.");
    }
    private static decimal GetPathDecimalValue(XElement start, string slashPath)
    {
        string raw = GetPathValue(start, slashPath);

        if (string.IsNullOrWhiteSpace(raw))
            return 0m;

        return decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : 0m;
    }

    private static decimal ParseDecimal(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return 0m;

        return decimal.Parse(raw, NumberStyles.Any, CultureInfo.InvariantCulture);
    }

    private static void AssertHeaderTaxes(XDocument expected, XDocument actual)
    {
        var expTaxes = DescPath(expected.Root!, "SupplyChainTradeTransaction", "ApplicableHeaderTradeSettlement", "ApplicableTradeTax").ToList();
        var actTaxes = DescPath(actual.Root!, "SupplyChainTradeTransaction", "ApplicableHeaderTradeSettlement", "ApplicableTradeTax").ToList();

        Assert.AreEqual(expTaxes.Count, actTaxes.Count, "Header tax group count is different.");

        foreach (var expTax in expTaxes)
        {
            decimal expRate = DecimalFromString(ChildValue(expTax, "RateApplicablePercent"));

            var actTax = actTaxes.FirstOrDefault(t =>
                DecimalFromString(ChildValue(t, "RateApplicablePercent")) == expRate);

            Assert.IsNotNull(actTax, $"No header tax group found for rate {expRate:0.##}.");

            Assert.AreEqual(
                ChildValue(expTax, "TypeCode"),
                ChildValue(actTax!, "TypeCode"),
                $"Header tax TypeCode at {expRate:0.##}% is different.");

            Assert.AreEqual(
                ChildValue(expTax, "CategoryCode"),
                ChildValue(actTax!, "CategoryCode"),
                $"Header tax CategoryCode at {expRate:0.##}% is different.");

            Assert.AreEqual(
                DecimalFromString(ChildValue(expTax, "BasisAmount")),
                DecimalFromString(ChildValue(actTax!, "BasisAmount")),
                $"Header tax BasisAmount at {expRate:0.##}% is different.");

            Assert.AreEqual(
                DecimalFromString(ChildValue(expTax, "CalculatedAmount")),
                DecimalFromString(ChildValue(actTax!, "CalculatedAmount")),
                $"Header tax CalculatedAmount at {expRate:0.##}% is different.");
        }
    }

    private static decimal DecimalFromString(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return 0m;

        return decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? decimal.Round(value, 2) : 0m;
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

            Assert.AreEqual(
                DescValue(e, "AssociatedDocumentLineDocument", "LineID"),
                DescValue(a, "AssociatedDocumentLineDocument", "LineID"),
                $"LineID in row {i + 1} is different.");

            Assert.AreEqual(
                DescValue(e, "SpecifiedTradeProduct", "GlobalID"),
                DescValue(a, "SpecifiedTradeProduct", "GlobalID"),
                $"GlobalID in row {i + 1} is different.");

            Assert.AreEqual(
                DescValue(e, "SpecifiedTradeProduct", "SellerAssignedID"),
                DescValue(a, "SpecifiedTradeProduct", "SellerAssignedID"),
                $"SellerAssignedID in row {i + 1} is different.");

            Assert.AreEqual(
                DescValue(e, "SpecifiedTradeProduct", "BuyerAssignedID"),
                DescValue(a, "SpecifiedTradeProduct", "BuyerAssignedID"),
                $"BuyerAssignedID in row {i + 1} is different.");

            Assert.AreEqual(
                DescValue(e, "SpecifiedTradeProduct", "Name"),
                DescValue(a, "SpecifiedTradeProduct", "Name"),
                $"Product name in row {i + 1} is different.");

            Assert.AreEqual(
                DescValue(e, "SpecifiedTradeProduct", "Description"),
                DescValue(a, "SpecifiedTradeProduct", "Description"),
                $"Product description in row {i + 1} is different.");

            Assert.AreEqual(
                DescValue(e, "SpecifiedLineTradeAgreement", "BuyerOrderReferencedDocument", "IssuerAssignedID"),
                DescValue(a, "SpecifiedLineTradeAgreement", "BuyerOrderReferencedDocument", "IssuerAssignedID"),
                $"Buyer order reference in row {i + 1} is different.");

            Assert.AreEqual(
                DescValue(e, "SpecifiedLineTradeAgreement", "BuyerOrderReferencedDocument", "FormattedIssueDateTime", "DateTimeString"),
                DescValue(a, "SpecifiedLineTradeAgreement", "BuyerOrderReferencedDocument", "FormattedIssueDateTime", "DateTimeString"),
                $"Buyer order date in row {i + 1} is different.");

            Assert.AreEqual(
                DecimalFromString(DescValue(e, "SpecifiedLineTradeAgreement", "GrossPriceProductTradePrice", "ChargeAmount")),
                DecimalFromString(DescValue(a, "SpecifiedLineTradeAgreement", "GrossPriceProductTradePrice", "ChargeAmount")),
                $"Gross price in row {i + 1} is different.");

            Assert.AreEqual(
                DecimalFromString(DescValue(e, "SpecifiedLineTradeAgreement", "GrossPriceProductTradePrice", "BasisQuantity")),
                DecimalFromString(DescValue(a, "SpecifiedLineTradeAgreement", "GrossPriceProductTradePrice", "BasisQuantity")),
                $"Gross price basis quantity in row {i + 1} is different.");

            Assert.AreEqual(
                DecimalFromString(DescValue(e, "SpecifiedLineTradeAgreement", "NetPriceProductTradePrice", "ChargeAmount")),
                DecimalFromString(DescValue(a, "SpecifiedLineTradeAgreement", "NetPriceProductTradePrice", "ChargeAmount")),
                $"Net price in row {i + 1} is different.");

            Assert.AreEqual(
                DecimalFromString(DescValue(e, "SpecifiedLineTradeAgreement", "NetPriceProductTradePrice", "BasisQuantity")),
                DecimalFromString(DescValue(a, "SpecifiedLineTradeAgreement", "NetPriceProductTradePrice", "BasisQuantity")),
                $"Net price basis quantity in row {i + 1} is different.");

            Assert.AreEqual(
                DecimalFromString(DescValue(e, "SpecifiedLineTradeDelivery", "BilledQuantity")),
                DecimalFromString(DescValue(a, "SpecifiedLineTradeDelivery", "BilledQuantity")),
                $"Quantity in row {i + 1} is different.");

            Assert.AreEqual(
                DescAttrValue(e, new[] { "SpecifiedLineTradeDelivery", "BilledQuantity" }, "unitCode"),
                DescAttrValue(a, new[] { "SpecifiedLineTradeDelivery", "BilledQuantity" }, "unitCode"),
                $"Unit code in row {i + 1} is different.");

            Assert.AreEqual(
                DescValue(e, "SpecifiedLineTradeDelivery", "DeliveryNoteReferencedDocument", "IssuerAssignedID"),
                DescValue(a, "SpecifiedLineTradeDelivery", "DeliveryNoteReferencedDocument", "IssuerAssignedID"),
                $"Delivery note number in row {i + 1} is different.");

            Assert.AreEqual(
                DescValue(e, "SpecifiedLineTradeDelivery", "DeliveryNoteReferencedDocument", "LineID"),
                DescValue(a, "SpecifiedLineTradeDelivery", "DeliveryNoteReferencedDocument", "LineID"),
                $"Delivery note line ID in row {i + 1} is different.");

            Assert.AreEqual(
                DescValue(e, "SpecifiedLineTradeDelivery", "DeliveryNoteReferencedDocument", "FormattedIssueDateTime", "DateTimeString"),
                DescValue(a, "SpecifiedLineTradeDelivery", "DeliveryNoteReferencedDocument", "FormattedIssueDateTime", "DateTimeString"),
                $"Delivery note date in row {i + 1} is different.");

            Assert.AreEqual(
                DescValue(e, "SpecifiedLineTradeSettlement", "ApplicableTradeTax", "TypeCode"),
                DescValue(a, "SpecifiedLineTradeSettlement", "ApplicableTradeTax", "TypeCode"),
                $"Tax type in row {i + 1} is different.");

            Assert.AreEqual(
                DescValue(e, "SpecifiedLineTradeSettlement", "ApplicableTradeTax", "CategoryCode"),
                DescValue(a, "SpecifiedLineTradeSettlement", "ApplicableTradeTax", "CategoryCode"),
                $"Tax category in row {i + 1} is different.");

            Assert.AreEqual(
                DecimalFromString(DescValue(e, "SpecifiedLineTradeSettlement", "ApplicableTradeTax", "RateApplicablePercent")),
                DecimalFromString(DescValue(a, "SpecifiedLineTradeSettlement", "ApplicableTradeTax", "RateApplicablePercent")),
                $"Tax percent in row {i + 1} is different.");

            Assert.AreEqual(
                DescValue(e, "SpecifiedLineTradeSettlement", "BillingSpecifiedPeriod", "EndDateTime", "DateTimeString"),
                DescValue(a, "SpecifiedLineTradeSettlement", "BillingSpecifiedPeriod", "EndDateTime", "DateTimeString"),
                $"Billing period end date in row {i + 1} is different.");

            Assert.AreEqual(
                DecimalFromString(DescValue(e, "SpecifiedLineTradeSettlement", "SpecifiedTradeSettlementLineMonetarySummation", "LineTotalAmount")),
                DecimalFromString(DescValue(a, "SpecifiedLineTradeSettlement", "SpecifiedTradeSettlementLineMonetarySummation", "LineTotalAmount")),
                $"Line total amount in row {i + 1} is different.");

            Assert.AreEqual(
                DecimalFromString(DescValue(e, "SpecifiedLineTradeSettlement", "SpecifiedTradeSettlementLineMonetarySummation", "TotalAllowanceChargeAmount")),
                DecimalFromString(DescValue(a, "SpecifiedLineTradeSettlement", "SpecifiedTradeSettlementLineMonetarySummation", "TotalAllowanceChargeAmount")),
                $"Line allowance/charge amount in row {i + 1} is different.");

            Assert.AreEqual(
                DescValue(e, "SpecifiedLineTradeSettlement", "AdditionalReferencedDocument", "IssuerAssignedID"),
                DescValue(a, "SpecifiedLineTradeSettlement", "AdditionalReferencedDocument", "IssuerAssignedID"),
                $"Additional referenced document ID in row {i + 1} is different.");

            Assert.AreEqual(
                DescValue(e, "SpecifiedLineTradeSettlement", "AdditionalReferencedDocument", "TypeCode"),
                DescValue(a, "SpecifiedLineTradeSettlement", "AdditionalReferencedDocument", "TypeCode"),
                $"Additional referenced document TypeCode in row {i + 1} is different.");

            Assert.AreEqual(
                DescValue(e, "SpecifiedLineTradeSettlement", "AdditionalReferencedDocument", "ReferenceTypeCode"),
                DescValue(a, "SpecifiedLineTradeSettlement", "AdditionalReferencedDocument", "ReferenceTypeCode"),
                $"Additional referenced document ReferenceTypeCode in row {i + 1} is different.");

            var expLineNotes = DescPath(e, "AssociatedDocumentLineDocument", "IncludedNote").ToList();
            var actLineNotes = DescPath(a, "AssociatedDocumentLineDocument", "IncludedNote").ToList();

            Assert.AreEqual(expLineNotes.Count, actLineNotes.Count, $"Line note count in row {i + 1} is different.");

            for (int n = 0; n < expLineNotes.Count; n++)
            {
                Assert.AreEqual(
                    ChildValue(expLineNotes[n], "Content"),
                    ChildValue(actLineNotes[n], "Content"),
                    $"Line note content in row {i + 1}, note {n + 1} is different.");

                Assert.AreEqual(
                    ChildValue(expLineNotes[n], "SubjectCode"),
                    ChildValue(actLineNotes[n], "SubjectCode"),
                    $"Line note subject code in row {i + 1}, note {n + 1} is different.");
            }
        }
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

    private static string NormalizeXml(XDocument doc)
    {
        var clone = XDocument.Parse(doc.ToString(SaveOptions.DisableFormatting));

        foreach (var el in clone.Descendants())
        {
            if (!el.HasElements)
            {
                el.Value = el.Value.Trim();
            }

            if (el.HasAttributes)
            {
                var attrs = el.Attributes().OrderBy(a => a.Name.ToString()).ToList();
                el.RemoveAttributes();
                foreach (var attr in attrs)
                {
                    el.Add(attr);
                }
            }
        }

        return clone.ToString(SaveOptions.DisableFormatting);
    }

    #endregion

    #region Utilities
    private static void SaveXmlFile(Invoice invoice, string xml)
    {
        string tempDir = Path.GetTempPath();
        string fileName = $"XmlCii_{invoice.InvoiceNumber}_{DateTime.Now:yyyyMMddHHmmssfff}.xml";
        string filePath = Path.Combine(tempDir, fileName);

        File.WriteAllText(filePath, xml);
    }
    #endregion
}
