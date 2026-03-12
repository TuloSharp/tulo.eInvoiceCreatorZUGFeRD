using System.Xml.Serialization;
using tulo.eInvoiceXmlGeneratorCii.Mappers;
using tulo.eInvoiceXmlGeneratorCii.Models;
using tulo.eInvoiceXmlGeneratorCii.Services;
using Zugferd24.Extended;

namespace Tests;

[TestClass]
public class GenerateXmlCiiIntegrationTests
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

    [TestMethod(DisplayName = "Verifies it creates ZUGFeRD invoice same like ZF_Extended__Sammelrechnung_3_Bestellungen.xml")]
    public void Generated_invoice_matches_official_extended_colletive_invoice()
    {
        //Arrange: Invoice object
        var invoice = new Invoice
        {
            InvoiceNumber = "6063636771001",
            InvoiceDate = new DateTime(2026, 08, 27),
            Currency = "EUR",
            DocumentName = "RECHNUNG",
            DocumentTypeCode = "380",
            HeaderChargeTotalAmount = 0.00m,
            HeaderAllowanceTotalAmount = 0.00m,
            HeaderTotalPrepaidAmount = 0m,
            HeaderDuePayableAmount = 655.38m,

            Seller = new Party
            {
                Name = "Lieferant GmbH & Co. KG",
                Street = "Lieferanten-Strasse.12-17",
                Zip = "98765",
                City = "Lieferantenstadt",
                CountryCode = "DE",
                VatId = "DE946280061",
                FiscalId = "88888/00072",
                GeneralEmail = "info@lieferant.com",
                ContactPersonName = "Tim Kleine",
                ContactPhone = "0170...",
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
                AccountName = "Lieferant GmbH & Co. KG",
                Terms = new List<PaymentTermDetails>
                    {
                        new PaymentTermDetails
                        {
                            Description = "Bis zum 06.09.2026 erhalten Sie 2,000 % Skonto",
                            DiscountTerms = new PaymentDiscountTermsDetails
                            {
                                BasisDate = new DateTime(2026, 8, 27),
                                BasisPeriodDays = 10,
                                BasisAmount = 655.38m,
                                CalculationPercent = 2.000m,
                                ActualDiscountAmount = 13.11m
                            }
                        },
                        new PaymentTermDetails
                        {
                            Description = "Bis zum 16.09.2026 ohne Abzug",
                            DueDate = new DateTime(2026, 9, 16)
                        }
                    }
            }
        };

        invoice.Notes.AddRange(new[]
         {
                new InvoiceNote
                {
                    Content =
                        "Bank: LBBW Stuttgart BLZ: 999 888 77 Kto.Nr.: 1234567, IBAN: DE33 9998 8877 0001 2345 67 BIC / Swift-Code: SOLADEST600",
                    SubjectCode = "REG"
                },
                new InvoiceNote
                {
                    Content =
                        "Lieferant GmbH & Co. KG - 98765 Lieferantenstadt - T +49 (0)7891 11-0 - F +49(0)7891 11-1000 - info@lieferant.com - www.lieferant.com Hausanschrift: Lieferanten-Strasse 12-17 - 98765 Lieferantenstadt - Sitz Lieferantenstadt, Amtsgericht Stuttgart HRA 123456 Geschäftsführer: ",
                    SubjectCode = "REG"
                },
                new InvoiceNote
                {
                    Content = "Wenn nicht anders angegeben entspricht das Leistungsdatum dem Rechnungsdatum.",
                    SubjectCode = "AAI"
                },
                new InvoiceNote
                {
                    Content =
                        "Haben Sie Fragen zur Rechnung? Gerne hilft Ihnen Ihr zuständiger Lieferant Verkäufer weiter. Die Lieferung erfolgte zu unseren bekannten Verkaufs- und Lieferbedingungen. Bitte beachten Sie hierzu unsere allgemeinen Geschäftsbedingungen unter www.lieferant.de/agb.",
                    SubjectCode = "AAI"
                },
                new InvoiceNote
                {
                    Content =
                        "Zahlungsavis/Aufstellung/Auflistung zur Zahlung bitte an: E-mail: zahlungseingang@lieferant.com oder Fax +49 7891 11-59333",
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

        // Item lines: 
        invoice.Lines.Add(new InvoiceLine
        {
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
            AdditionalReferencedDocumentReferenceTypeCode = "VN"
        });

        invoice.Lines.Add(new InvoiceLine
        {
            Description = "MUELLSACK-EXTRASTARK-BLAU-700X1100X0,07",
            Quantity = 150m,
            UnitCode = "H87",
            UnitPrice = 0.4929m,
            TaxPercent = 19m,
            TaxCategory = "S"
        });

        invoice.Lines.Add(new InvoiceLine
        {
            Description = "SHR-AW30-(A2K)-7,5X152",
            Quantity = 400m,
            UnitCode = "H87",
            UnitPrice = 0.3276m,
            TaxPercent = 19m,
            TaxCategory = "S"
        });

        invoice.Lines.Add(new InvoiceLine
        {
            Description = "MU-6KT-DIN934-I8I-SW17-(A2K)-M10",
            Quantity = 500m,
            UnitCode = "H87",
            UnitPrice = 0.0916m,
            TaxPercent = 19m,
            TaxCategory = "S"
        });

        invoice.Lines.Add(new InvoiceLine
        {
            Description = "MU-6KT-DIN934-I8I-SW19-(A2K)-M12",
            Quantity = 500m,
            UnitCode = "H87",
            UnitPrice = 0.1332m,
            TaxPercent = 19m,
            TaxCategory = "S"
        });

        invoice.Lines.Add(new InvoiceLine
        {
            Description = "MUELLSACK-EXTRASTARK-BLAU-700X1100X0,07",
            Quantity = 300m,
            UnitCode = "H87",
            UnitPrice = 0.4929m,
            TaxPercent = 19m,
            TaxCategory = "S"
        });

        invoice.Lines.Add(new InvoiceLine
        {
            Description = "KAFFEE-ESPRESSO-GANZE-BOHNEN-1KG",
            Quantity = 2m,
            UnitCode = "H87",
            UnitPrice = 18.90m,
            TaxPercent = 7m,
            TaxCategory = "S"
        });

        // Act
        var cii = _mapper.Map(invoice);
        string xml = _exporter.ToXml(cii);

        // optional: XSD-Validierung
        CiiSchemaValidator.ValidateCiiZugferd24Extended(xml);
        // optional use SAXONCS but ist not free, alternative SaxonHE11NetXslt
        //CiiSchematronValidator.ValidateCiiExtendedSchematron(xml);

        SaveXmlFile(invoice, xml);

        //compare with original file      
        string baseDir = AppContext.BaseDirectory;
        string samplePath = Path.Combine(baseDir, "Examples", "ZF_Extended__Sammelrechnung_3_Bestellungen.xml");

        Assert.IsTrue(File.Exists(samplePath), $"Example-XML is not fount: {samplePath}");

        string sampleXml = File.ReadAllText(samplePath);

        var serializer = new XmlSerializer(typeof(CrossIndustryInvoiceType));

        CrossIndustryInvoiceType expected;
        CrossIndustryInvoiceType actual;

        using (var sr = new StringReader(sampleXml))
        {
            expected = (CrossIndustryInvoiceType)serializer.Deserialize(sr)!;
        }

        using (var sr = new StringReader(xml))
        {
            actual = (CrossIndustryInvoiceType)serializer.Deserialize(sr)!;
        }

        //Assert: 
        Assert.AreEqual(expected.ExchangedDocumentContext.GuidelineSpecifiedDocumentContextParameter.ID.Value,
            actual.ExchangedDocumentContext.GuidelineSpecifiedDocumentContextParameter.ID.Value, "Guideline ID is different (Profil).");

        Assert.AreEqual(expected.ExchangedDocument.ID.Value, actual.ExchangedDocument.ID.Value, "invocie number is different.");

        var expIssue = expected.ExchangedDocument.IssueDateTime.Item;
        var actIssue = actual.ExchangedDocument.IssueDateTime.Item;
        Assert.AreEqual(expIssue.Value, actIssue.Value, "Invoice date is different.");

        // Seller / Buyer Name
        Assert.AreEqual(expected.SupplyChainTradeTransaction.ApplicableHeaderTradeAgreement.SellerTradeParty.Name.Value, actual.SupplyChainTradeTransaction.ApplicableHeaderTradeAgreement.SellerTradeParty.Name.Value, "Seller-Name is different.");

        Assert.AreEqual(expected.SupplyChainTradeTransaction.ApplicableHeaderTradeAgreement.BuyerTradeParty.Name.Value,
            actual.SupplyChainTradeTransaction.ApplicableHeaderTradeAgreement.BuyerTradeParty.Name.Value, "Buyer-Name is diferent ");

        // Header-Sum
        var expSum = expected.SupplyChainTradeTransaction.ApplicableHeaderTradeSettlement.SpecifiedTradeSettlementHeaderMonetarySummation;
        var actSum = actual.SupplyChainTradeTransaction.ApplicableHeaderTradeSettlement.SpecifiedTradeSettlementHeaderMonetarySummation;

        Assert.AreEqual(expSum.LineTotalAmount.Value, actSum.LineTotalAmount.Value, "LineTotalAmount (Header) ist diferent.");
        Assert.AreEqual(expSum.TaxBasisTotalAmount.Value, actSum.TaxBasisTotalAmount.Value, "TaxBasisTotalAmount (Header) is diferent.");
        Assert.AreEqual(expSum.GrandTotalAmount.Value, actSum.GrandTotalAmount.Value, "GrandTotalAmount (Header) is diferent.");
        Assert.AreEqual(expSum.DuePayableAmount.Value, actSum.DuePayableAmount.Value, "DuePayableAmount (Header) is diferent.");

        decimal expTaxTotal = expSum.TaxTotalAmount?.Sum(a => a.Value) ?? 0m;
        decimal actTaxTotal = actSum.TaxTotalAmount?.Sum(a => a.Value) ?? 0m;
        Assert.AreEqual(expTaxTotal, actTaxTotal, "TaxTotalAmount (Header) is different.");

        // Header tax breakdown (19% + 7%)
        var expTaxes = expected.SupplyChainTradeTransaction.ApplicableHeaderTradeSettlement.ApplicableTradeTax;
        var actTaxes = actual.SupplyChainTradeTransaction.ApplicableHeaderTradeSettlement.ApplicableTradeTax;

        Assert.HasCount(expTaxes.Length, actTaxes, "Quanty Header control groups is different.");

        foreach (var expTax in expTaxes)
        {
            var actTax = actTaxes.FirstOrDefault(t =>
                t.RateApplicablePercent.Value == expTax.RateApplicablePercent.Value);

            Assert.IsNotNull(actTax, $"No steering group with {expTax.RateApplicablePercent.Value}% in generated document.");

            Assert.AreEqual(expTax.BasisAmount.Value, actTax.BasisAmount.Value, $"BasisAmount at {expTax.RateApplicablePercent.Value}% is different.");
            Assert.AreEqual(expTax.CalculatedAmount.Value, actTax.CalculatedAmount.Value, $"CalculatedAmount at {expTax.RateApplicablePercent.Value}% is different.");
        }

        // Number of lines
        var expLines = expected.SupplyChainTradeTransaction.IncludedSupplyChainTradeLineItem;
        var actLines = actual.SupplyChainTradeTransaction.IncludedSupplyChainTradeLineItem;

        Assert.HasCount(expLines.Length, actLines, "Position lines quantity is different.");

        // compare all lines Name + LineTotalAmount
        for (int i = 0; i < expLines.Length; i++)
        {
            var e = expLines[i];
            var a = actLines[i];

            Assert.AreEqual(
                e.SpecifiedTradeProduct.Name.Value,
                a.SpecifiedTradeProduct.Name.Value,
                $"Product name in row {i + 1} are different.");

            var eSum = e.SpecifiedLineTradeSettlement
                        .SpecifiedTradeSettlementLineMonetarySummation
                        .LineTotalAmount.Value;
            var aSum = a.SpecifiedLineTradeSettlement
                        .SpecifiedTradeSettlementLineMonetarySummation
                        .LineTotalAmount.Value;

            Assert.AreEqual(eSum, aSum,
                $"LineTotalAmount in row {i + 1} is different.");
        }
    }

    [TestMethod(DisplayName = "Verifies it creates ZUGFeRD invoice same like EXTENDED_Fremdwaehrung.xml")]
    public void Generated_invoice_matches_extended_foreign_currency_example()
    {
        //Arrange: 
        var invoice = new Invoice
        {
            InvoiceNumber = "47110815",
            InvoiceDate = new DateTime(2025, 12, 01),
            Currency = "GBP",
            DocumentName = "RECHNUNG",
            DocumentTypeCode = "380",

            Seller = new Party
            {
                ID = "12345676",
                Name = "Rohstoff AG Salzgitter",
                Street = "Marktstr. 153",
                Zip = "38226",
                City = "Salzgitter",
                CountryCode = "DE",
                VatId = "DE123456789"
            },
            Buyer = new Party
            {
                ID = "75969813",
                Name = "Metallbau Leipzig GmbH & Co. KG",
                Street = "Pappelallee 15",
                Zip = "12345",
                City = "Leipzig",
                CountryCode = "DE",
                LeitwegId = "04011000-1234512345-35"
            },
            Payment = new PaymentDetails
            {
                PaymentReference = "47110815",
                PaymentMeansTypeCode = "58",
                PaymentMeansInformation = "Bezahlung per SEPA Überweisung",

                Iban = "DE77 3707 0060 0321 9870 00",

                AccountName = "Global Supplies Financial Services",
                PaymentTermsText = "Zahlbar mit 2% Skonto bis",
                DueDate = new DateTime(2025, 12, 31)
            }
        };

        // Header
        invoice.Notes.AddRange(new[]
        {
        new InvoiceNote
        {
            Content =
                "Mitglieder der Geschäftsleitung\n" +
                "\t\t\tH. Meier Geschäftsführer\n" +
                "\t\t\tT. Müller Prokurist\n" +
                "\t\t\tHRB Braunschweig 12345",
            SubjectCode = "REG"
        },
        new InvoiceNote
        {
            Content = "Vom 17. Dezember 2024 bis 6. Januar 2025 haben wir Betriebsferien.",
            SubjectCode = "AAI"
        },
        new InvoiceNote
        {
            Content = "Aus konzern-internen Gründen wird der Steuerbetrag sowohl in der Rechungswährung (EUR) als auch in der Buchwährung (GBP) ausgegeben.",
            SubjectCode = "TXD"
        },
        new InvoiceNote
        {
            Content = "ZUGFeRD vers 2.4.0 (Extended)",
            SubjectCode = "ACB"
        },
        new InvoiceNote
        {
            Content = "Dies ist ein Beispiel zur empfohlenen Darstellung von Fremdwährungsrechnungen",
            SubjectCode = "ACB"
        }
    });

        // Position from  EXTENDED_Fremdwaehrung.xml
        invoice.Lines.Add(new InvoiceLine
        {
            Description = "Stahlcoil",
            ProductDescription = "Materialzertifikat X-234 gem ISO XYZ. Ware bleibt bis zur vollständigen Bezahlung unser Eigentum.",
            SellerAssignedId = "CO-123/V2A",

            BuyerOrderReferencedId = "ORDER84359",

            Quantity = 10m,
            UnitCode = "H87",
            UnitPrice = 100m,
            TaxPercent = 19m,
            TaxCategory = "S",

            BillingPeriodEndDate = new DateTime(2025, 11, 30),
            OriginCountryCode = "DE"
        });

        // Act
        var cii = _mapper.Map(invoice);
        string xml = _exporter.ToXml(cii);

        // XSD-Validierung (Extended)
        CiiSchemaValidator.ValidateCiiZugferd24Extended(xml);
        // optional use SAXON but ist not free
        //CiiSchematronValidator.ValidateCiiExtendedSchematron(xml);

        SaveXmlFile(invoice, xml);

        // ========= load expected XML =========
        string baseDir = AppContext.BaseDirectory;
        string samplePath = Path.Combine(baseDir, "Examples", "EXTENDED_Fremdwaehrung.xml");

        Assert.IsTrue(File.Exists(samplePath), $"Example-XML is not found: {samplePath}");

        string sampleXml = File.ReadAllText(samplePath);

        var serializer = new XmlSerializer(typeof(CrossIndustryInvoiceType));

        CrossIndustryInvoiceType expected;
        CrossIndustryInvoiceType actual;

        using (var sr = new StringReader(sampleXml))
        {
            expected = (CrossIndustryInvoiceType)serializer.Deserialize(sr)!;
        }

        using (var sr = new StringReader(xml))
        {
            actual = (CrossIndustryInvoiceType)serializer.Deserialize(sr)!;
        }

        //Assert:

        //Guideline / Profil
        Assert.AreEqual(expected.ExchangedDocumentContext.GuidelineSpecifiedDocumentContextParameter.ID.Value, actual.ExchangedDocumentContext.GuidelineSpecifiedDocumentContextParameter.ID.Value, "Guideline ID is different (Profil).");

        //Invoice document (Number, Name, Typ, Date)
        Assert.AreEqual(expected.ExchangedDocument.ID.Value, actual.ExchangedDocument.ID.Value, "Invoice number is different.");
        Assert.AreEqual(expected.ExchangedDocument.Name.Value, actual.ExchangedDocument.Name.Value, "Document-Name is different.");

        Assert.AreEqual(expected.ExchangedDocument.TypeCode.Value, actual.ExchangedDocument.TypeCode.Value, "TypeCode is different.");

        var expIssue = expected.ExchangedDocument.IssueDateTime.Item;
        var actIssue = actual.ExchangedDocument.IssueDateTime.Item;
        Assert.AreEqual(expIssue.Value, actIssue.Value, "Invoice date is different.");

        //Seller / Buyer (Name, IDs, Address)
        var expAgreement = expected.SupplyChainTradeTransaction.ApplicableHeaderTradeAgreement;
        var actAgreement = actual.SupplyChainTradeTransaction.ApplicableHeaderTradeAgreement;

        // Seller
        Assert.AreEqual(expAgreement.SellerTradeParty.Name.Value, actAgreement.SellerTradeParty.Name.Value, "Seller-Name is different.");
        Assert.AreEqual(expAgreement.SellerTradeParty.PostalTradeAddress.PostcodeCode.Value, actAgreement.SellerTradeParty.PostalTradeAddress.PostcodeCode.Value, "Seller-Postal Code is different.");
        Assert.AreEqual(expAgreement.SellerTradeParty.PostalTradeAddress.CityName.Value, actAgreement.SellerTradeParty.PostalTradeAddress.CityName.Value, "Seller-City is different.");

        // Buyer
        Assert.AreEqual(expAgreement.BuyerTradeParty.Name.Value, actAgreement.BuyerTradeParty.Name.Value, "Buyer-Name is different.");

        // InvoiceCurrencyCode
        var expSettlement = expected.SupplyChainTradeTransaction.ApplicableHeaderTradeSettlement;
        var actSettlement = actual.SupplyChainTradeTransaction.ApplicableHeaderTradeSettlement;
        Assert.AreEqual(expSettlement.InvoiceCurrencyCode.Value, actSettlement.InvoiceCurrencyCode.Value, "InvoiceCurrencyCode is different.");

        // Payment: IBAN + Acount Data
        var expPaymentMeans = expSettlement.SpecifiedTradeSettlementPaymentMeans.FirstOrDefault();
        var actPaymentMeans = actSettlement.SpecifiedTradeSettlementPaymentMeans.FirstOrDefault();
        Assert.IsNotNull(actPaymentMeans, "The generated document is missing a PaymentMeans.");
        Assert.IsNotNull(expPaymentMeans, "The example document is missing a PaymentMeans (unerwartet).");
        Assert.AreEqual(expPaymentMeans.PayeePartyCreditorFinancialAccount.IBANID.Value, actPaymentMeans.PayeePartyCreditorFinancialAccount.IBANID.Value, "IBAN is different.");
        Assert.AreEqual(expPaymentMeans.PayeePartyCreditorFinancialAccount.AccountName.Value, actPaymentMeans.PayeePartyCreditorFinancialAccount.AccountName.Value, "Account holder  is different.");

        // Positionszeilen – im Beispiel gibt es genau eine Position
        var expLines = expected.SupplyChainTradeTransaction.IncludedSupplyChainTradeLineItem;
        var actLines = actual.SupplyChainTradeTransaction.IncludedSupplyChainTradeLineItem;
        Assert.HasCount(expLines.Length, actLines, "Number of item lines varies.");
        Assert.HasCount(1, actLines, "Fremdwährungs-Beispiel should have exactly one position.");
        var e = expLines[0];
        var a = actLines[0];

        // Product name
        Assert.AreEqual(e.SpecifiedTradeProduct.Name.Value, a.SpecifiedTradeProduct.Name.Value, "Product name of the position is different.");
        // SellerAssignedID (CO-123/V2A)
        Assert.AreEqual(e.SpecifiedTradeProduct.SellerAssignedID.Value, a.SpecifiedTradeProduct.SellerAssignedID.Value, "SellerAssignedID the Position is different.");
        // BilledQuantity (10 H87)
        Assert.AreEqual(e.SpecifiedLineTradeDelivery.BilledQuantity.Value, a.SpecifiedLineTradeDelivery.BilledQuantity.Value, "BilledQuantity is different.");
        Assert.AreEqual(e.SpecifiedLineTradeDelivery.BilledQuantity.unitCode, a.SpecifiedLineTradeDelivery.BilledQuantity.unitCode, "BilledQuantity.UnitCode is different.");
        // Net unit price (100)
        Assert.AreEqual(e.SpecifiedLineTradeAgreement.NetPriceProductTradePrice.ChargeAmount.Value, a.SpecifiedLineTradeAgreement.NetPriceProductTradePrice.ChargeAmount.Value, "Net unit price (ChargeAmount) is different.");
    }
    
    [TestMethod(DisplayName = "Creates an EN16931 credit note (Gutschrift) and compares key fields with EN16931_Gutschrift.xml")]
    public void Generated_credit_note_matches_en16931_sample()
    {
        // ========= arrange =========
        var creditNote = new Invoice
        {
            InvoiceNumber = "471102",
            InvoiceDate = new DateTime(2018, 03, 05),
            Currency = "EUR",
            DocumentName = "Gutschrift",
            DocumentTypeCode = "389", // EN16931 sample uses 389 (credit note)

            Seller = new Party
            {
                ID = "549910",
                Name = "Lieferant GmbH",
                Street = "Lieferantenstraße 20",
                Zip = "80333",
                City = "München",
                CountryCode = "DE",
                VatId = "DE123456789",
                FiscalId = "201/113/40209"
            },
            Buyer = new Party
            {
                ID = "GE2020211",
                Name = "Kunden AG Mitte",
                Street = "Kundenstraße 15",
                Zip = "69876",
                City = "Frankfurt",
                CountryCode = "DE",
                VatId = "DE136695976"
            },

            // no payment means in the EN16931 sample -> keep bank data empty
            Payment = new PaymentDetails
            {
                PaymentTermsText = "Der Betrag wird ihrem Kundenkonto gutgeschrieben und mit der nächsten Rechnung verrechnet."
            },

            // optional: header totals (the mapper may also calculate them)
            HeaderChargeTotalAmount = 0.00m,
            HeaderAllowanceTotalAmount = 0.00m,
            HeaderTotalPrepaidAmount = 0.00m,
            HeaderDuePayableAmount = 529.87m
        };

        creditNote.Lines.Add(new InvoiceLine
        {
            Description = "Trennblätter A4",
            Quantity = 20.0m,
            UnitCode = "H87",
            UnitPrice = 9.90m,
            TaxPercent = 19.00m,
            TaxCategory = "S"
        });

        creditNote.Lines.Add(new InvoiceLine
        {
            Description = "Joghurt Banane",
            Quantity = 50.0m,
            UnitCode = "H87",
            UnitPrice = 5.50m,
            TaxPercent = 7.00m,
            TaxCategory = "S"
        });

        // ========= act =========
        var cii = _mapper.Map(creditNote);
        string xml = _exporter.ToXml(cii);

        // ========= assert: compare with sample =========
        string baseDir = AppContext.BaseDirectory;
        string samplePath = Path.Combine(baseDir, "Examples", "EN16931_Gutschrift.xml");
        Assert.IsTrue(File.Exists(samplePath), $"Sample XML not found: {samplePath}");

        string sampleXml = File.ReadAllText(samplePath);

        var serializer = new XmlSerializer(typeof(CrossIndustryInvoiceType));

        CrossIndustryInvoiceType expected;
        CrossIndustryInvoiceType actual;

        using (var sr = new StringReader(sampleXml))
            expected = (CrossIndustryInvoiceType)serializer.Deserialize(sr)!;

        using (var sr = new StringReader(xml))
            actual = (CrossIndustryInvoiceType)serializer.Deserialize(sr)!;

        // Document header
        Assert.AreEqual(expected.ExchangedDocument.ID.Value, actual.ExchangedDocument.ID.Value, "ID (Rechnungsnummer) is different.");
        Assert.AreEqual(expected.ExchangedDocument.TypeCode.Value, actual.ExchangedDocument.TypeCode.Value, "TypeCode is different.");

        var expIssue = expected.ExchangedDocument.IssueDateTime.ToString();
        var actIssue = actual.ExchangedDocument.IssueDateTime.ToString();
        Assert.AreEqual(expIssue, actIssue, "IssueDate is different.");

        // Parties
        Assert.AreEqual(expected.SupplyChainTradeTransaction.ApplicableHeaderTradeAgreement.SellerTradeParty.Name.Value,
                        actual.SupplyChainTradeTransaction.ApplicableHeaderTradeAgreement.SellerTradeParty.Name.Value,
                        "Seller Name is different");
        Assert.AreEqual(expected.SupplyChainTradeTransaction.ApplicableHeaderTradeAgreement.BuyerTradeParty.Name.Value,
                        actual.SupplyChainTradeTransaction.ApplicableHeaderTradeAgreement.BuyerTradeParty.Name.Value,
                        "Buyer Name is different.");

        // Lines count + core fields
        var expLines = expected.SupplyChainTradeTransaction.IncludedSupplyChainTradeLineItem;
        var actLines = actual.SupplyChainTradeTransaction.IncludedSupplyChainTradeLineItem;
        Assert.HasCount(expLines.Length, actLines, "Number of item lines varies.");

        for (int i = 0; i < expLines.Length; i++)
        {
            Assert.AreEqual(expLines[i].SpecifiedTradeProduct.Name.Value, actLines[i].SpecifiedTradeProduct.Name.Value, $"Line {i + 1} Product name is different.");
            Assert.AreEqual(expLines[i].SpecifiedLineTradeDelivery.BilledQuantity.Value, actLines[i].SpecifiedLineTradeDelivery.BilledQuantity.Value, $"Line {i + 1} Quantity is different.");
            Assert.AreEqual(expLines[i].SpecifiedLineTradeAgreement.NetPriceProductTradePrice.ChargeAmount.Value,
                            actLines[i].SpecifiedLineTradeAgreement.NetPriceProductTradePrice.ChargeAmount.Value,
                            $"Line {i + 1} Price is different.");
            Assert.AreEqual(expLines[i].SpecifiedLineTradeSettlement.ApplicableTradeTax[0].RateApplicablePercent.Value,
                            actLines[i].SpecifiedLineTradeSettlement.ApplicableTradeTax[0].RateApplicablePercent.Value,
                            $"Line {i + 1} Tax% is different.");
        }

        // Header monetary summation (DuePayable)
        Assert.AreEqual(
            expected.SupplyChainTradeTransaction.ApplicableHeaderTradeSettlement.SpecifiedTradeSettlementHeaderMonetarySummation.DuePayableAmount.Value,
            actual.SupplyChainTradeTransaction.ApplicableHeaderTradeSettlement.SpecifiedTradeSettlementHeaderMonetarySummation.DuePayableAmount.Value,
            "DuePayableAmount is different.");

        // Ensure payment means are not emitted
        Assert.DoesNotContain("SpecifiedTradeSettlementPaymentMeans", xml, "PaymentMeans must not be included in the credit note.");
    }

    [TestMethod(DisplayName = "Creates an EN16931 corrected invoice (Rechnungskorrektur) and compares key fields with EN16931_Rechnungskorrektur.xml")]
    public void Generated_correction_invoice_matches_en16931_sample()
    {
        // ========= arrange =========
        var correction = new Invoice
        {
            InvoiceNumber = "RK21012345",
            InvoiceDate = new DateTime(2018, 09, 16),
            Currency = "EUR",
            DocumentName = "Rechnungskorrektur",
            DocumentTypeCode = "384", // corrected invoice

            Seller = new Party
            {
                ID = "549910",
                Name = "MUSTERLIEFERANT GMBH",
                Street = "BAHNHOFSTRASSE 99",
                Zip = "99199",
                City = "MUSTERHAUSEN",
                CountryCode = "DE",
                VatId = "DE123456789"
            },
            Buyer = new Party
            {
                ID = "009420",
                Name = "MUSTER-KUNDE GMBH",
                Street = "KUNDENWEG 88",
                Zip = "40235",
                City = "DUESSELDORF",
                CountryCode = "DE"
            },

            // EN16931 sample has no payment means/terms -> keep empty
            Payment = new PaymentDetails(),

            // Important: header allowance is negative in sample
            HeaderChargeTotalAmount = 0.00m,
            HeaderAllowanceTotalAmount = -0.23m,
            HeaderTotalPrepaidAmount = 0.00m,
            HeaderDuePayableAmount = -8.79m
        };

        // sample positions are negative quantities
        correction.Lines.Add(new InvoiceLine
        {
            Description = "Zitronensäure 100ml",
            Quantity = -5.0m,
            UnitCode = "H87",
            UnitPrice = 1.00m,
            TaxPercent = 19.00m,
            TaxCategory = "S"
        });

        correction.Lines.Add(new InvoiceLine
        {
            Description = "Gelierzucker Extra 250g",
            Quantity = -2.0m,
            UnitCode = "H87",
            UnitPrice = 1.45m,
            TaxPercent = 7.00m,
            TaxCategory = "S"
        });

        // optional notes (present in sample)
        correction.Notes.Add(new InvoiceNote
        {
            Content = "Es bestehen Rabatt- oder Bonusvereinbarungen.",
            SubjectCode = "AAI"
        });

        // ========= act =========
        var cii = _mapper.Map(correction);
        string xml = _exporter.ToXml(cii);

        // ========= assert: compare with sample =========
        string baseDir = AppContext.BaseDirectory;
        string samplePath = Path.Combine(baseDir, "Examples", "EN16931_Rechnungskorrektur.xml");
        Assert.IsTrue(File.Exists(samplePath), $"Sample XML not found: {samplePath}");

        string sampleXml = File.ReadAllText(samplePath);

        var serializer = new XmlSerializer(typeof(CrossIndustryInvoiceType));

        CrossIndustryInvoiceType expected;
        CrossIndustryInvoiceType actual;

        using (var sr = new StringReader(sampleXml))
            expected = (CrossIndustryInvoiceType)serializer.Deserialize(sr)!;

        using (var sr = new StringReader(xml))
            actual = (CrossIndustryInvoiceType)serializer.Deserialize(sr)!;

        // Document header
        Assert.AreEqual(expected.ExchangedDocument.ID.Value, actual.ExchangedDocument.ID.Value, "ID (Korrektur-Rechnungsnummer) is different.");
        Assert.AreEqual(expected.ExchangedDocument.TypeCode.Value, actual.ExchangedDocument.TypeCode.Value, "TypeCode is different.");

        var expIssue = expected.ExchangedDocument.IssueDateTime.ToString();
        var actIssue = actual.ExchangedDocument.IssueDateTime.ToString();
        Assert.AreEqual(expIssue, actIssue, "IssueDate is different");

        // Parties
        Assert.AreEqual(expected.SupplyChainTradeTransaction.ApplicableHeaderTradeAgreement.SellerTradeParty.Name.Value,
                        actual.SupplyChainTradeTransaction.ApplicableHeaderTradeAgreement.SellerTradeParty.Name.Value,
                        "Seller Name is different.");
        Assert.AreEqual(expected.SupplyChainTradeTransaction.ApplicableHeaderTradeAgreement.BuyerTradeParty.Name.Value,
                        actual.SupplyChainTradeTransaction.ApplicableHeaderTradeAgreement.BuyerTradeParty.Name.Value,
                        "Buyer Name is different.");

        // Header monetary summation (Allowance/DuePayable)
        var expSum = expected.SupplyChainTradeTransaction.ApplicableHeaderTradeSettlement.SpecifiedTradeSettlementHeaderMonetarySummation;
        var actSum = actual.SupplyChainTradeTransaction.ApplicableHeaderTradeSettlement.SpecifiedTradeSettlementHeaderMonetarySummation;

        Assert.AreEqual(expSum.AllowanceTotalAmount.Value, actSum.AllowanceTotalAmount.Value, "AllowanceTotalAmount is different.");
        Assert.AreEqual(expSum.DuePayableAmount.Value, actSum.DuePayableAmount.Value, "DuePayableAmount unteis differentrschiedlich.");

        // Lines count + core fields
        var expLines = expected.SupplyChainTradeTransaction.IncludedSupplyChainTradeLineItem;
        var actLines = actual.SupplyChainTradeTransaction.IncludedSupplyChainTradeLineItem;
        Assert.HasCount(expLines.Length, actLines, "Number of position lines is different.");

        for (int i = 0; i < expLines.Length; i++)
        {
            Assert.AreEqual(expLines[i].SpecifiedTradeProduct.Name.Value, actLines[i].SpecifiedTradeProduct.Name.Value, $"Line {i + 1} Product name is different.");
            Assert.AreEqual(expLines[i].SpecifiedLineTradeDelivery.BilledQuantity.Value, actLines[i].SpecifiedLineTradeDelivery.BilledQuantity.Value, $"Line {i + 1} Quantity varies");
            Assert.AreEqual(expLines[i].SpecifiedLineTradeAgreement.NetPriceProductTradePrice.ChargeAmount.Value,
                            actLines[i].SpecifiedLineTradeAgreement.NetPriceProductTradePrice.ChargeAmount.Value,
                            $"Line {i + 1} Price is different.");
            Assert.AreEqual(expLines[i].SpecifiedLineTradeSettlement.ApplicableTradeTax[0].RateApplicablePercent.Value,
                            actLines[i].SpecifiedLineTradeSettlement.ApplicableTradeTax[0].RateApplicablePercent.Value,
                            $"Line {i + 1} Tax% is different.");
        }

        // Ensure no payment means emitted
        Assert.DoesNotContain("SpecifiedTradeSettlementPaymentMeans", xml, "PaymentMeans must not be included in the correction (sample contains none).");
    }

    [TestMethod(DisplayName = "Verifies it creates ZUGFeRD 2.4 Extended invoice with SubInvoiceLines like Extended___SubInvoiceLines_Buero_Material_Bsp3__.xml")]
    public void Generated_invoice_matches_official_extended_sub_invoice_lines_example()
    {
        // Arrange: invoice object matching the official ZUGFeRD 2.4 Extended SubInvoiceLines example
        var invoice = new Invoice
        {
            InvoiceNumber = "99877",
            InvoiceDate = new DateTime(2026, 05, 30),
            Currency = "EUR",
            DocumentName = "RECHNUNG",
            DocumentTypeCode = "380",
            BuyerReference = "Kundenref. BT-10",
            SellerOrderReferencedId = "G12042-1-01",
            BuyerOrderReferencedId = "BT-13",
            ContractReferencedId = "Vertragsnr. BT-12",
            AdditionalReferencedDocumentId = "Vergabenr. BT-17",
            AdditionalReferencedDocumentTypeCode = "50",
            ProcuringProjectId = "Projektnr. BT-11",
            ProcuringProjectName = "Project reference",
            ReceivableAccountingAccountId = "Kostenstelle BT-19",
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
                LegalOrganizationId = "HRA 45678",
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
                Terms =
                {
                    new PaymentTermDetails
                    {
                        Description = "Bei Zahlung bis zum 06.06.2026 zahlen Sie mit 2,00 % Skonto € 1638,51 €",
                        DueDate = new DateTime(2026, 06, 06),
                        DiscountTerms = new PaymentDiscountTermsDetails
                        {
                            BasisDate = new DateTime(2026, 05, 30),
                            BasisPeriodDays = 7,
                            BasisAmount = 1671.95m,
                            CalculationPercent = 2.00m,
                            ActualDiscountAmount = 33.44m
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
            Quantity = 2.00m,
            UnitCode = "H87",
            UnitPrice = 300.00m,
            TaxPercent = 19.00m,
            TaxCategory = "S"
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
            Quantity = 3.00m,
            UnitCode = "H87",
            UnitPrice = 150.00m,
            TaxPercent = 19.00m,
            TaxCategory = "S"
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
            Quantity = -1.00m,
            UnitCode = "H87",
            UnitPrice = 50.00m,
            TaxPercent = 19.00m,
            TaxCategory = "S"
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
            Quantity = 1.00m,
            UnitCode = "H87",
            UnitPrice = 1000.00m,
            ForcedLineTotalAmount = 1000.00m,
            TaxPercent = 19.00m,
            TaxCategory = "S"
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
            Quantity = 3.00m,
            UnitCode = "H87",
            UnitPrice = 120.00m,
            TaxPercent = 19.00m,
            TaxCategory = "S"
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
            Quantity = 10.00m,
            UnitCode = "H87",
            UnitPrice = 9.00m,
            TaxPercent = 19.00m,
            TaxCategory = "S"
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
            Quantity = -1.00m,
            UnitCode = "H87",
            UnitPrice = 45.00m,
            TaxPercent = 19.00m,
            TaxCategory = "S"
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
            Quantity = 1.00m,
            UnitCode = "H87",
            UnitPrice = 405.00m,
            ForcedLineTotalAmount = 405.00m,
            TaxPercent = 19.00m,
            TaxCategory = "S"
        });

        // Act
        var cii = _mapper.Map(invoice);
        var xml = _exporter.ToXml(cii);

        CiiSchemaValidator.ValidateCiiZugferd24Extended(xml);
        SaveXmlFile(invoice, xml);

        var baseDir = AppContext.BaseDirectory;
        var samplePath = Path.Combine(baseDir, "Examples", "Extended___SubInvoiceLines_Buero_Material_Bsp3__.xml");

        Assert.IsTrue(File.Exists(samplePath), $"Example XML was not found: {samplePath}");

        var sampleXml = File.ReadAllText(samplePath);
        var serializer = new XmlSerializer(typeof(CrossIndustryInvoiceType));

        CrossIndustryInvoiceType expected;
        CrossIndustryInvoiceType actual;

        using (var sr = new StringReader(sampleXml))
        {
            expected = (CrossIndustryInvoiceType)serializer.Deserialize(sr)!;
        }

        using (var sr = new StringReader(xml))
        {
            actual = (CrossIndustryInvoiceType)serializer.Deserialize(sr)!;
        }

        // Assert: document header
        Assert.AreEqual(
            expected.ExchangedDocumentContext.GuidelineSpecifiedDocumentContextParameter.ID.Value,
            actual.ExchangedDocumentContext.GuidelineSpecifiedDocumentContextParameter.ID.Value,
            "Guideline ID is different.");

        Assert.AreEqual(expected.ExchangedDocument.ID.Value, actual.ExchangedDocument.ID.Value, "Invoice number is different.");
        Assert.AreEqual(expected.ExchangedDocument.TypeCode.Value, actual.ExchangedDocument.TypeCode.Value, "Document type code is different.");

        var expIssue = expected.ExchangedDocument.IssueDateTime.Item;
        var actIssue = actual.ExchangedDocument.IssueDateTime.Item;
        Assert.AreEqual(expIssue.Value, actIssue.Value, "Invoice date is different.");

        // Assert: header parties
        Assert.AreEqual(
            expected.SupplyChainTradeTransaction.ApplicableHeaderTradeAgreement.SellerTradeParty.Name.Value,
            actual.SupplyChainTradeTransaction.ApplicableHeaderTradeAgreement.SellerTradeParty.Name.Value,
            "Seller name is different.");

        Assert.AreEqual(
            expected.SupplyChainTradeTransaction.ApplicableHeaderTradeAgreement.BuyerTradeParty.Name.Value,
            actual.SupplyChainTradeTransaction.ApplicableHeaderTradeAgreement.BuyerTradeParty.Name.Value,
            "Buyer name is different.");

        // Assert: header monetary summation
        var expSum = expected.SupplyChainTradeTransaction.ApplicableHeaderTradeSettlement.SpecifiedTradeSettlementHeaderMonetarySummation;
        var actSum = actual.SupplyChainTradeTransaction.ApplicableHeaderTradeSettlement.SpecifiedTradeSettlementHeaderMonetarySummation;

        Assert.AreEqual(expSum.LineTotalAmount.Value, actSum.LineTotalAmount.Value, "LineTotalAmount is different.");
        Assert.AreEqual(expSum.TaxBasisTotalAmount.Value, actSum.TaxBasisTotalAmount.Value, "TaxBasisTotalAmount is different.");
        Assert.AreEqual(expSum.GrandTotalAmount.Value, actSum.GrandTotalAmount.Value, "GrandTotalAmount is different.");
        Assert.AreEqual(expSum.DuePayableAmount.Value, actSum.DuePayableAmount.Value, "DuePayableAmount is different.");

        var expTaxTotal = expSum.TaxTotalAmount?.Sum(a => a.Value) ?? 0m;
        var actTaxTotal = actSum.TaxTotalAmount?.Sum(a => a.Value) ?? 0m;
        Assert.AreEqual(expTaxTotal, actTaxTotal, "TaxTotalAmount is different.");

        // Assert: line count
        var expLines = expected.SupplyChainTradeTransaction.IncludedSupplyChainTradeLineItem;
        var actLines = actual.SupplyChainTradeTransaction.IncludedSupplyChainTradeLineItem;

        Assert.AreEqual(expLines.Length, actLines.Length, "The number of line items is different.");

        // Assert: each line including SubInvoiceLines specific fields
        for (var i = 0; i < expLines.Length; i++)
        {
            var e = expLines[i];
            var a = actLines[i];

            Assert.AreEqual(
                e.AssociatedDocumentLineDocument.LineID.Value,
                a.AssociatedDocumentLineDocument.LineID.Value,
                $"LineID in row {i + 1} is different.");

            Assert.AreEqual(
                e.AssociatedDocumentLineDocument.ParentLineID?.Value,
                a.AssociatedDocumentLineDocument.ParentLineID?.Value,
                $"ParentLineID in row {i + 1} is different.");

            Assert.AreEqual(
                e.AssociatedDocumentLineDocument.LineStatusReasonCode?.Value,
                a.AssociatedDocumentLineDocument.LineStatusReasonCode?.Value,
                $"LineStatusReasonCode in row {i + 1} is different.");

            Assert.AreEqual(
                e.SpecifiedTradeProduct.GlobalID?.Value,
                a.SpecifiedTradeProduct.GlobalID?.Value,
                $"GlobalID in row {i + 1} is different.");

            Assert.AreEqual(
                e.SpecifiedTradeProduct.SellerAssignedID?.Value,
                a.SpecifiedTradeProduct.SellerAssignedID?.Value,
                $"SellerAssignedID in row {i + 1} is different.");

            Assert.AreEqual(
                e.SpecifiedTradeProduct.BuyerAssignedID?.Value,
                a.SpecifiedTradeProduct.BuyerAssignedID?.Value,
                $"BuyerAssignedID in row {i + 1} is different.");

            Assert.AreEqual(
                e.SpecifiedTradeProduct.Name?.Value,
                a.SpecifiedTradeProduct.Name?.Value,
                $"Product name in row {i + 1} is different.");

            Assert.AreEqual(
                e.SpecifiedTradeProduct.Description?.Value,
                a.SpecifiedTradeProduct.Description?.Value,
                $"Product description in row {i + 1} is different.");

            Assert.AreEqual(
                e.SpecifiedLineTradeDelivery.BilledQuantity.Value,
                a.SpecifiedLineTradeDelivery.BilledQuantity.Value,
                $"BilledQuantity in row {i + 1} is different.");

            Assert.AreEqual(
                e.SpecifiedLineTradeAgreement.NetPriceProductTradePrice.ChargeAmount.Value,
                a.SpecifiedLineTradeAgreement.NetPriceProductTradePrice.ChargeAmount.Value,
                $"Net price in row {i + 1} is different.");

            Assert.AreEqual(
                e.SpecifiedLineTradeSettlement.ApplicableTradeTax[0].RateApplicablePercent.Value,
                a.SpecifiedLineTradeSettlement.ApplicableTradeTax[0].RateApplicablePercent.Value,
                $"Tax percent in row {i + 1} is different.");

            var expectedLineTotal = e.SpecifiedLineTradeSettlement
                .SpecifiedTradeSettlementLineMonetarySummation
                .LineTotalAmount.Value;
            var actualLineTotal = a.SpecifiedLineTradeSettlement
                .SpecifiedTradeSettlementLineMonetarySummation
                .LineTotalAmount.Value;

            Assert.AreEqual(expectedLineTotal, actualLineTotal, $"LineTotalAmount in row {i + 1} is different.");
        }
    }

    [TestMethod(DisplayName = "Generated invoice matches official extended advance invoice with sub invoice lines and LV reference")]
    public void Generated_invoice_matches_official_extended_advance_invoice_with_sub_invoice_lines_and_lv_reference()
    {
        // Arrange
        var invoice = new Invoice
        {
            InvoiceNumber = "210111 mit LV",
            InvoiceDate = new DateTime(2026, 05, 30),
            Currency = "EUR",
            DocumentTypeCode = "875",

            Seller = new Party
            {
                ID = "998877",
                Name = "Musterbetrieb AG Demodaten",
                Street = "August-Spindler-Strasse 222",
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
                Street = "Gartenstraße 1212",
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
                PaymentReference = "210111 mit LV",
                PaymentMeansTypeCode = "58",
                PaymentMeansInformation = "SEPA credit transfer",
                Iban = "DE75512108001245126199",
                Bic = "PBNKDEFF",
                AccountName = "Musterbetrieb Kontoname",
                Terms =
                [
                    new PaymentTermDetails
                    {
                        Description = "Bei Zahlung bis zum 06.06.2026 zahlen Sie mit 2,50 % Skonto € 4.176,90",
                        DueDate = new DateTime(2026, 06, 06)
                    },
                    new PaymentTermDetails
                    {
                        Description = "Bis zum zum 13.06.2026 ohne Abzug",
                        DueDate = new DateTime(2026, 06, 13)
                    }
                ]
            },

            HeaderChargeTotalAmount = 0.00m,
            HeaderAllowanceTotalAmount = 0.00m,
            HeaderTotalPrepaidAmount = 0.00m,
            HeaderDuePayableAmount = 4284.00m,

            BuyerReference = "Kundenref. BT-10"
        };

        invoice.Notes.AddRange(
        [
            new InvoiceNote { Content = "1. Abschlagsrechnung", SubjectCode = "ACB" },
            new InvoiceNote { Content = "Geschäftsführer: Herr Geschäftsführer , Muster Bau GmbH etc.", SubjectCode = "REG" },
            new InvoiceNote { Content = "Es bestehen Vereinbarungen, aus denen sich Minderungen des Entgelts ergeben können.", SubjectCode = "AAI" },
            new InvoiceNote { Content = "ZUGFeRD vers 2.4.0 Extended", SubjectCode = "ACB" },
            new InvoiceNote { Content = "Dies ist eine Beispiel-Rechnung zur Darstellung einer  Bau-Abschlags-Rechnung mit Sub-Invoice-Lines und Leistungsverzeichnis-Bezug je Position", SubjectCode = "ACB" },
            new InvoiceNote { Content = "Betreff zum LV für eine Kurzinformation zum Bauvorhaben BT-22", SubjectCode = "ACB" },
            new InvoiceNote { Content = "Kopftext für zusätzliche Beschreibungen zur Rechnung. Z.B. als Anschreiben für die Rechnung BT-22", SubjectCode = "ACB" },
            new InvoiceNote { Content = "ergänzneder Fußtext für die Rechnung mit zusätzlichen Angaben. Z.B: Ist kein gesondertes Lieferdatum angegeben, entspricht das Rechnungsdatum dem Datum der Lieferung und Leistung", SubjectCode = "ACB" },
            new InvoiceNote { Content = "freier Text zur Rechnung BT-22", SubjectCode = "ACB" }
        ]);

        invoice.Lines.Add(new InvoiceLine
        {
            LineId = "01.01",
            ParentLineId = "01",
            LineStatusReasonCode = "GROUP",
            Description = "Baugelände abräumen Anfallender Schutt, Pflanzenreste und Müll entsorgen",
            UnitPrice = 7.00m,
            Quantity = 300.00m,
            UnitCode = "H87",
            TaxPercent = 19m,
            TaxCategory = "S",
            ForcedLineTotalAmount = 2100.00m
        });

        invoice.Lines.Add(new InvoiceLine
        {
            LineId = "01.01.01",
            ParentLineId = "01.01",
            LineStatusReasonCode = "DETAIL",
            Description = "Baugelände abräumen",
            UnitPrice = 7.00m,
            Quantity = 100.00m,
            UnitCode = "H87",
            TaxPercent = 19m,
            TaxCategory = "S",
            ForcedLineTotalAmount = 700.00m
        });

        invoice.Lines.Add(new InvoiceLine
        {
            LineId = "01.01.02",
            ParentLineId = "01.01",
            LineStatusReasonCode = "DETAIL",
            Description = "Anfallender Pflanzenreste entsorgen",
            UnitPrice = 7.00m,
            Quantity = 100.00m,
            UnitCode = "H87",
            TaxPercent = 19m,
            TaxCategory = "S",
            ForcedLineTotalAmount = 700.00m
        });

        invoice.Lines.Add(new InvoiceLine
        {
            LineId = "01.01.03",
            ParentLineId = "01.01",
            LineStatusReasonCode = "DETAIL",
            Description = "Müll entsorgen",
            UnitPrice = 7.00m,
            Quantity = 100.00m,
            UnitCode = "H87",
            TaxPercent = 19m,
            TaxCategory = "S",
            ForcedLineTotalAmount = 700.00m
        });

        invoice.Lines.Add(new InvoiceLine
        {
            LineId = "01.02",
            ParentLineId = "01",
            LineStatusReasonCode = "DETAIL",
            Description = "Pflasterfläche vorbereiten, Planum herstellen und verdichten",
            UnitPrice = 6.00m,
            Quantity = 250.00m,
            UnitCode = "MTK",
            TaxPercent = 19m,
            TaxCategory = "S",
            ForcedLineTotalAmount = 1500.00m
        });

        invoice.Lines.Add(new InvoiceLine
        {
            LineId = "01",
            LineStatusReasonCode = "GROUP",
            Description = "Summe 01 Bauabschnitt 1 - Vorarbeiten",
            UnitPrice = 3600.00m,
            Quantity = 1.00m,
            UnitCode = "H87",
            TaxPercent = 19m,
            TaxCategory = "S",
            BuyerOrderLineId = "000001",
            ForcedLineTotalAmount = 3600.00m
        });

        // Act
        var cii = _mapper.Map(invoice);
        var xml = _exporter.ToXml(cii);

        // Assert
        CiiSchemaValidator.ValidateCiiZugferd24Extended(xml);

        SaveXmlFile(invoice, xml);

        var baseDir = AppContext.BaseDirectory;
        var samplePath = Path.Combine(baseDir, "Examples", "ZUGFeRD_Extended__Abschlagsrechnung_SubInvoiceLine_u_LV_Nr_.xml");
        Assert.IsTrue(File.Exists(samplePath), $"Sample XML not found: {samplePath}");

        var sampleXml = File.ReadAllText(samplePath);

        var serializer = new XmlSerializer(typeof(CrossIndustryInvoiceType));
        CrossIndustryInvoiceType expected;
        CrossIndustryInvoiceType actual;

        using (var sr = new StringReader(sampleXml))
            expected = (CrossIndustryInvoiceType)serializer.Deserialize(sr)!;

        using (var sr = new StringReader(xml))
            actual = (CrossIndustryInvoiceType)serializer.Deserialize(sr)!;

        Assert.AreEqual(expected.ExchangedDocumentContext.GuidelineSpecifiedDocumentContextParameter.ID.Value,
            actual.ExchangedDocumentContext.GuidelineSpecifiedDocumentContextParameter.ID.Value,
            "Guideline ID is different.");

        Assert.AreEqual(expected.ExchangedDocument.ID.Value, actual.ExchangedDocument.ID.Value, "Invoice number is different.");
        Assert.AreEqual(expected.ExchangedDocument.TypeCode.Value, actual.ExchangedDocument.TypeCode.Value, "Document type is different.");
        Assert.AreEqual(expected.ExchangedDocument.IssueDateTime.Item.Value, actual.ExchangedDocument.IssueDateTime.Item.Value, "Invoice date is different.");

        Assert.HasCount(expected.ExchangedDocument.IncludedNote.Length, actual.ExchangedDocument.IncludedNote, "Included note count is different.");
        for (int i = 0; i < expected.ExchangedDocument.IncludedNote.Length; i++)
        {
            Assert.AreEqual(expected.ExchangedDocument.IncludedNote[i].SubjectCode.Value, actual.ExchangedDocument.IncludedNote[i].SubjectCode.Value, $"Note subject code at index {i} is different.");
            Assert.AreEqual(expected.ExchangedDocument.IncludedNote[i].Content.Value, actual.ExchangedDocument.IncludedNote[i].Content.Value, $"Note content at index {i} is different.");
        }

        var expAgr = expected.SupplyChainTradeTransaction.ApplicableHeaderTradeAgreement;
        var actAgr = actual.SupplyChainTradeTransaction.ApplicableHeaderTradeAgreement;

        Assert.AreEqual(expAgr.BuyerReference?.Value, actAgr.BuyerReference?.Value, "Buyer reference is different.");
        Assert.AreEqual(expAgr.SellerTradeParty.ID[0].Value, actAgr.SellerTradeParty.ID[0].Value, "Seller ID is different.");
        Assert.AreEqual(expAgr.SellerTradeParty.Name.Value, actAgr.SellerTradeParty.Name.Value, "Seller name is different.");
        
        var expSellerLegalOrgId = expAgr.SellerTradeParty?.SpecifiedLegalOrganization?.ID?.Value;
        var actSellerLegalOrgId = actAgr.SellerTradeParty?.SpecifiedLegalOrganization?.ID?.Value;
        Assert.AreEqual(expSellerLegalOrgId, actSellerLegalOrgId, "Seller legal organization ID is different.");

        Assert.AreEqual(expAgr.BuyerTradeParty.ID[0].Value, actAgr.BuyerTradeParty.ID[0].Value, "Buyer ID is different.");
        Assert.AreEqual(expAgr.BuyerTradeParty.Name.Value, actAgr.BuyerTradeParty.Name.Value, "Buyer name is different.");

        var expDel = expected.SupplyChainTradeTransaction.ApplicableHeaderTradeDelivery;
        var actDel = actual.SupplyChainTradeTransaction.ApplicableHeaderTradeDelivery;

        Assert.AreEqual(expDel.ShipToTradeParty.Name.Value, actDel.ShipToTradeParty.Name.Value, "Ship-to party name is different.");
        Assert.AreEqual(expDel.ActualDeliverySupplyChainEvent.OccurrenceDateTime.Item.Value, actDel.ActualDeliverySupplyChainEvent.OccurrenceDateTime.Item.Value, "Actual delivery date is different.");

        var expSet = expected.SupplyChainTradeTransaction.ApplicableHeaderTradeSettlement;
        var actSet = actual.SupplyChainTradeTransaction.ApplicableHeaderTradeSettlement;

        Assert.AreEqual(expSet.InvoiceCurrencyCode.Value, actSet.InvoiceCurrencyCode.Value, "Invoice currency is different.");

        var expPm = expSet.SpecifiedTradeSettlementPaymentMeans.First();
        var actPm = actSet.SpecifiedTradeSettlementPaymentMeans.First();

        Assert.AreEqual(expPm.TypeCode.Value, actPm.TypeCode.Value, "Payment means type code is different.");
        Assert.AreEqual(expPm.PayeePartyCreditorFinancialAccount.IBANID.Value, actPm.PayeePartyCreditorFinancialAccount.IBANID.Value, "IBAN is different.");
        Assert.AreEqual(expPm.PayeePartyCreditorFinancialAccount.AccountName.Value, actPm.PayeePartyCreditorFinancialAccount.AccountName.Value, "Account name is different.");
        Assert.AreEqual(expPm.PayeeSpecifiedCreditorFinancialInstitution.BICID.Value, actPm.PayeeSpecifiedCreditorFinancialInstitution.BICID.Value, "BIC is different.");

        Assert.HasCount(expSet.SpecifiedTradePaymentTerms.Length, actSet.SpecifiedTradePaymentTerms, "Payment term count is different.");
        for (int i = 0; i < expSet.SpecifiedTradePaymentTerms.Length; i++)
        {
            Assert.AreEqual(expSet.SpecifiedTradePaymentTerms[i].Description.Value, actSet.SpecifiedTradePaymentTerms[i].Description.Value, $"Payment term description at index {i} is different.");
            Assert.AreEqual(expSet.SpecifiedTradePaymentTerms[i].DueDateDateTime.Item.Value, actSet.SpecifiedTradePaymentTerms[i].DueDateDateTime.Item.Value, $"Payment term due date at index {i} is different.");
        }

        var expSum = expSet.SpecifiedTradeSettlementHeaderMonetarySummation;
        var actSum = actSet.SpecifiedTradeSettlementHeaderMonetarySummation;

        Assert.AreEqual(expSum.LineTotalAmount.Value, actSum.LineTotalAmount.Value, "Line total amount is different.");
        Assert.AreEqual(expSum.ChargeTotalAmount.Value, actSum.ChargeTotalAmount.Value, "Charge total amount is different.");
        Assert.AreEqual(expSum.AllowanceTotalAmount.Value, actSum.AllowanceTotalAmount.Value, "Allowance total amount is different.");
        Assert.AreEqual(expSum.TaxBasisTotalAmount.Value, actSum.TaxBasisTotalAmount.Value, "Tax basis total amount is different.");
        Assert.AreEqual(expSum.TaxTotalAmount.Sum(x => x.Value), actSum.TaxTotalAmount.Sum(x => x.Value), "Tax total amount is different.");
        Assert.AreEqual(expSum.GrandTotalAmount.Value, actSum.GrandTotalAmount.Value, "Grand total amount is different.");
        Assert.AreEqual(expSum.TotalPrepaidAmount.Value, actSum.TotalPrepaidAmount.Value, "Total prepaid amount is different.");
        Assert.AreEqual(expSum.DuePayableAmount.Value, actSum.DuePayableAmount.Value, "Due payable amount is different.");

        var expTaxes = expSet.ApplicableTradeTax;
        var actTaxes = actSet.ApplicableTradeTax;
        Assert.HasCount(expTaxes.Length, actTaxes, "Header tax entry count is different.");
        for (int i = 0; i < expTaxes.Length; i++)
        {
            Assert.AreEqual(expTaxes[i].CalculatedAmount.Value, actTaxes[i].CalculatedAmount.Value, $"Header tax calculated amount at index {i} is different.");
            Assert.AreEqual(expTaxes[i].BasisAmount.Value, actTaxes[i].BasisAmount.Value, $"Header tax basis amount at index {i} is different.");
            Assert.AreEqual(expTaxes[i].CategoryCode.Value, actTaxes[i].CategoryCode.Value, $"Header tax category at index {i} is different.");
            Assert.AreEqual(expTaxes[i].RateApplicablePercent.Value, actTaxes[i].RateApplicablePercent.Value, $"Header tax rate at index {i} is different.");
        }

        var expLines = expected.SupplyChainTradeTransaction.IncludedSupplyChainTradeLineItem;
        var actLines = actual.SupplyChainTradeTransaction.IncludedSupplyChainTradeLineItem;

        Assert.HasCount(expLines.Length, actLines, "Line item count is different.");

        var actById = actLines.ToDictionary(x => x.AssociatedDocumentLineDocument.LineID.Value, x => x);

        foreach (var e in expLines)
        {
            var id = e.AssociatedDocumentLineDocument?.LineID?.Value;
            Assert.IsFalse(string.IsNullOrWhiteSpace(id), "Expected line ID is missing.");
            Assert.IsTrue(actById.ContainsKey(id), $"Line ID {id} is missing in generated XML.");

            var a = actById[id];

            Assert.AreEqual(e.AssociatedDocumentLineDocument?.ParentLineID?.Value, a.AssociatedDocumentLineDocument?.ParentLineID?.Value, $"Parent line ID at {id} is different.");
            Assert.AreEqual(e.AssociatedDocumentLineDocument?.LineStatusReasonCode?.Value, a.AssociatedDocumentLineDocument?.LineStatusReasonCode?.Value, $"Line status reason code at {id} is different.");
            Assert.AreEqual(e.SpecifiedTradeProduct?.GlobalID?.Value, a.SpecifiedTradeProduct?.GlobalID?.Value, $"Global ID at {id} is different.");
            Assert.AreEqual(e.SpecifiedTradeProduct?.GlobalID?.schemeID, a.SpecifiedTradeProduct?.GlobalID?.schemeID, $"Global ID scheme at {id} is different.");
            Assert.AreEqual(e.SpecifiedTradeProduct?.SellerAssignedID?.Value, a.SpecifiedTradeProduct?.SellerAssignedID?.Value, $"Seller assigned ID at {id} is different.");
            Assert.AreEqual(e.SpecifiedTradeProduct?.BuyerAssignedID?.Value, a.SpecifiedTradeProduct?.BuyerAssignedID?.Value, $"Buyer assigned ID at {id} is different.");
            Assert.AreEqual(e.SpecifiedTradeProduct?.Name?.Value, a.SpecifiedTradeProduct?.Name?.Value, $"Product name at {id} is different.");
            Assert.AreEqual(e.SpecifiedTradeProduct?.Description?.Value, a.SpecifiedTradeProduct?.Description?.Value, $"Product description at {id} is different.");

            var expBuyerOrderLineId = e.SpecifiedLineTradeAgreement?.BuyerOrderReferencedDocument?.LineID?.Value;
            var actBuyerOrderLineId = a.SpecifiedLineTradeAgreement?.BuyerOrderReferencedDocument?.LineID?.Value;
            Assert.AreEqual(expBuyerOrderLineId, actBuyerOrderLineId, $"Buyer order line ID at {id} is different.");

            Assert.AreEqual(e.SpecifiedLineTradeAgreement?.NetPriceProductTradePrice?.ChargeAmount?.Value, a.SpecifiedLineTradeAgreement?.NetPriceProductTradePrice?.ChargeAmount?.Value, $"Charge amount at {id} is different.");
            Assert.AreEqual(e.SpecifiedLineTradeAgreement?.NetPriceProductTradePrice?.BasisQuantity?.Value, a.SpecifiedLineTradeAgreement?.NetPriceProductTradePrice?.BasisQuantity?.Value, $"Basis quantity at {id} is different.");
            Assert.AreEqual(e.SpecifiedLineTradeAgreement?.NetPriceProductTradePrice?.BasisQuantity?.unitCode, a.SpecifiedLineTradeAgreement?.NetPriceProductTradePrice?.BasisQuantity?.unitCode, $"Basis quantity unit code at {id} is different.");
            Assert.AreEqual(e.SpecifiedLineTradeDelivery?.BilledQuantity?.Value, a.SpecifiedLineTradeDelivery?.BilledQuantity?.Value, $"Billed quantity at {id} is different.");
            Assert.AreEqual(e.SpecifiedLineTradeDelivery?.BilledQuantity?.unitCode, a.SpecifiedLineTradeDelivery?.BilledQuantity?.unitCode, $"Billed quantity unit code at {id} is different.");

            var expectedLineTaxes = e.SpecifiedLineTradeSettlement?.ApplicableTradeTax ?? Array.Empty<TradeTaxType>();
            var actualLineTaxes = a.SpecifiedLineTradeSettlement?.ApplicableTradeTax ?? Array.Empty<TradeTaxType>();
            Assert.AreEqual(expTaxes.Length, actTaxes.Length, $"Tax entry count at {id} is different.");

            for (int taxIndex = 0; taxIndex < expTaxes.Length; taxIndex++)
            {
                Assert.AreEqual(expTaxes[taxIndex]?.CategoryCode?.Value, actTaxes[taxIndex]?.CategoryCode?.Value, $"Tax category at {id}, entry {taxIndex + 1} is different.");
                Assert.AreEqual(expTaxes[taxIndex]?.RateApplicablePercent?.Value, actTaxes[taxIndex]?.RateApplicablePercent?.Value, $"Tax percent at {id}, entry {taxIndex + 1} is different.");
            }

            Assert.AreEqual(e.SpecifiedLineTradeSettlement?.SpecifiedTradeSettlementLineMonetarySummation?.LineTotalAmount?.Value, a.SpecifiedLineTradeSettlement?.SpecifiedTradeSettlementLineMonetarySummation?.LineTotalAmount?.Value, $"Line total amount at {id} is different.");
        }
    }

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
