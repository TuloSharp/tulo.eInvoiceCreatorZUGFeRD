using tulo.eInvoiceXmlGeneratorCii.Models;
using System.Globalization;
using Zugferd24.Extended;

namespace tulo.eInvoiceXmlGeneratorCii.Mappers;

public class CiiMapper : ICiiMapper
{
    // ── Constants ────────────────────────────────────────────────────────────
    private static class C
    {
        public const string TaxTypeVat = "VAT";
        public const string DateFormat102 = "102";
        public const string DefaultCurrency = "EUR";
        public const string DefaultTypeCode = "380";
        public const string TaxSchemeVat = "VA";
        public const string TaxSchemeFc = "FC";
        public const string EmailScheme = "EM";
        public const string DayUnit = "DAY";
        public const string LeitwegScheme = "0204";
    }

    // ── Inline helpers ───────────────────────────────────────────────────────
    private static bool Has(string? s) => !string.IsNullOrWhiteSpace(s);
    private static decimal R2(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);

    private static IDType MakeId(string v, string? scheme = null) => new() { Value = v, schemeID = scheme };
    private static TextType MakeText(string v) => new() { Value = v };
    private static CodeType MakeCode(string v) => new() { Value = v };
    private static AmountType MakeAmt(decimal v) => new() { Value = v };
    private static PercentType MakePct(decimal v) => new() { Value = v };

    private static DateTimeType MakeDate(DateTime dt) => new()
    {
        Item = new DateTimeTypeDateTimeString { format = C.DateFormat102, Value = dt.ToString("yyyyMMdd") }
    };

    private static FormattedDateTimeType MakeFmtDate(DateTime dt) => new()
    {
        DateTimeString = new FormattedDateTimeTypeDateTimeString { format = C.DateFormat102, Value = dt.ToString("yyyyMMdd") }
    };

    private static ReferencedDocumentType? MakeRefDoc(string? id) =>
        Has(id) ? new ReferencedDocumentType { IssuerAssignedID = MakeId(id!) } : null;

    // ── Map (entry point) ────────────────────────────────────────────────────
    public CrossIndustryInvoiceType Map(Invoice inv)
    {
        if (inv == null) return new CrossIndustryInvoiceType();

        var currency = Has(inv.Currency) ? inv.Currency : C.DefaultCurrency;
        var typeCode = Has(inv.DocumentTypeCode) ? inv.DocumentTypeCode : C.DefaultTypeCode;

        return new CrossIndustryInvoiceType
        {
            ExchangedDocumentContext = new ExchangedDocumentContextType
            {
                GuidelineSpecifiedDocumentContextParameter = new DocumentContextParameterType
                {
                    ID = MakeId("urn:cen.eu:en16931:2017#conformant#urn:factur-x.eu:1p0:extended")
                }
            },
            ExchangedDocument = new ExchangedDocumentType
            {
                ID = MakeId(inv.InvoiceNumber),
                Name = Has(inv.DocumentName) ? MakeText(inv.DocumentName!) : null,
                TypeCode = new DocumentCodeType { Value = typeCode },
                IssueDateTime = MakeDate(inv.InvoiceDate),
                IncludedNote = MapNotes(inv)
            },
            SupplyChainTradeTransaction = new SupplyChainTradeTransactionType
            {
                ApplicableHeaderTradeAgreement = new HeaderTradeAgreementType
                {
                    BuyerReference = Has(inv.BuyerReference) ? MakeText(inv.BuyerReference!) : null,
                    SellerTradeParty = MapParty(inv.Seller),
                    BuyerTradeParty = MapParty(inv.Buyer),
                    SellerOrderReferencedDocument = MakeRefDoc(inv.SellerOrderReferencedId),
                    BuyerOrderReferencedDocument = MakeRefDoc(inv.BuyerOrderReferencedId),
                    ContractReferencedDocument = MakeRefDoc(inv.ContractReferencedId)
                },
                ApplicableHeaderTradeDelivery = MapHeaderDelivery(inv),
                ApplicableHeaderTradeSettlement = MapHeaderSettlement(inv),
                IncludedSupplyChainTradeLineItem = inv.Lines == null ? []
                    : [.. inv.Lines.Select((l, i) => MapLine(inv, l, i + 1, currency!)).Where(x => x != null)]
            }
        };
    }

    // ── Party ────────────────────────────────────────────────────────────────
    private TradePartyType MapParty(Party p)
    {
        if (p == null) return null!;

        var party = new TradePartyType
        {
            Name = Has(p.Name) ? MakeText(p.Name!) : null,
            PostalTradeAddress = BuildAddress(p)
        };

        if (Has(p.ID))
            party.ID = [MakeId(p.ID!)];

        if (Has(p.ContactPersonName) || Has(p.ContactPhone) || Has(p.ContactEmail))
            party.DefinedTradeContact = [BuildContact(p)];

        if (Has(p.GeneralEmail))
            party.URIUniversalCommunication = new UniversalCommunicationType
            { URIID = MakeId(p.GeneralEmail!, C.EmailScheme) };
        else if (Has(p.LeitwegId))
            party.URIUniversalCommunication = new UniversalCommunicationType
            { URIID = MakeId(p.LeitwegId!, Has(p.LeitwegIdSchemeId) ? p.LeitwegIdSchemeId : C.LeitwegScheme) };

        var legalId = Has(p.LegalOrganizationId) ? p.LegalOrganizationId : p.FiscalId;
        if (Has(legalId))
            party.SpecifiedLegalOrganization = new LegalOrganizationType
            { ID = MakeId(legalId!, Has(p.IdSchemeId) ? p.IdSchemeId : null) };

        var taxes = new List<TaxRegistrationType>();
        if (Has(p.VatId)) taxes.Add(new TaxRegistrationType { ID = MakeId(p.VatId!, C.TaxSchemeVat) });
        if (Has(p.TaxRegistrationFcId)) taxes.Add(new TaxRegistrationType { ID = MakeId(p.TaxRegistrationFcId!, C.TaxSchemeFc) });
        if (taxes.Count > 0)
            party.SpecifiedTaxRegistration = [.. taxes];

        if (Has(p.GlobalId))
            party.GlobalID = [MakeId(p.GlobalId!, Has(p.GlobalIdSchemeId) ? p.GlobalIdSchemeId : null)];

        return party;
    }

    private static TradeAddressType? BuildAddress(Party p)
    {
        if (!Has(p.Street) && !Has(p.Zip) && !Has(p.City) && !Has(p.CountryCode))
            return null;

        return new TradeAddressType
        {
            LineOne = Has(p.Street) ? MakeText(p.Street!) : null,
            PostcodeCode = Has(p.Zip) ? MakeCode(p.Zip!) : null,
            CityName = Has(p.City) ? MakeText(p.City!) : null,
            CountryID = Has(p.CountryCode) ? new CountryIDType { Value = p.CountryCode } : null
        };
    }

    private static TradeContactType BuildContact(Party p) => new()
    {
        PersonName = Has(p.ContactPersonName) ? MakeText(p.ContactPersonName!) : null,
        TelephoneUniversalCommunication = Has(p.ContactPhone)
            ? new UniversalCommunicationType { CompleteNumber = MakeText(p.ContactPhone!) } : null,
        EmailURIUniversalCommunication = Has(p.ContactEmail)
            ? new UniversalCommunicationType { URIID = MakeId(p.ContactEmail!) } : null
    };

    // ── Line ─────────────────────────────────────────────────────────────────
    private SupplyChainTradeLineItemType MapLine(Invoice inv, InvoiceLine invLine, int lineNo, string currency)
    {
        if (invLine == null) return null!;

        var lineNet = invLine.ForcedLineTotalAmount ?? ComputeLineNet(invLine);
        var priceBasisQty = invLine.PriceBasisQuantity.GetValueOrDefault(1m);
        var isChildOrDetail = Has(invLine.ParentLineId) || IsStatus(invLine, "DETAIL");
        var omitGrossPrice = isChildOrDetail || IsStatus(invLine, "GROUP");
        var omitNetBasisQty = invLine.OmitNetPriceBasisQuantity ?? isChildOrDetail;
        var chargeAmount = R2(invLine.UnitPrice * priceBasisQty);

        return new SupplyChainTradeLineItemType
        {
            AssociatedDocumentLineDocument = BuildLineDocument(invLine, lineNo),
            SpecifiedTradeProduct = BuildProduct(invLine),
            SpecifiedLineTradeAgreement = BuildLineAgreement(invLine, omitGrossPrice, omitNetBasisQty, chargeAmount, priceBasisQty),
            SpecifiedLineTradeDelivery = BuildLineDelivery(inv, invLine),
            SpecifiedLineTradeSettlement = BuildLineSettlement(invLine, lineNet, omitGrossPrice)
        };
    }

    private static DocumentLineDocumentType BuildLineDocument(InvoiceLine invLine, int lineNo) => new()
    {
        LineID = MakeId(Has(invLine.LineId) ? invLine.LineId! : lineNo.ToString(CultureInfo.InvariantCulture)),
        ParentLineID = Has(invLine.ParentLineId) ? MakeId(invLine.ParentLineId!) : null,
        LineStatusReasonCode = Has(invLine.LineStatusReasonCode) ? MakeCode(invLine.LineStatusReasonCode!) : null,
        IncludedNote = MapLineNotes(invLine)
    };

    private static TradeProductType BuildProduct(InvoiceLine invLine) => new()
    {
        GlobalID = Has(invLine.GlobalId)
            ? MakeId(invLine.GlobalId!, Has(invLine.GlobalIdSchemeId) ? invLine.GlobalIdSchemeId : null) : null,
        SellerAssignedID = Has(invLine.SellerAssignedId) ? MakeId(invLine.SellerAssignedId!) : null,
        BuyerAssignedID = Has(invLine.BuyerAssignedId) ? MakeId(invLine.BuyerAssignedId!) : null,
        Name = Has(invLine.Description) ? MakeText(invLine.Description!) : null,
        Description = Has(invLine.ProductDescription) ? MakeText(invLine.ProductDescription!) : null,
        OriginTradeCountry = Has(invLine.OriginCountryCode)
            ? new TradeCountryType { ID = new CountryIDType { Value = invLine.OriginCountryCode } } : null
    };

    private static LineTradeAgreementType BuildLineAgreement(
        InvoiceLine invLine, bool omitGrossPrice, bool omitNetBasisQty,
        decimal chargeAmount, decimal priceBasisQty)
    {
        QuantityType? GrossQty() => omitGrossPrice ? null : new QuantityType { unitCode = invLine.UnitCode, Value = priceBasisQty };
        QuantityType? NetQty() => omitNetBasisQty ? null : new QuantityType { unitCode = invLine.UnitCode, Value = priceBasisQty };

        ReferencedDocumentType? buyerOrderDoc = null;
        if (Has(invLine.BuyerOrderReferencedId) || Has(invLine.BuyerOrderLineId) || invLine.BuyerOrderDate != null)
            buyerOrderDoc = new ReferencedDocumentType
            {
                IssuerAssignedID = Has(invLine.BuyerOrderReferencedId) ? MakeId(invLine.BuyerOrderReferencedId!) : null,
                LineID = Has(invLine.BuyerOrderLineId) ? MakeId(invLine.BuyerOrderLineId!) : null,
                FormattedIssueDateTime = invLine.BuyerOrderDate != null ? MakeFmtDate(invLine.BuyerOrderDate.Value) : null
            };

        return new LineTradeAgreementType
        {
            BuyerOrderReferencedDocument = buyerOrderDoc,
            GrossPriceProductTradePrice = omitGrossPrice ? null
                : new TradePriceType { ChargeAmount = MakeAmt(chargeAmount), BasisQuantity = GrossQty() },
            NetPriceProductTradePrice = new TradePriceType { ChargeAmount = MakeAmt(chargeAmount), BasisQuantity = NetQty() }
        };
    }

    private LineTradeDeliveryType BuildLineDelivery(Invoice inv, InvoiceLine invLine)
    {
        ReferencedDocumentType? deliveryDoc = null;
        if (Has(invLine.DeliveryNoteNumber) || Has(invLine.DeliveryNoteLineId) || invLine.DeliveryNoteDate != null)
            deliveryDoc = new ReferencedDocumentType
            {
                IssuerAssignedID = Has(invLine.DeliveryNoteNumber) ? MakeId(invLine.DeliveryNoteNumber!) : null,
                LineID = Has(invLine.DeliveryNoteLineId) ? MakeId(invLine.DeliveryNoteLineId!) : null,
                FormattedIssueDateTime = invLine.DeliveryNoteDate != null ? MakeFmtDate(invLine.DeliveryNoteDate.Value) : null
            };

        return new LineTradeDeliveryType
        {
            BilledQuantity = new QuantityType { unitCode = invLine.UnitCode, Value = invLine.Quantity },
            ShipToTradeParty = MapParty(inv.Buyer),
            DeliveryNoteReferencedDocument = deliveryDoc
        };
    }

    private static LineTradeSettlementType BuildLineSettlement(InvoiceLine invLine, decimal lineNet, bool omitLineTaxAmts) => new()
    {
        ApplicableTradeTax =
        [
            new TradeTaxType
            {
                TypeCode              = new TaxTypeCodeType    { Value = C.TaxTypeVat },
                CategoryCode          = new TaxCategoryCodeType { Value = invLine.TaxCategory },
                BasisAmount           = omitLineTaxAmts ? null : null,
                CalculatedAmount      = omitLineTaxAmts ? null : null,
                RateApplicablePercent = MakePct(invLine.TaxPercent)
            }
        ],
        BillingSpecifiedPeriod = invLine.BillingPeriodEndDate == null ? null
            : new SpecifiedPeriodType { EndDateTime = MakeDate(invLine.BillingPeriodEndDate.Value) },
        SpecifiedTradeSettlementLineMonetarySummation = new TradeSettlementLineMonetarySummationType
        {
            LineTotalAmount = MakeAmt(lineNet),
            TotalAllowanceChargeAmount = MakeAmt(0m)
        },
        AdditionalReferencedDocument = BuildAdditionalRefDoc(invLine)
    };

    private static ReferencedDocumentType[]? BuildAdditionalRefDoc(InvoiceLine invLine)
    {
        if (!Has(invLine.AdditionalReferencedDocumentId)
            && !Has(invLine.AdditionalReferencedDocumentTypeCode)
            && !Has(invLine.AdditionalReferencedDocumentReferenceTypeCode))
            return null;

        return
        [
            new ReferencedDocumentType
            {
                IssuerAssignedID  = Has(invLine.AdditionalReferencedDocumentId)
                    ? MakeId(invLine.AdditionalReferencedDocumentId!) : null,
                TypeCode          = Has(invLine.AdditionalReferencedDocumentTypeCode)
                    ? new DocumentCodeType  { Value = invLine.AdditionalReferencedDocumentTypeCode }          : null,
                ReferenceTypeCode = Has(invLine.AdditionalReferencedDocumentReferenceTypeCode)
                    ? new ReferenceCodeType { Value = invLine.AdditionalReferencedDocumentReferenceTypeCode } : null
            }
        ];
    }

    // ── Header delivery ──────────────────────────────────────────────────────
    private HeaderTradeDeliveryType MapHeaderDelivery(Invoice inv) => new()
    {
        ShipToTradeParty = MapParty(inv.Buyer),
        ShipFromTradeParty = MapParty(inv.Seller),
        ActualDeliverySupplyChainEvent = new SupplyChainEventType
        { OccurrenceDateTime = MakeDate(inv.InvoiceDate) }
    };

    // ── Settlement ───────────────────────────────────────────────────────────
    private HeaderTradeSettlementType MapHeaderSettlement(Invoice inv)
    {
        var currency = Has(inv.Currency) ? inv.Currency : C.DefaultCurrency;
        var lines = (inv.Lines ?? []).Where(l => !IsStatus(l, "GROUP")).ToArray();
        var netTotal = R2(lines.Sum(l => l?.ForcedLineTotalAmount ?? R2((l?.Quantity ?? 0m) * (l?.UnitPrice ?? 0m))));
        var chargeTotal = R2(inv.HeaderChargeTotalAmount);
        var allowanceTotal = R2(inv.HeaderAllowanceTotalAmount);
        var prepaidTotal = R2(inv.HeaderTotalPrepaidAmount);

        var headerTaxes = MapHeaderTradeTax(inv, currency!);
        var taxTotal = R2(headerTaxes?.Sum(t => t?.CalculatedAmount?.Value ?? 0m) ?? 0m);
        var taxBasisTotal = R2(netTotal + chargeTotal - allowanceTotal);
        var grandTotal = R2(taxBasisTotal + taxTotal);
        var duePayable = R2(grandTotal - prepaidTotal);

        var settlement = new HeaderTradeSettlementType
        {
            PaymentReference = Has(inv.Payment?.PaymentReference) ? MakeText(inv.Payment!.PaymentReference!) : null,
            InvoiceCurrencyCode = new CurrencyCodeType { Value = currency },
            ApplicableTradeTax = headerTaxes,
            SpecifiedTradeSettlementHeaderMonetarySummation = new TradeSettlementHeaderMonetarySummationType
            {
                LineTotalAmount = MakeAmt(netTotal),
                ChargeTotalAmount = MakeAmt(chargeTotal),
                AllowanceTotalAmount = MakeAmt(allowanceTotal),
                TaxBasisTotalAmount = MakeAmt(taxBasisTotal),
                TaxTotalAmount = [new AmountType { currencyID = currency, Value = taxTotal }],
                GrandTotalAmount = MakeAmt(grandTotal),
                TotalPrepaidAmount = MakeAmt(prepaidTotal),
                DuePayableAmount = MakeAmt(duePayable)
            }
        };

        if (inv.Payment != null)
        {
            settlement.SpecifiedTradeSettlementPaymentMeans = MapPaymentMeans(inv.Payment, currency!);
            settlement.SpecifiedTradePaymentTerms = MapPaymentTerms(inv.Payment);
        }

        return settlement;
    }

    // ── Header trade tax ─────────────────────────────────────────────────────
    private TradeTaxType[] MapHeaderTradeTax(Invoice inv, string currency)
    {
        if (inv.Lines == null || inv.Lines.Count == 0) return null!;

        static decimal LineNet(InvoiceLine l) =>
            l?.ForcedLineTotalAmount ?? R2((l?.Quantity ?? 0m) * (l?.UnitPrice ?? 0m));

        var relevant = inv.Lines.Where(l => !IsStatus(l, "GROUP")).ToArray();
        if (relevant.Length == 0) return null!;

        var totalLineNet = relevant.Sum(LineNet);
        var totalAdjustment = R2(inv.HeaderChargeTotalAmount) - R2(inv.HeaderAllowanceTotalAmount);

        return relevant
            .GroupBy(l => new { l.TaxPercent, l.TaxCategory })
            .Select(g =>
            {
                var groupNet = g.Sum(LineNet);
                var adjustedBasis = ComputeAdjustedGroupBasis(groupNet, totalLineNet, totalAdjustment);
                var tax = R2(adjustedBasis * g.Key.TaxPercent / 100m);

                return new TradeTaxType
                {
                    TypeCode = new TaxTypeCodeType { Value = C.TaxTypeVat },
                    CategoryCode = new TaxCategoryCodeType { Value = g.Key.TaxCategory },
                    BasisAmount = MakeAmt(adjustedBasis),
                    CalculatedAmount = MakeAmt(tax),
                    RateApplicablePercent = MakePct(g.Key.TaxPercent)
                };
            })
            .ToArray();
    }

    /// <summary>
    /// EN16931: distributes header-level allowance/charge proportionally
    /// across tax groups so that per-group BasisAmount and CalculatedAmount
    /// are consistent with the overall TaxBasisTotalAmount and TaxTotal.
    /// </summary>
    private static decimal ComputeAdjustedGroupBasis(
        decimal groupNet, decimal totalLineNet, decimal totalAdjustment)
    {
        var proportion = totalLineNet != 0m ? groupNet / totalLineNet : 0m;
        return R2(groupNet + totalAdjustment * proportion);
    }

    // ── Payment means ────────────────────────────────────────────────────────
    private TradeSettlementPaymentMeansType[] MapPaymentMeans(PaymentDetails payment, string currency)
    {
        if (payment == null) return null!;

        var hasAnything = Has(payment.PaymentMeansTypeCode) || Has(payment.PaymentMeansInformation)
                       || Has(payment.Iban) || Has(payment.Bic) || Has(payment.AccountName);
        if (!hasAnything) return null!;

        return
        [
            new TradeSettlementPaymentMeansType
            {
                TypeCode    = Has(payment.PaymentMeansTypeCode)
                    ? new PaymentMeansCodeType { Value = payment.PaymentMeansTypeCode } : null,
                Information = Has(payment.PaymentMeansInformation)
                    ? MakeText(payment.PaymentMeansInformation!) : null,
                PayeePartyCreditorFinancialAccount = (Has(payment.Iban) || Has(payment.AccountName))
                    ? new CreditorFinancialAccountType
                    {
                        IBANID      = Has(payment.Iban)         ? MakeId(payment.Iban!)         : null,
                        AccountName = Has(payment.AccountName)  ? MakeText(payment.AccountName!) : null
                    } : null,
                PayeeSpecifiedCreditorFinancialInstitution = Has(payment.Bic)
                    ? new CreditorFinancialInstitutionType { BICID = MakeId(payment.Bic!) } : null
            }
        ];
    }

    // ── Payment terms ────────────────────────────────────────────────────────
    private TradePaymentTermsType[] MapPaymentTerms(PaymentDetails payment)
    {
        if (payment == null) return null!;

        var hasStructured = payment.Terms?.Count > 0;
        var hasLegacy = Has(payment.PaymentTermsText) || payment.DueDate != null || Has(payment.DirectDebitMandateId);

        if (!hasStructured && !hasLegacy) return null!;

        if (hasStructured)
            return payment.Terms!.Select(t => new TradePaymentTermsType
            {
                Description = Has(t.Description) ? MakeText(t.Description!) : null,
                DueDateDateTime = t.DueDate != null ? MakeDate(t.DueDate.Value) : null,
                ApplicableTradePaymentDiscountTerms = t.DiscountTerms == null ? null
                    : new TradePaymentDiscountTermsType
                    {
                        BasisDateTime = t.DiscountTerms.BasisDate == default ? null : MakeDate(t.DiscountTerms.BasisDate),
                        BasisPeriodMeasure = t.DiscountTerms.BasisPeriodDays <= 0 ? null : new MeasureType { unitCode = C.DayUnit, Value = t.DiscountTerms.BasisPeriodDays },
                        BasisAmount = t.DiscountTerms.BasisAmount == 0m ? null : MakeAmt(t.DiscountTerms.BasisAmount),
                        CalculationPercent = t.DiscountTerms.CalculationPercent == 0m ? null : MakePct(t.DiscountTerms.CalculationPercent),
                        ActualDiscountAmount = t.DiscountTerms.ActualDiscountAmount == 0m ? null : MakeAmt(t.DiscountTerms.ActualDiscountAmount)
                    }
            }).ToArray();

        return
        [
            new TradePaymentTermsType
            {
                Description          = Has(payment.PaymentTermsText)    ? MakeText(payment.PaymentTermsText!)   : null,
                DueDateDateTime      = payment.DueDate != null           ? MakeDate(payment.DueDate.Value)        : null,
                DirectDebitMandateID = Has(payment.DirectDebitMandateId) ? MakeId(payment.DirectDebitMandateId!) : null
            }
        ];
    }

    // ── Notes ────────────────────────────────────────────────────────────────
    private NoteType[] MapNotes(Invoice inv)
    {
        if (inv.Notes == null || inv.Notes.Count == 0) return null!;
        return [.. inv.Notes.Where(n => Has(n.Content)).Select(MapNote)];
    }

    private static NoteType[]? MapLineNotes(InvoiceLine line)
    {
        if (line.Notes == null || line.Notes.Count == 0) return null;
        return [.. line.Notes.Where(n => Has(n.Content)).Select(MapNote)];
    }

    private static NoteType MapNote(InvoiceNote n) => new()
    {
        Content = MakeText(n.Content!),
        SubjectCode = Has(n.SubjectCode) ? MakeCode(n.SubjectCode!) : null,
        ContentCode = Has(n.ContentCode) ? MakeCode(n.ContentCode!) : null
    };

    // ── Utilities ────────────────────────────────────────────────────────────
    private static bool IsStatus(InvoiceLine? l, string code) =>
        string.Equals(l?.LineStatusReasonCode, code, StringComparison.OrdinalIgnoreCase);

    private static decimal ComputeLineNet(InvoiceLine line) =>
        R2(line.Quantity * line.UnitPrice);
}
