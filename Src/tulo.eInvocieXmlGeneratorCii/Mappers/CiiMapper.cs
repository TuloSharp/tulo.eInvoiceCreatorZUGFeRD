using tulo.eInvoiceXmlGeneratorCii.Models;
using System.Globalization;
using Zugferd24.Extended;

namespace tulo.eInvoiceXmlGeneratorCii.Mappers;

public class CiiMapper : ICiiMapper
{
    public CrossIndustryInvoiceType Map(Invoice inv)
    {
        if (inv == null) return new CrossIndustryInvoiceType();
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
                IssueDateTime = new DateTimeType
                {
                    Item = new DateTimeTypeDateTimeString
                    {
                        format = "102",
                        Value = inv.InvoiceDate.ToString("yyyyMMdd")
                    }
                },
                IncludedNote = MapNotes(inv)
            },

            SupplyChainTradeTransaction = new SupplyChainTradeTransactionType
            {
                ApplicableHeaderTradeAgreement = new HeaderTradeAgreementType
                {
                    BuyerReference = string.IsNullOrWhiteSpace(inv.BuyerReference) ? null : new TextType { Value = inv.BuyerReference },
                    SellerTradeParty = MapParty(inv.Seller),
                    BuyerTradeParty = MapParty(inv.Buyer),
                    SellerOrderReferencedDocument = string.IsNullOrWhiteSpace(inv.SellerOrderReferencedId) ? null : new ReferencedDocumentType
                    {
                        IssuerAssignedID = new IDType { Value = inv.SellerOrderReferencedId }
                    },

                    BuyerOrderReferencedDocument = string.IsNullOrWhiteSpace(inv.BuyerOrderReferencedId) ? null : new ReferencedDocumentType
                    {
                        IssuerAssignedID = new IDType { Value = inv.BuyerOrderReferencedId }
                    },

                    ContractReferencedDocument = string.IsNullOrWhiteSpace(inv.ContractReferencedId) ? null : new ReferencedDocumentType
                    {
                        IssuerAssignedID = new IDType { Value = inv.ContractReferencedId }
                    }
                },

                ApplicableHeaderTradeDelivery = MapHeaderDelivery(inv),
                IncludedSupplyChainTradeLineItem = inv.Lines == null ? [] : [.. inv.Lines.Select((line, idx) => MapLine(inv, line, idx + 1, currency)).Where(x => x != null)],
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

        // Identifier
        if (!string.IsNullOrWhiteSpace(p.ID))
            party.ID = [new IDType { Value = p.ID }];

        // Legal organization
        if (!string.IsNullOrWhiteSpace(p.FiscalId))
        {
            party.SpecifiedLegalOrganization = new LegalOrganizationType
            {
                ID = new IDType { Value = p.FiscalId }
            };
        }

        // Contact
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

        // General email or Leitweg-ID
        if (!string.IsNullOrWhiteSpace(p.GeneralEmail))
        {
            party.URIUniversalCommunication = new UniversalCommunicationType { URIID = new IDType { schemeID = "0088", Value = p.GeneralEmail } };
        }
        else if (!string.IsNullOrWhiteSpace(p.LeitwegId))
        {
            party.URIUniversalCommunication = new UniversalCommunicationType { URIID = new IDType { schemeID = "0204", Value = p.LeitwegId } };
        }

        // Legal organization
        if (!string.IsNullOrWhiteSpace(p.FiscalId))
        {
            party.SpecifiedLegalOrganization = new LegalOrganizationType
            {
                ID = new IDType { Value = p.FiscalId }
            };
        }

        // Tax registrations
        var regs = new List<TaxRegistrationType>();

        if (!string.IsNullOrWhiteSpace(p.TaxRegistrationFcId))
            regs.Add(new TaxRegistrationType { ID = new IDType { schemeID = "FC", Value = p.TaxRegistrationFcId } });

        if (!string.IsNullOrWhiteSpace(p.VatId))
            regs.Add(new TaxRegistrationType { ID = new IDType { schemeID = "VA", Value = p.VatId } });

        if (regs.Count > 0)
            party.SpecifiedTaxRegistration = regs.ToArray();

        if (!string.IsNullOrWhiteSpace(p.ID))
            party.ID = [new IDType { Value = p.ID }];

        if (!string.IsNullOrWhiteSpace(p.GlobalId))
            party.GlobalID = [new IDType { schemeID = string.IsNullOrWhiteSpace(p.GlobalIdSchemeId) ? null : p.GlobalIdSchemeId, Value = p.GlobalId }];

        return party;
    }

    private SupplyChainTradeLineItemType MapLine(Invoice inv, InvoiceLine invLine, int lineNo, string currency)
    {
        if (invLine == null) return null!;

        var lineNet = invLine.ForcedLineTotalAmount ?? ComputeLineNet(invLine);
        var priceBasisQuantity = invLine.PriceBasisQuantity.GetValueOrDefault(1m);

        var isChildOrDetailLine = !string.IsNullOrWhiteSpace(invLine.ParentLineId) || string.Equals(invLine.LineStatusReasonCode, "DETAIL", StringComparison.OrdinalIgnoreCase);
        var omitGrossPrice = !string.IsNullOrWhiteSpace(invLine.ParentLineId) || string.Equals(invLine.LineStatusReasonCode, "DETAIL", StringComparison.OrdinalIgnoreCase) || string.Equals(invLine.LineStatusReasonCode, "GROUP", StringComparison.OrdinalIgnoreCase);
        var omitLineTaxAmounts = !string.IsNullOrWhiteSpace(invLine.ParentLineId) || string.Equals(invLine.LineStatusReasonCode, "DETAIL", StringComparison.OrdinalIgnoreCase) || string.Equals(invLine.LineStatusReasonCode, "GROUP", StringComparison.OrdinalIgnoreCase);
        var omitNetPriceBasisQuantity = invLine.OmitNetPriceBasisQuantity ?? isChildOrDetailLine;

        var grossBasisQuantity = omitGrossPrice ? null : new QuantityType { unitCode = invLine.UnitCode, Value = priceBasisQuantity };
        var netBasisQuantity = omitNetPriceBasisQuantity ? null : new QuantityType { unitCode = invLine.UnitCode, Value = priceBasisQuantity };

        var chargeAmount = Math.Round(invLine.UnitPrice * priceBasisQuantity, 2, MidpointRounding.AwayFromZero);

        return new SupplyChainTradeLineItemType
        {
            AssociatedDocumentLineDocument = new DocumentLineDocumentType
            {
                LineID = new IDType { Value = !string.IsNullOrWhiteSpace(invLine.LineId) ? invLine.LineId : lineNo.ToString(CultureInfo.InvariantCulture) },
                ParentLineID = string.IsNullOrWhiteSpace(invLine.ParentLineId) ? null : new IDType { Value = invLine.ParentLineId },
                LineStatusReasonCode = string.IsNullOrWhiteSpace(invLine.LineStatusReasonCode) ? null : new CodeType { Value = invLine.LineStatusReasonCode },
                IncludedNote = MapLineNotes(invLine)
            },

            SpecifiedTradeProduct = new TradeProductType
            {
                GlobalID = string.IsNullOrWhiteSpace(invLine.GlobalId) ? null : new IDType { schemeID = string.IsNullOrWhiteSpace(invLine.GlobalIdSchemeId) ? null : invLine.GlobalIdSchemeId, Value = invLine.GlobalId },
                SellerAssignedID = string.IsNullOrWhiteSpace(invLine.SellerAssignedId) ? null : new IDType { Value = invLine.SellerAssignedId },
                BuyerAssignedID = string.IsNullOrWhiteSpace(invLine.BuyerAssignedId) ? null : new IDType { Value = invLine.BuyerAssignedId },
                Name = string.IsNullOrWhiteSpace(invLine.Description) ? null : new TextType { Value = invLine.Description },
                Description = string.IsNullOrWhiteSpace(invLine.ProductDescription) ? null : new TextType { Value = invLine.ProductDescription },
                OriginTradeCountry = string.IsNullOrWhiteSpace(invLine.OriginCountryCode) ? null : new TradeCountryType { ID = new CountryIDType { Value = invLine.OriginCountryCode } }
            },

            SpecifiedLineTradeAgreement = new LineTradeAgreementType
            {
                BuyerOrderReferencedDocument =
                    string.IsNullOrWhiteSpace(invLine.BuyerOrderReferencedId) &&
                    string.IsNullOrWhiteSpace(invLine.BuyerOrderLineId) &&
                    invLine.BuyerOrderDate == null
                        ? null
                        : new ReferencedDocumentType
                        {
                            IssuerAssignedID = string.IsNullOrWhiteSpace(invLine.BuyerOrderReferencedId) ? null : new IDType { Value = invLine.BuyerOrderReferencedId },
                            LineID = string.IsNullOrWhiteSpace(invLine.BuyerOrderLineId) ? null : new IDType { Value = invLine.BuyerOrderLineId },
                            FormattedIssueDateTime = invLine.BuyerOrderDate == null ? null : new FormattedDateTimeType { DateTimeString = new FormattedDateTimeTypeDateTimeString { format = "102", Value = invLine.BuyerOrderDate.Value.ToString("yyyyMMdd") } }
                        },
                GrossPriceProductTradePrice = omitGrossPrice ? null : new TradePriceType { ChargeAmount = new AmountType { Value = chargeAmount }, BasisQuantity = grossBasisQuantity },
                NetPriceProductTradePrice = new TradePriceType { ChargeAmount = new AmountType { Value = chargeAmount }, BasisQuantity = netBasisQuantity }
            },

            SpecifiedLineTradeDelivery = new LineTradeDeliveryType
            {
                BilledQuantity = new QuantityType { unitCode = invLine.UnitCode, Value = invLine.Quantity },
                ShipToTradeParty = MapParty(inv.Buyer),
                DeliveryNoteReferencedDocument =
                    string.IsNullOrWhiteSpace(invLine.DeliveryNoteNumber) && string.IsNullOrWhiteSpace(invLine.DeliveryNoteLineId) && invLine.DeliveryNoteDate == null
                        ? null
                        : new ReferencedDocumentType
                        {
                            IssuerAssignedID = string.IsNullOrWhiteSpace(invLine.DeliveryNoteNumber) ? null : new IDType { Value = invLine.DeliveryNoteNumber },
                            LineID = string.IsNullOrWhiteSpace(invLine.DeliveryNoteLineId) ? null : new IDType { Value = invLine.DeliveryNoteLineId },
                            FormattedIssueDateTime = invLine.DeliveryNoteDate == null ? null : new FormattedDateTimeType { DateTimeString = new FormattedDateTimeTypeDateTimeString { format = "102", Value = invLine.DeliveryNoteDate.Value.ToString("yyyyMMdd") } }
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
                        BasisAmount = omitLineTaxAmounts ? null : null,
                        CalculatedAmount = omitLineTaxAmounts ? null : null,
                        RateApplicablePercent = new PercentType { Value = invLine.TaxPercent }
                    }
                },
                BillingSpecifiedPeriod = invLine.BillingPeriodEndDate == null ? null : new SpecifiedPeriodType { EndDateTime = new DateTimeType { Item = new DateTimeTypeDateTimeString { format = "102", Value = invLine.BillingPeriodEndDate.Value.ToString("yyyyMMdd") } } },
                SpecifiedTradeSettlementLineMonetarySummation = new TradeSettlementLineMonetarySummationType
                {
                    LineTotalAmount = new AmountType { Value = lineNet },
                    TotalAllowanceChargeAmount = new AmountType { Value = 0m }
                },
                AdditionalReferencedDocument =
                    string.IsNullOrWhiteSpace(invLine.AdditionalReferencedDocumentId) &&
                    string.IsNullOrWhiteSpace(invLine.AdditionalReferencedDocumentTypeCode) &&
                    string.IsNullOrWhiteSpace(invLine.AdditionalReferencedDocumentReferenceTypeCode)
                        ? null
                        : new[]
                        {
                            new ReferencedDocumentType
                            {
                                IssuerAssignedID = string.IsNullOrWhiteSpace(invLine.AdditionalReferencedDocumentId) ? null : new IDType { Value = invLine.AdditionalReferencedDocumentId },
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

        decimal LineNet(InvoiceLine l) => l?.ForcedLineTotalAmount ?? Math.Round((l?.Quantity ?? 0m) * (l?.UnitPrice ?? 0m), 2, MidpointRounding.AwayFromZero);
        bool CountsTowardsHeaderTotals(InvoiceLine l) => !string.Equals(l?.LineStatusReasonCode, "GROUP", StringComparison.OrdinalIgnoreCase);

        var lines = (inv.Lines ?? []).Where(CountsTowardsHeaderTotals).ToArray();
        var netTotal = lines.Sum(LineNet);
        var headerTaxes = MapHeaderTradeTax(inv, currency);
        var chargeTotal = inv.HeaderChargeTotalAmount;
        var allowanceTotal = inv.HeaderAllowanceTotalAmount;
        var taxTotal = headerTaxes?.Sum(t => t.CalculatedAmount.Value) ?? 0m;
        var grandTotal = netTotal + taxTotal;
        var prepaidTotal = inv.HeaderTotalPrepaidAmount;
        var duePayable = inv.HeaderDuePayableAmount;

        var settlement = new HeaderTradeSettlementType
        {
            PaymentReference = string.IsNullOrWhiteSpace(inv.Payment?.PaymentReference) ? null : new TextType { Value = inv.Payment.PaymentReference },
            InvoiceCurrencyCode = new CurrencyCodeType { Value = currency },
            ApplicableTradeTax = headerTaxes,
            SpecifiedTradeSettlementHeaderMonetarySummation =
                new TradeSettlementHeaderMonetarySummationType
                {
                    LineTotalAmount = new AmountType { Value = netTotal },
                    ChargeTotalAmount = new AmountType { Value = chargeTotal },
                    AllowanceTotalAmount = new AmountType { Value = allowanceTotal },
                    TaxBasisTotalAmount = new AmountType { Value = netTotal },
                    TaxTotalAmount = new[] { new AmountType { currencyID = currency, Value = taxTotal } },
                    GrandTotalAmount = new AmountType { Value = grandTotal },
                    TotalPrepaidAmount = new AmountType { Value = prepaidTotal },
                    DuePayableAmount = new AmountType { Value = duePayable }
                }
        };

        // Payment data
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
            // 58 = SEPA direct debit, 31 = bank transfer
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
                    BasisDateTime = t.DiscountTerms.BasisDate == default ? null : MakeDate(t.DiscountTerms.BasisDate),
                    BasisPeriodMeasure = t.DiscountTerms.BasisPeriodDays <= 0 ? null : new MeasureType { unitCode = "DAY", Value = t.DiscountTerms.BasisPeriodDays },
                    BasisAmount = t.DiscountTerms.BasisAmount == 0m ? null : new AmountType { Value = t.DiscountTerms.BasisAmount },
                    CalculationPercent = t.DiscountTerms.CalculationPercent == 0m ? null : new PercentType { Value = t.DiscountTerms.CalculationPercent },
                    ActualDiscountAmount = t.DiscountTerms.ActualDiscountAmount == 0m ? null : new AmountType { Value = t.DiscountTerms.ActualDiscountAmount }
                }
            }).ToArray();
        }

        // Fallback
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
            GuidelineSpecifiedDocumentContextParameter = new DocumentContextParameterType { ID = new IDType { Value = "urn:cen.eu:en16931:2017#conformant#urn:factur-x.eu:1p0:extended" } }
        };
    }

    private HeaderTradeDeliveryType MapHeaderDelivery(Invoice inv)
    {
        var delivery = new HeaderTradeDeliveryType
        {
            ShipToTradeParty = MapParty(inv.Buyer),
            ShipFromTradeParty = MapParty(inv.Seller),
            ActualDeliverySupplyChainEvent = new SupplyChainEventType { OccurrenceDateTime = new DateTimeType { Item = new DateTimeTypeDateTimeString { format = "102", Value = inv.InvoiceDate.ToString("yyyyMMdd") } } }
        };

        return delivery;
    }

    private TradeTaxType[] MapHeaderTradeTax(Invoice inv, string currency)
    {
        if (inv.Lines == null || inv.Lines.Count == 0)
            return null!;

        // Same rounding logic as header totals
        decimal LineNet(InvoiceLine l) => l?.ForcedLineTotalAmount ?? Math.Round((l?.Quantity ?? 0m) * (l?.UnitPrice ?? 0m), 2, MidpointRounding.AwayFromZero);
        bool CountsTowardsHeaderTotals(InvoiceLine l) => !string.Equals(l?.LineStatusReasonCode, "GROUP", StringComparison.OrdinalIgnoreCase);

        var relevantLines = inv.Lines.Where(CountsTowardsHeaderTotals).ToArray();
        if (relevantLines.Length == 0)
            return null!;

        var groups = relevantLines.GroupBy(l => new { l.TaxPercent, l.TaxCategory });

        var result = new List<TradeTaxType>();

        foreach (var g in groups)
        {
            var basisAmount = g.Sum(LineNet);
            var taxAmount = Math.Round(basisAmount * g.Key.TaxPercent / 100m, 2, MidpointRounding.AwayFromZero);

            result.Add(new TradeTaxType
            {
                TypeCode = new TaxTypeCodeType { Value = "VAT" },
                CategoryCode = new TaxCategoryCodeType { Value = g.Key.TaxCategory },
                BasisAmount = new AmountType { Value = basisAmount },
                CalculatedAmount = new AmountType { Value = taxAmount },
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

    private static NoteType[]? MapLineNotes(InvoiceLine line)
    {
        if (line.Notes == null || line.Notes.Count == 0)
            return null;

        return
        [
            .. line.Notes
            .Where(n => !string.IsNullOrWhiteSpace(n.Content))
            .Select(n => new NoteType
            {
                Content = new TextType { Value = n.Content },
                SubjectCode = string.IsNullOrWhiteSpace(n.SubjectCode) ? null : new CodeType { Value = n.SubjectCode },
                ContentCode = string.IsNullOrWhiteSpace(n.ContentCode) ? null : new CodeType { Value = n.ContentCode }
            })
        ];
    }

    private static decimal ComputeLineNet(InvoiceLine line)
    {
        return Math.Round(line.Quantity * line.UnitPrice, 2, MidpointRounding.AwayFromZero);
    }
}