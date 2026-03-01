using tulo.eInvoiceXmlGeneratorCii.Models;
using System.Globalization;
using Zugferd24.Extended;

namespace tulo.eInvoiceXmlGeneratorCii.Mappers;

public class CiiMapper : ICiiMapper
{
    public CrossIndustryInvoiceType Map(Invoice inv)
    {
        if (inv == null) throw new ArgumentNullException(nameof(inv));
        var currency = string.IsNullOrWhiteSpace(inv.Currency) ? "EUR" : inv.Currency;

        var typeCodeValue = string.IsNullOrWhiteSpace(inv.DocumentTypeCode) ? "380" : inv.DocumentTypeCode;

        var invoice = new CrossIndustryInvoiceType
        {
            ExchangedDocumentContext = CreateExchangedDocumentContext(),

            ExchangedDocument = new ExchangedDocumentType
            {
                ID = new IDType { Value = inv.InvoiceNumber },
                Name = string.IsNullOrWhiteSpace(inv.DocumentName) ? null : new TextType { Value = inv.DocumentName },
                TypeCode = new DocumentCodeType { Value = typeCodeValue },
                IssueDateTime = new DateTimeType { Item = new DateTimeTypeDateTimeString { format = "102", Value = inv.InvoiceDate.ToString("yyyyMMdd") } },
                IncludedNote = MapNotes(inv)
            },
            SupplyChainTradeTransaction = new SupplyChainTradeTransactionType
            {
                ApplicableHeaderTradeAgreement = new HeaderTradeAgreementType
                {
                    SellerTradeParty = MapParty(inv.Seller),
                    BuyerTradeParty = MapParty(inv.Buyer)
                },
                ApplicableHeaderTradeDelivery = MapHeaderDelivery(inv),
                IncludedSupplyChainTradeLineItem = [.. inv.Lines.Select((line, idx) => MapLine(inv, line, idx + 1, currency))],
                ApplicableHeaderTradeSettlement = MapHeaderSettlement(inv)
            }
        };

        return invoice;
    }

    private TradePartyType MapParty(Party p)
    {
        if (p == null) return null!;

        var party = new TradePartyType
        {
            Name = string.IsNullOrWhiteSpace(p.Name) ? null : new TextType { Value = p.Name },
            PostalTradeAddress = string.IsNullOrWhiteSpace(p.Street) &&
                                  string.IsNullOrWhiteSpace(p.Zip) &&
                                  string.IsNullOrWhiteSpace(p.City) &&
                                  string.IsNullOrWhiteSpace(p.CountryCode) ? null : new TradeAddressType
                                  {
                                      LineOne = string.IsNullOrWhiteSpace(p.Street) ? null : new TextType { Value = p.Street },
                                      PostcodeCode = string.IsNullOrWhiteSpace(p.Zip) ? null : new CodeType { Value = p.Zip },
                                      CityName = string.IsNullOrWhiteSpace(p.City) ? null : new TextType { Value = p.City },
                                      CountryID = string.IsNullOrWhiteSpace(p.CountryCode) ? null : new CountryIDType { Value = p.CountryCode }
                                  }
        };

        //Identifier
        if (!string.IsNullOrWhiteSpace(p.ID))
            party.ID = [new IDType { Value = p.ID }];

        // Contact: <ram:DefinedTradeContact>...
        if (!string.IsNullOrWhiteSpace(p.ContactPersonName) || !string.IsNullOrWhiteSpace(p.ContactPhone) || !string.IsNullOrWhiteSpace(p.ContactEmail))
        {
            party.DefinedTradeContact = [
                 new TradeContactType
                    {
                        PersonName = string.IsNullOrWhiteSpace(p.ContactPersonName) ? null : new TextType { Value = p.ContactPersonName },
                        TelephoneUniversalCommunication = string.IsNullOrWhiteSpace(p.ContactPhone) ? null : new UniversalCommunicationType { CompleteNumber = new TextType { Value = p.ContactPhone } },
                        EmailURIUniversalCommunication = string.IsNullOrWhiteSpace(p.ContactEmail) ? null :  new UniversalCommunicationType { URIID = new IDType { Value = p.ContactEmail } }
                     }
            ];
        }

        // General email (info@...) as in the example: schemeID="0088"
        // Attention: you only have ONE URIUniversalCommunication field – set priority:
        if (!string.IsNullOrWhiteSpace(p.GeneralEmail))
        {
            party.URIUniversalCommunication = new UniversalCommunicationType { URIID = new IDType { schemeID = "0088", Value = p.GeneralEmail } };
        }
        else if (!string.IsNullOrWhiteSpace(p.LeitwegId))
        {
            party.URIUniversalCommunication = new UniversalCommunicationType { URIID = new IDType { schemeID = "0204", Value = p.LeitwegId } };
        }

        // Tax registrations: FC + VA (as in your reference)
        var regs = new List<TaxRegistrationType>();

        if (!string.IsNullOrWhiteSpace(p.FiscalId))
            regs.Add(new TaxRegistrationType { ID = new IDType { schemeID = "FC", Value = p.FiscalId } });

        if (!string.IsNullOrWhiteSpace(p.VatId))
            regs.Add(new TaxRegistrationType { ID = new IDType { schemeID = "VA", Value = p.VatId } });

        if (regs.Count > 0)
            party.SpecifiedTaxRegistration = regs.ToArray();

        return party;
    }

    private SupplyChainTradeLineItemType MapLine(Invoice inv, InvoiceLine invLine, int lineNo, string currency)
    {
        if (invLine == null) throw new ArgumentNullException(nameof(invLine));

        var lineNet = ComputeLineNet(invLine);

        return new SupplyChainTradeLineItemType
        {
            AssociatedDocumentLineDocument = new DocumentLineDocumentType
            {
                LineID = new IDType { Value = lineNo.ToString(CultureInfo.InvariantCulture) }
            },
            SpecifiedTradeProduct = new TradeProductType
            {
                GlobalID = string.IsNullOrWhiteSpace(invLine.GlobalId) ? null : new IDType
                {
                    schemeID = string.IsNullOrWhiteSpace(invLine.GlobalIdSchemeId) ? null : invLine.GlobalIdSchemeId,
                    Value = invLine.GlobalId
                },
                SellerAssignedID = string.IsNullOrWhiteSpace(invLine.SellerAssignedId) ? null : new IDType { Value = invLine.SellerAssignedId },
                Name = string.IsNullOrWhiteSpace(invLine.Description) ? null : new TextType { Value = invLine.Description },
                Description = string.IsNullOrWhiteSpace(invLine.ProductDescription) ? null : new TextType { Value = invLine.ProductDescription },
                OriginTradeCountry = string.IsNullOrWhiteSpace(invLine.OriginCountryCode) ? null : new TradeCountryType { ID = new CountryIDType { Value = invLine.OriginCountryCode } }  // z.B. "DE"
            },
            SpecifiedLineTradeAgreement = new LineTradeAgreementType
            {
                BuyerOrderReferencedDocument = string.IsNullOrWhiteSpace(invLine.BuyerOrderReferencedId) && invLine.BuyerOrderDate == null ? null : new ReferencedDocumentType
                {
                    IssuerAssignedID = string.IsNullOrWhiteSpace(invLine.BuyerOrderReferencedId) ? null : new IDType { Value = invLine.BuyerOrderReferencedId },
                    FormattedIssueDateTime = invLine.DeliveryNoteDate == null ? null : new FormattedDateTimeType
                    {
                        DateTimeString = new FormattedDateTimeTypeDateTimeString { format = "102", Value = invLine.DeliveryNoteDate.Value.ToString("yyyyMMdd") }
                    }
                },

                GrossPriceProductTradePrice = new TradePriceType
                {
                    ChargeAmount = new AmountType { currencyID = currency, Value = invLine.UnitPrice },
                    BasisQuantity = new QuantityType { unitCode = invLine.UnitCode, Value = 1m }
                },
                NetPriceProductTradePrice = new TradePriceType
                {
                    ChargeAmount = new AmountType { currencyID = currency, Value = invLine.UnitPrice },
                    BasisQuantity = new QuantityType { unitCode = invLine.UnitCode, Value = 1m }
                }
            },
            SpecifiedLineTradeDelivery = new LineTradeDeliveryType
            {
                BilledQuantity = new QuantityType { unitCode = invLine.UnitCode, Value = invLine.Quantity },
                ShipToTradeParty = MapParty(inv.Buyer),
                DeliveryNoteReferencedDocument = string.IsNullOrWhiteSpace(invLine.DeliveryNoteNumber) && string.IsNullOrWhiteSpace(invLine.DeliveryNoteLineId) &&
                    invLine.DeliveryNoteDate == null ? null : new ReferencedDocumentType
                    {
                        IssuerAssignedID = string.IsNullOrWhiteSpace(invLine.DeliveryNoteNumber) ? null : new IDType { Value = invLine.DeliveryNoteNumber },
                        LineID = string.IsNullOrWhiteSpace(invLine.DeliveryNoteLineId) ? null : new IDType { Value = invLine.DeliveryNoteLineId },
                        FormattedIssueDateTime = invLine.DeliveryNoteDate == null ? null : new FormattedDateTimeType
                        {
                            DateTimeString = new FormattedDateTimeTypeDateTimeString { format = "102", Value = invLine.DeliveryNoteDate.Value.ToString("yyyyMMdd") }
                        }
                    }
            },
            SpecifiedLineTradeSettlement = new LineTradeSettlementType
            {
                ApplicableTradeTax = new[]
                {
                        new TradeTaxType
                        {
                            TypeCode = new TaxTypeCodeType { Value = "VAT" },
                            CategoryCode = new TaxCategoryCodeType { Value = invLine.TaxCategory },
                            BasisAmount = new AmountType { currencyID = currency, Value = lineNet },
                            CalculatedAmount = new AmountType { currencyID = currency, Value = Math.Round(lineNet * invLine.TaxPercent / 100m,2,MidpointRounding.AwayFromZero) },
                            RateApplicablePercent = new PercentType { Value = invLine.TaxPercent }
                        }
                    },
                BillingSpecifiedPeriod = invLine.BillingPeriodEndDate == null ? null : new SpecifiedPeriodType
                {
                    EndDateTime = new DateTimeType { Item = new DateTimeTypeDateTimeString { format = "102", Value = invLine.BillingPeriodEndDate.Value.ToString("yyyyMMdd") } }
                },
                SpecifiedTradeSettlementLineMonetarySummation = new TradeSettlementLineMonetarySummationType
                {
                    LineTotalAmount = new AmountType { currencyID = currency, Value = lineNet },
                    TotalAllowanceChargeAmount = new AmountType { currencyID = currency, Value = 0m }
                },
                AdditionalReferencedDocument = string.IsNullOrWhiteSpace(invLine.AdditionalReferencedDocumentId) && string.IsNullOrWhiteSpace(invLine.AdditionalReferencedDocumentTypeCode)
                                                && string.IsNullOrWhiteSpace(invLine.AdditionalReferencedDocumentReferenceTypeCode) ? null : new[]
                {
                    new ReferencedDocumentType
                    {
                        IssuerAssignedID = string.IsNullOrWhiteSpace(invLine.AdditionalReferencedDocumentId)  ? null : new IDType { Value = invLine.AdditionalReferencedDocumentId },
                        TypeCode = string.IsNullOrWhiteSpace(invLine.AdditionalReferencedDocumentTypeCode) ? null : new DocumentCodeType { Value = invLine.AdditionalReferencedDocumentTypeCode },
                        ReferenceTypeCode = string.IsNullOrWhiteSpace(invLine.AdditionalReferencedDocumentReferenceTypeCode) ? null : new ReferenceCodeType { Value = invLine.AdditionalReferencedDocumentReferenceTypeCode }
                    }
                }

            }
        };
    }

    private HeaderTradeSettlementType MapHeaderSettlement(Invoice inv)
    {
        var currency = string.IsNullOrWhiteSpace(inv.Currency) ? "EUR" : inv.Currency;

        if (inv.Lines == null || inv.Lines.Count == 0)
            throw new InvalidOperationException("Invoice muss mindestens eine Position haben.");

        decimal LineNet(InvoiceLine l) => Math.Round(l.Quantity * l.UnitPrice, 2, MidpointRounding.AwayFromZero);

        var netTotal = inv.Lines.Sum(LineNet);
        var headerTaxes = MapHeaderTradeTax(inv, currency);
        var chargeTotal = inv.HeaderChargeTotalAmount;
        var allowanceTotal = inv.HeaderAllowanceTotalAmount;
        var taxTotal = headerTaxes?.Sum(t => t.CalculatedAmount.Value) ?? 0m;
        var grandTotal = netTotal + taxTotal;
        var prepaidTotal = inv.HeaderTotalPrepaidAmount;
        var duePayable = inv.HeaderDuePayableAmount;

        var settlement = new HeaderTradeSettlementType
        {
            PaymentReference = new TextType { Value = inv.Payment.PaymentReference },
            InvoiceCurrencyCode = new CurrencyCodeType { Value = currency },
            ApplicableTradeTax = headerTaxes,
            SpecifiedTradeSettlementHeaderMonetarySummation =
                new TradeSettlementHeaderMonetarySummationType
                {
                    LineTotalAmount = new AmountType { currencyID = currency, Value = netTotal },
                    ChargeTotalAmount = new AmountType { currencyID = currency, Value = chargeTotal },
                    AllowanceTotalAmount = new AmountType { currencyID = currency, Value = allowanceTotal },
                    TaxBasisTotalAmount = new AmountType { currencyID = currency, Value = netTotal },  // 554,55
                    TaxTotalAmount = new[] { new AmountType { currencyID = currency, Value = taxTotal } },  // 100,83
                    GrandTotalAmount = new AmountType { currencyID = currency, Value = grandTotal },  // 655,38
                    TotalPrepaidAmount = new AmountType { currencyID = currency, Value = prepaidTotal },
                    DuePayableAmount = new AmountType { currencyID = currency, Value = duePayable }  // 655,38
                }
        };

        // Payment method / IBAN / BIC / Conditions from PaymentDetails
        if (inv.Payment != null)
        {
            settlement.SpecifiedTradeSettlementPaymentMeans = MapPaymentMeans(inv.Payment, currency);
            settlement.SpecifiedTradePaymentTerms = MapPaymentTerms(inv.Payment);
        }

        return settlement;
    }

    private TradeSettlementPaymentMeansType[] MapPaymentMeans(PaymentDetails payment, string currency)
    {
        if (payment == null) return null!;

        var hasAnything = !string.IsNullOrWhiteSpace(payment.PaymentMeansTypeCode) ||
                              !string.IsNullOrWhiteSpace(payment.PaymentMeansInformation) ||
                              !string.IsNullOrWhiteSpace(payment.Iban) ||
                              !string.IsNullOrWhiteSpace(payment.Bic) ||
                              !string.IsNullOrWhiteSpace(payment.AccountName);

        if (!hasAnything)
            return null!;

        var means = new TradeSettlementPaymentMeansType
        {
            // 58 = SEPA Direct Debit, 31 = Bank transfer etc.
            TypeCode = string.IsNullOrWhiteSpace(payment.PaymentMeansTypeCode) ? null : new PaymentMeansCodeType { Value = payment.PaymentMeansTypeCode },
            Information = string.IsNullOrWhiteSpace(payment.PaymentMeansInformation) ? null : new TextType { Value = payment.PaymentMeansInformation },
            PayeePartyCreditorFinancialAccount =
                string.IsNullOrWhiteSpace(payment.Iban) && string.IsNullOrWhiteSpace(payment.AccountName) ? null : new CreditorFinancialAccountType
                {
                    IBANID = string.IsNullOrWhiteSpace(payment.Iban) ? null : new IDType { Value = payment.Iban },
                    AccountName = string.IsNullOrWhiteSpace(payment.AccountName) ? null : new TextType { Value = payment.AccountName }
                },
            PayeeSpecifiedCreditorFinancialInstitution = string.IsNullOrWhiteSpace(payment.Bic) ? null : new CreditorFinancialInstitutionType { BICID = new IDType { Value = payment.Bic } }
        };

        return [means];
    }

    private TradePaymentTermsType[] MapPaymentTerms(PaymentDetails payment)
    {
        if (payment == null) return null!;

        var hasStructuredTerms = payment.Terms != null && payment.Terms.Count > 0;
        var hasLegacyTerms = !string.IsNullOrWhiteSpace(payment.PaymentTermsText) || payment.DueDate != null || !string.IsNullOrWhiteSpace(payment.DirectDebitMandateId);

        if (!hasStructuredTerms && !hasLegacyTerms)
            return null!;

        DateTimeType MakeDate(DateTime dt) => new DateTimeType { Item = new DateTimeTypeDateTimeString { format = "102", Value = dt.ToString("yyyyMMdd") } };

        if (hasStructuredTerms)
        {
            return payment.Terms!.Select(t => new TradePaymentTermsType
            {
                Description = string.IsNullOrWhiteSpace(t.Description) ? null : new TextType { Value = t.Description },
                DueDateDateTime = t.DueDate == null ? null : MakeDate(t.DueDate.Value),
                ApplicableTradePaymentDiscountTerms = t.DiscountTerms == null ? null : new TradePaymentDiscountTermsType
                {
                    BasisDateTime = MakeDate(t.DiscountTerms.BasisDate),
                    BasisPeriodMeasure = new MeasureType { unitCode = "DAY", Value = t.DiscountTerms.BasisPeriodDays },
                    BasisAmount = new AmountType { Value = t.DiscountTerms.BasisAmount },
                    CalculationPercent = new PercentType { Value = t.DiscountTerms.CalculationPercent },
                    ActualDiscountAmount = new AmountType { Value = t.DiscountTerms.ActualDiscountAmount }
                }
            }).ToArray();
        }

        // Fallback: single field
        return
        [
            new TradePaymentTermsType
        {
            Description = string.IsNullOrWhiteSpace(payment.PaymentTermsText) ? null : new TextType { Value = payment.PaymentTermsText },
            DueDateDateTime = payment.DueDate == null ? null : MakeDate(payment.DueDate.Value),
            DirectDebitMandateID = string.IsNullOrWhiteSpace(payment.DirectDebitMandateId) ? null : new IDType { Value = payment.DirectDebitMandateId }
        }
        ];
    }

    private ExchangedDocumentContextType CreateExchangedDocumentContext()
    {
        return new ExchangedDocumentContextType
        {
            GuidelineSpecifiedDocumentContextParameter = new DocumentContextParameterType { ID = new IDType { Value = "urn:cen.eu:en16931:2017#conformant#urn:factur-x.eu:1p0:extended" } }  // Official value for EN16931-compliant Factur-X/ZUGFeRD Extended
        };
    }

    private HeaderTradeDeliveryType MapHeaderDelivery(Invoice inv)
    {
        var delivery = new HeaderTradeDeliveryType
        {
            ShipToTradeParty = MapParty(inv.Buyer),      // ShipTo = Buyer (typischer Fall)
            ShipFromTradeParty = MapParty(inv.Seller),  // Optional: ShipFrom = Seller
            // Actual delivery date (here: invoice date)
            ActualDeliverySupplyChainEvent = new SupplyChainEventType { OccurrenceDateTime = new DateTimeType { Item = new DateTimeTypeDateTimeString { format = "102", Value = inv.InvoiceDate.ToString("yyyyMMdd") } } }
        };

        return delivery;
    }

    private TradeTaxType[] MapHeaderTradeTax(Invoice inv, string currency)
    {
        if (inv.Lines == null || inv.Lines.Count == 0)
            return null!;

        // Same rounding logic as in the header
        decimal LineNet(InvoiceLine l) => Math.Round(l.Quantity * l.UnitPrice, 2, MidpointRounding.AwayFromZero);

        var groups = inv.Lines.GroupBy(l => new { l.TaxPercent, l.TaxCategory });

        var result = new List<TradeTaxType>();

        foreach (var g in groups)
        {
            var basisAmount = g.Sum(LineNet);
            var taxAmount = Math.Round(basisAmount * g.Key.TaxPercent / 100m, 2, MidpointRounding.AwayFromZero);

            result.Add(new TradeTaxType
            {
                TypeCode = new TaxTypeCodeType { Value = "VAT" },
                CategoryCode = new TaxCategoryCodeType { Value = g.Key.TaxCategory }, // "S"
                BasisAmount = new AmountType { currencyID = currency, Value = basisAmount },
                CalculatedAmount = new AmountType { currencyID = currency, Value = taxAmount },
                RateApplicablePercent = new PercentType { Value = g.Key.TaxPercent }
            });
        }

        return result.ToArray();
    }

    private NoteType[] MapNotes(Invoice inv)
    {
        if (inv.Notes == null || inv.Notes.Count == 0)
            return null!;

        return [.. inv.Notes
            .Where(n => !string.IsNullOrWhiteSpace(n.Content))
            .Select(n => new NoteType
            {
                Content = new TextType { Value = n.Content },
                SubjectCode = string.IsNullOrWhiteSpace(n.SubjectCode) ? null : new CodeType { Value = n.SubjectCode },
                ContentCode = string.IsNullOrWhiteSpace(n.ContentCode) ? null : new CodeType { Value = n.ContentCode }
            })];
    }

    private static decimal ComputeLineNet(InvoiceLine line)
    {
        return Math.Round(line.Quantity * line.UnitPrice, 2, MidpointRounding.AwayFromZero);
    }
}