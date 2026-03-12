using tulo.XMLeInvoiceToPdf.Languages;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace tulo.XMLeInvoiceToPdf.Services;

public class PdfGeneratorFromInvoiceCii(ITranslatorProvider translationProvider) : PdfGeneratorFromInvoiceBase(translationProvider), IPdfGeneratorFromInvoice
{
    private readonly ITranslatorProvider _translationProvider = translationProvider;
    public string Name => "CII";

    protected override void SetupNamespaces()
    {
        nsmgr!.AddNamespace("rsm", "urn:un:unece:uncefact:data:standard:CrossIndustryInvoice:100");
        nsmgr.AddNamespace("ram", "urn:un:unece:uncefact:data:standard:ReusableAggregateBusinessInformationEntity:100");
        nsmgr.AddNamespace("udt", "urn:un:unece:uncefact:data:standard:UnqualifiedDataType:100");
        nsmgr.AddNamespace("qdt", "urn:un:unece:uncefact:data:standard:QualifiedDataType:100");
    }

    #region CII Invoice File
    public string GeneratePdfFile(string pdfPath, string xmlInvoiceFileName, string xmlInvoiceContent, bool hasToRenderHeader)
    {
        LoadXmlInvoiceContent(xmlInvoiceContent);
        using (PdfDocument pdfDoc = new PdfDocument())
        {
            CreatePdfContent(xmlInvoiceFileName, xmlInvoiceContent, hasToRenderHeader, pdfDoc);
            ApplyFooterToAllPages(pdfDoc);
            pdfDoc.Save(pdfPath);
        }
        return pdfPath;
    }
    #endregion

    #region CII Invoice Stream
    public MemoryStream GeneratePdfStream(string xmlInvoiceFileName, string xmlInvoiceContent, bool hasToRenderHeader)
    {
        LoadXmlInvoiceContent(xmlInvoiceContent);
        MemoryStream pdfStream = new MemoryStream();
        using (PdfDocument pdfDoc = new PdfDocument())
        {
            CreatePdfContent(xmlInvoiceFileName, xmlInvoiceContent, hasToRenderHeader, pdfDoc);
            ApplyFooterToAllPages(pdfDoc);
            pdfDoc.Save(pdfStream, false);
        }
        pdfStream.Position = 0;
        return pdfStream;
    }
    #endregion

    #region Create PDf Content
    private void CreatePdfContent(string xmlInvoiceFileName, string xmlInvoiceContent, bool hasToRenderHeader, PdfDocument pdfDoc)
    {
        int yPosition = 20;

        PdfPage pfdPage = pdfDoc.AddPage();
        pfdPage.Size = PageSize.A4;

        double pageWidth = pfdPage.Width.Point;
        double pageHeight = pfdPage.Height.Point;

        XGraphics xGraphics = XGraphics.FromPdfPage(pfdPage);

        try
        {
            #region section footer infos
            var notesText = ReadLineNotesAsText(invoiceDoc!, nsmgr!, "//rsm:ExchangedDocument/ram:IncludedNote", "./ram:Content", "./ram:SubjectCode", includeSubject: true, subjectFilter: new[] { "REG", "AAI", "PMT" });
            if (!string.IsNullOrWhiteSpace(notesText))
            {
                _currentFooterText = notesText;
                _currentFooterTitle = string.Empty;
            }
            #endregion

            #region Section Header
            var infoHeader = xmlInvoiceFileName;
            if (hasToRenderHeader)
            {
                DrawHeader(ref xGraphics, _translationProvider.Translate("PdfDisclamerText"), infoHeader, ref yPosition, pageWidth);
                yPosition += 16;
            }
            else
                yPosition += 26;
            #endregion

            #region Buyer, Seller & Invoice-Info Tables
            //build dictionary for icons
            var iconDictionary = new Dictionary<string, string>
                {
                    { "Name", "Company.svg" },
                    { "ContactName",  "Person.svg" },
                    { "Address" , "Address.svg" },
                    { "Phone" , "Phone.svg" },
                    { "Email" , "EmailAddress.svg" },
                    { "TaxId", "IdCard.svg"}
                };

            var vatIdLabel = _translationProvider.Translate("VatID", "VatID");
            var taxNumberLabel = _translationProvider.Translate("TaxNumber", "TaxNumber");

            var sellerTaxNrSchemeVA = taxNumberLabel + ":" + GetContentXNode(nsmgr!, "//ram:SellerTradeParty/ram:SpecifiedTaxRegistration[ram:ID[@schemeID='VA']]/ram:ID", invoiceDoc);
            var sellerTaxIDSchemeFC = vatIdLabel + ":" + GetContentXNode(nsmgr!, "//ram:SellerTradeParty/ram:SpecifiedTaxRegistration[ram:ID[@schemeID='FC']]/ram:ID", invoiceDoc);

            //// Informations seller
            var buildedSellerAddress = GetContentXNode(nsmgr!, "//ram:SellerTradeParty/ram:PostalTradeAddress/ram:LineOne", invoiceDoc) + ", " + GetContentXNode(nsmgr!, "//ram:SellerTradeParty/ram:PostalTradeAddress/ram:PostcodeCode", invoiceDoc)
               + " " + GetContentXNode(nsmgr!, "//ram:SellerTradeParty/ram:PostalTradeAddress/ram:CityName", invoiceDoc) + ", " + GetContentXNode(nsmgr!, "//ram:SellerTradeParty/ram:PostalTradeAddress/ram:CountryID", invoiceDoc);

            var buyerTaxNrSchemeVA = taxNumberLabel + ":" + GetContentXNode(nsmgr!, "//ram:BuyerTradeParty/ram:SpecifiedTaxRegistration[ram:ID[@schemeID='VA']]/ram:ID", invoiceDoc);
            var buyerTaxIDSchemeFC = vatIdLabel + ":" + GetContentXNode(nsmgr!, "//ram:BuyerTradeParty/ram:SpecifiedTaxRegistration[ram:ID[@schemeID='FC']]/ram:ID", invoiceDoc);

            //Information Buyer
            var buildedBuyerAddress = GetContentXNode(nsmgr!, "//ram:BuyerTradeParty/ram:PostalTradeAddress/ram:LineOne", invoiceDoc) + ", " + GetContentXNode(nsmgr!, "//ram:BuyerTradeParty/ram:PostalTradeAddress/ram:PostcodeCode", invoiceDoc)
                + " " + GetContentXNode(nsmgr!, "//ram:BuyerTradeParty/ram:PostalTradeAddress/ram:CityName", invoiceDoc) + ", " + GetContentXNode(nsmgr!, "//ram:BuyerTradeParty/ram:PostalTradeAddress/ram:CountryID", invoiceDoc);

            var sellerRows = new List<(string, string)>
            {
                ("//ram:SellerTradeParty/ram:Name", "SellerName"),
                (sellerTaxIDSchemeFC, "TaxId"),
                (sellerTaxNrSchemeVA, "TaxNr"),
                ("//ram:SellerTradeParty/ram:DefinedTradeContact/ram:PersonName", "SellerContactName"),
                (buildedSellerAddress, "Address"),
                ("//ram:SellerTradeParty/ram:DefinedTradeContact/ram:TelephoneUniversalCommunication/ram:CompleteNumber", "SellerPhone"),
                ("//ram:SellerTradeParty/ram:DefinedTradeContact/ram:EmailURIUniversalCommunication/ram:URIID", "SellerEmail"),
            };

            var buyerRows = new List<(string, string)>
            {
                ("//ram:BuyerTradeParty/ram:Name", "BuyerName"),
                (buyerTaxIDSchemeFC, "TaxId"),
                (buyerTaxNrSchemeVA, "TaxNr"),
                ("//ram:BuyerTradeParty/ram:DefinedTradeContact/ram:PersonName", "BuyerContactName"),
                (buildedBuyerAddress, "Address"),
                ("//ram:BuyerTradeParty/ram:DefinedTradeContact/ram:TelephoneUniversalCommunication/ram:CompleteNumber", "BuyerPhone"),
                ("//ram:BuyerTradeParty/ram:DefinedTradeContact/ram:EmailURIUniversalCommunication/ram:URIID", "BuyerEmail"),
            };

            var titleInvoiceTypeCode = GetTitleInvoiceTypeCode("//rsm:ExchangedDocument/ram:TypeCode");
            var invoiceFields = new Dictionary<string, string>
            {
                { "//rsm:CrossIndustryInvoice/rsm:ExchangedDocument/ram:ID", "InvoiceNr" },
                { "//rsm:CrossIndustryInvoice/rsm:ExchangedDocument/ram:IssueDateTime/udt:DateTimeString", "InvoiceIssueDate" },
                { "//rsm:CrossIndustryInvoice/rsm:ExchangedDocument/ram:TypeCode", "InvoiceTypeCode" },
                { "//ram:BuyerTradeParty/ram:ID", "BuyerRefId" }
            };

            CreateBuyerInvocieDataSellerBlock(pdfDoc, ref pfdPage, ref xGraphics, invoiceDoc, nsmgr!, _translationProvider, sellerRows, buyerRows, _translationProvider.Translate(titleInvoiceTypeCode), invoiceFields, ref yPosition, fontTitleInfo, fontBody, iconDictionary, keepIconsForPhoneEmailOnly: true);
            yPosition += 8;
            #endregion

            #region occurrence date (delivery / billing period / due date)
            var result = string.Empty;

            // RAW values
            var occurrenceRaw = GetContentXNode(nsmgr!,"//ram:ApplicableHeaderTradeDelivery/ram:ActualDeliverySupplyChainEvent/ram:OccurrenceDateTime/udt:DateTimeString",invoiceDoc);
            var billingStartRaw = GetContentXNode(nsmgr!, "//ram:ApplicableHeaderTradeSettlement/ram:BillingSpecifiedPeriod/ram:StartDateTime/udt:DateTimeString", invoiceDoc);
            var billingEndRaw = GetContentXNode(nsmgr!, "//ram:ApplicableHeaderTradeSettlement/ram:BillingSpecifiedPeriod/ram:EndDateTime/udt:DateTimeString", invoiceDoc);
            var dueDateRaw = GetContentXNode(nsmgr!, "(//ram:ApplicableHeaderTradeSettlement/ram:SpecifiedTradePaymentTerms/ram:DueDateDateTime/udt:DateTimeString)[1]",invoiceDoc);

            string? occurrenceDate = !occurrenceRaw.Contains(ContentNotFound) ? _translationProvider.Translate("OccurrenceDateTime") + occurrenceRaw : null;

            string? billingPeriod = (!billingStartRaw.Contains(ContentNotFound) || !billingEndRaw.Contains(ContentNotFound))
                ? _translationProvider.Translate("BillingDateTimeFromTo")
                    .Replace("StartDate", billingStartRaw.Contains(ContentNotFound) ? "" : billingStartRaw)
                    .Replace("EndDate", billingEndRaw.Contains(ContentNotFound) ? "" : billingEndRaw)
                : null;

            string? dueDate = !dueDateRaw.Contains(ContentNotFound) ? _translationProvider.Translate("PaymentDueDate") + dueDateRaw : null;

            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(occurrenceDate)) parts.Add(occurrenceDate);
            if (!string.IsNullOrWhiteSpace(billingPeriod)) parts.Add(billingPeriod);
            if (!string.IsNullOrWhiteSpace(dueDate)) parts.Add(dueDate);

            result = parts.Count > 0 ? string.Join(" - ", parts) : string.Empty;
            
            if (!string.IsNullOrWhiteSpace(result))
                DrawLineText(ref xGraphics, result, ref yPosition, _blackBrushColor, TopLeftAlignment, true);
            yPosition += 1;
            #endregion

            #region positions table 
            //title for invoice table
            DrawTableTitle(ref xGraphics, _translationProvider.Translate("LabelInvoiceTable"), ref yPosition, TopLeftAlignment);
            //table “#”, “Item text”, “Quantity”, ‘Unit’, “Net unit price”, “Tax %”, “Net amount”, “Gross amount”
            CreateInvoicePositionsTable(pdfDoc, ref pfdPage, ref xGraphics, invoiceDoc, nsmgr!, _translationProvider, new Dictionary<string, string>
            {
                { ".//ram:AssociatedDocumentLineDocument/ram:LineID", "InvoiceTableNr" },
                { ".//ram:AssociatedDocumentLineDocument/ram:ParentLineID", "ParentLineId" },
                { ".//ram:SpecifiedTradeProduct/ram:Name", "InvoiceTablePosText" },
                { ".//ram:SpecifiedTradeProduct/ram:Description", "InvoiceTablePosText" },
                { ".//ram:SpecifiedTradeProduct/ram:SellerAssignedID", "InvoiceTablePosText" },
                { ".//ram:SpecifiedTradeProduct/ram:GlobalID", "InvoiceTablePosText" },
                { ".//ram:SpecifiedLineTradeDelivery/ram:BilledQuantity", "InvoiceTableQuantity" },
                { ".//ram:SpecifiedLineTradeDelivery/ram:BilledQuantity/@unitCode", "InvoiceTableUnit" },
                { ".//ram:SpecifiedLineTradeAgreement/ram:NetPriceProductTradePrice/ram:ChargeAmount", "InvoiceTableUnitNetPrice" },
                { ".//ram:SpecifiedLineTradeSettlement/ram:ApplicableTradeTax/ram:RateApplicablePercent", "InvoiceTableTaxPrecentage" },
                { ".//ram:SpecifiedLineTradeSettlement/ram:SpecifiedTradeSettlementLineMonetarySummation/ram:LineTotalAmount", "InvoiceTableNetAmount" }
            }, ref yPosition, fontColumnHeader, fontTableBody, "//ram:IncludedSupplyChainTradeLineItem");
            #endregion

            var currentCurrency = _translationProvider.Translate(GetContentXNode(nsmgr!, "//ram:ApplicableHeaderTradeSettlement/ram:InvoiceCurrencyCode", invoiceDoc));

            #region total amount table
            yPosition += 4;

            CreateInvoiceResultTable(pdfDoc, ref pfdPage, ref xGraphics, invoiceDoc, nsmgr!, _translationProvider, new Dictionary<string, string>
            {
                { "//ram:SpecifiedTradeSettlementHeaderMonetarySummation/ram:ChargeTotalAmount", "InvoiceChargeTotalAmount" },
                { "//ram:SpecifiedTradeSettlementHeaderMonetarySummation/ram:AllowanceTotalAmount", "InvoiceAllowanceTotalAmount" },
                { "//ram:SpecifiedTradeSettlementHeaderMonetarySummation/ram:LineTotalAmount", "InvoiceTotalAmountWithoutVat" },
                { "//ram:SpecifiedTradeSettlementHeaderMonetarySummation/ram:TaxTotalAmount", "InvoiceTotalVatAmount" },
                { "//ram:SpecifiedTradeSettlementHeaderMonetarySummation/ram:GrandTotalAmount", "SumInvoiceLineNetAmount" }
            }, ref yPosition, fontColumnHeader, fontTableBody, currentCurrency, "SumInvoiceLineNetAmount");
            #endregion

            #region Prepaid & Payable amount table ()
            CreateInvoiceResultTable(pdfDoc, ref pfdPage, ref xGraphics, invoiceDoc, nsmgr!, _translationProvider, new Dictionary<string, string>
            {
                { "//ram:SpecifiedTradeSettlementHeaderMonetarySummation/ram:TotalPrepaidAmount", "PaidAmount" },
                { "//ram:SpecifiedTradeSettlementHeaderMonetarySummation/ram:DuePayableAmount", "AmountDueForPayment" }
            }, ref yPosition, fontColumnHeader, fontTableBody, currentCurrency, "AmountDueForPayment");
            #endregion

            #region Payment data (Zahlungsdaten)
            var pmNode = GetContentXNode(nsmgr!, "//ram:SpecifiedTradeSettlementPaymentMeans/ram:TypeCode", invoiceDoc);

            XRect? qrRect = null;
            PdfPage? qrPage = null;

            if (pmNode == "58" || pmNode == "30")
            {
                CreateQrCode(pdfDoc, ref pfdPage, ref xGraphics, invoiceDoc, nsmgr!, _translationProvider,
                  [
                     "//ram:SpecifiedTradeSettlementPaymentMeans/ram:PayeePartyCreditorFinancialAccount/ram:IBANID" ,
                     "//ram:SpecifiedTradeSettlementPaymentMeans/ram:PayeeSpecifiedCreditorFinancialInstitution/ram:BICID" ,
                     "//ram:SellerTradeParty/ram:Name" ,
                     "//ram:ApplicableHeaderTradeSettlement/ram:InvoiceCurrencyCode" ,
                     "//ram:ApplicableHeaderTradeSettlement/ram:SpecifiedTradeSettlementHeaderMonetarySummation/ram:DuePayableAmount" ,
                     "//ram:ApplicableHeaderTradeSettlement/ram:PaymentReference"
                  ], ref yPosition, fontTextHeader, fontBody, out qrRect, out qrPage);
            }

            yPosition += 8;
            var descriptionPaymentTerms = GetContentXNodesAsText(nsmgr!, "//ram:SpecifiedTradePaymentTerms/ram:Description", invoiceDoc, separator: " - ");

            CreateContentTableWithLabel(pdfDoc, ref pfdPage, ref xGraphics, _translationProvider.Translate("OverviewPaymentsData"), invoiceDoc, nsmgr!, _translationProvider, new Dictionary<string, string>
            {
               { "//ram:SpecifiedTradeSettlementPaymentMeans/ram:PayeePartyCreditorFinancialAccount/ram:IBANID", "PaymentAccountId" },
               { "//ram:SpecifiedTradeSettlementPaymentMeans/ram:PayeeSpecifiedCreditorFinancialInstitution/ram:BICID", "PaymentServiceProviderId" },
               { "//rsm:CrossIndustryInvoice/rsm:SupplyChainTradeTransaction/ram:ApplicableHeaderTradeSettlement/ram:InvoiceCurrencyCode", "PaymentCurrencyCode" },
               { "//ram:ApplicableHeaderTradeSettlement/ram:PaymentReference", "RemittanceInfo" },
               { "//ram:SpecifiedTradeSettlementPaymentMeans/ram:TypeCode", "PaymentTypeCode" },
               { "//ram:ApplicableHeaderTradeSettlement/ram:CreditorReferenceID", "DirectDebitCreditorId" },
               { "//ram:ApplicableHeaderTradeSettlement/ram:SpecifiedTradePaymentTerms/ram:DirectDebitMandateID", "DirectDebitMandantRef" },
               { "//ram:ApplicableHeaderTradeSettlement/ram:SpecifiedTradeSettlementPaymentMeans/ram:ApplicableTradeSettlementFinancialCard/ram:ID", "PaymentCardPrimaryAccountNr" },
               { "//ram:ApplicableHeaderTradeSettlement/ram:SpecifiedTradeSettlementPaymentMeans/ram:ApplicableTradeSettlementFinancialCard/ram:CardholderName", "PaymentCardHolderName" },
               { descriptionPaymentTerms, "PaymentTerms" },
            }, ref yPosition, fontTextHeader, fontBody, qrRect, qrPage);

            yPosition += 12;
            #endregion

            #region Discount table (discount at the level of the invoice)
            //if no discount amount, table isn't rendered, check if there is any non zero discount amount at line level, if yes render the discount table with only the lines with non zero discount amount
            var hasZeroAmount = HasAnyNonZeroAmount(invoiceDoc!, nsmgr!, "//ram:IncludedSupplyChainTradeLineItem");
            if (hasZeroAmount)
            {
                string discountSubtotalXpath = "//ram:IncludedSupplyChainTradeLineItem";
                if (CheckIfXpathExist(invoiceDoc, nsmgr!, discountSubtotalXpath))
                {
                    string[] columnTaxSubtotalHeaders = {_translationProvider.Translate("InvoiceTableNr"), _translationProvider.Translate("InvoiceTablePosDescription"),
                                          _translationProvider.Translate("InvoiceLineAllowanceReason"), _translationProvider.Translate("ItemPriceDiscount"), _translationProvider.Translate("InvoiceTableNetAmount")};
                    int[] columnTaxSubTotalWidths = { 20, 250, 105, 70, 70 };
                    XStringFormat[] alignments = { XStringFormats.TopCenter, XStringFormats.TopLeft, XStringFormats.TopLeft, XStringFormats.TopRight, XStringFormats.TopRight };
                    //table "position", "Description", "discount reason", "discount(net)", "Nettobetrag"
                    CreateTableWithTitle(pdfDoc, ref pfdPage, ref xGraphics, invoiceDoc, nsmgr!, _translationProvider, new Dictionary<string, string>
                {
                    { ".//ram:LineID", "InvoiceTableNr" },
                    { ".//ram:SpecifiedTradeProduct/ram:Name", "InvoiceTablePosDescription" },
                    { ".//ram:SpecifiedLineTradeSettlement/ram:SpecifiedTradeAllowanceCharge/ram:Reason", "InvoiceLineAllowanceReason" },
                    { ".//ram:SpecifiedLineTradeSettlement/ram:SpecifiedTradeAllowanceCharge/ram:ActualAmount", "ItemPriceDiscount" },
                    { ".//ram:SpecifiedLineTradeSettlement/ram:SpecifiedTradeSettlementLineMonetarySummation/ram:LineTotalAmount", "InvoiceTableNetAmount"  }
                }, ref yPosition, fontColumnHeader, fontTableBody, discountSubtotalXpath, _translationProvider.Translate("OverviewDiscount"), TopLeftAlignment, columnTaxSubtotalHeaders, columnTaxSubTotalWidths, alignments);
                }
                yPosition += 6;
            }
            #endregion

            #region TaxSubtotal (Steuer - Zwischensumme)
            string taxSubtotalXpath = "//ram:ApplicableHeaderTradeSettlement/ram:ApplicableTradeTax";
            if (CheckIfXpathExist(invoiceDoc, nsmgr!, taxSubtotalXpath))
            {
                yPosition += 6;
                string[] columnTaxSubtotalHeaders = {_translationProvider.Translate("SubtotalTaxTableCategoryCode"), _translationProvider.Translate("SubtotalTaxTableTypeCode"),
                                          _translationProvider.Translate("SubtotalTaxTablePercent"), _translationProvider.Translate("SubtotalTaxTableBaseAmount"), _translationProvider.Translate("SubtotalTaxTableAmount")};
                int[] columnTaxSubTotalWidths = { 95, 230, 60, 65, 65 };
                XStringFormat[] alignments = { XStringFormats.TopLeft, XStringFormats.TopLeft, XStringFormats.TopRight, XStringFormats.TopRight, XStringFormats.TopRight };
                //table "Category", "Reason", "Steuern %", "Nettobetrag", "Bruttobetrag"
                CreateTableWithTitle(pdfDoc, ref pfdPage, ref xGraphics, invoiceDoc, nsmgr!, _translationProvider, new Dictionary<string, string>
                {
                    { ".//ram:CategoryCode", "SubtotalTaxTableCategoryCode" },
                    { ".//ram:TypeCode", "SubtotalTaxTableTypeCode" },
                    { ".//ram:RateApplicablePercent", "SubtotalTaxTablePercent" },
                    { ".//ram:BasisAmount", "SubtotalTaxTableBaseAmount" },
                    { ".//ram:CalculatedAmount", "SubtotalTaxTableAmount" }
                }, ref yPosition, fontColumnHeader, fontTableBody, taxSubtotalXpath, _translationProvider.Translate("OverviewSalesTax"), TopLeftAlignment, columnTaxSubtotalHeaders, columnTaxSubTotalWidths, alignments);
                yPosition += 6;
            }
            #endregion

            #region Handling flat rate
            string handlingFlatRateXpath = "//ram:SpecifiedTradeAllowanceCharge/ram:ActualAmount";
            if (CheckIfXpathExist(invoiceDoc, nsmgr!, handlingFlatRateXpath))
            {
                string allowanceChargeXpath = "//ram:ApplicableHeaderTradeSettlement/ram:SpecifiedTradeAllowanceCharge";
                string[] columnTaxSubtotalHeaders = {_translationProvider.Translate("SubtotalTaxTableReason"), _translationProvider.Translate("SubtotalTaxTableCategoryCode"), _translationProvider.Translate("SubtotalTaxTableTypeCode"),
                                          _translationProvider.Translate("SubtotalTaxTablePercent"), _translationProvider.Translate("SubtotalTaxTableBaseAmount"), _translationProvider .Translate("InvoiceTableGrossAmount")};
                int[] columnTaxSubTotalWidths = { 185, 95, 50, 55, 65, 65 };
                XStringFormat[] alignments = { XStringFormats.TopLeft, XStringFormats.TopLeft, XStringFormats.TopLeft, XStringFormats.TopRight, XStringFormats.TopRight, XStringFormats.TopRight };
                //table "Category", "Reason","Tax Category", "TypeCode", "Steuern %", "Nettobetrag", "Bruttobetrag"
                CreateTableWithTitle(pdfDoc, ref pfdPage, ref xGraphics, invoiceDoc, nsmgr!, _translationProvider, new Dictionary<string, string>
            {
                { ".//ram:Reason", "SubtotalTaxTableReason" },
                { ".//ram:CategoryCode", "SubtotalTaxTableCategoryCode" },
                { ".//ram:TypeCode", "SubtotalTaxTableTypeCode" },
                { ".//ram:RateApplicablePercent", "SubtotalTaxTablePercent" },
                { ".//ram:ActualAmount", "SubtotalTaxTableBaseAmount" },
                { ".//ram:CalculatedAmount", "InvoiceTableGrossAmount" }
            }, ref yPosition, fontColumnHeader, fontTableBody, allowanceChargeXpath, _translationProvider.Translate("OverviewHandyFlatRate"), TopLeftAlignment, columnTaxSubtotalHeaders, columnTaxSubTotalWidths, alignments);
                yPosition += 10;
            }
            #endregion

            #region Breakdown of sales tax (Aufschlüsselung der Umsatzsteuer)
            CreateContentTableWithLabel(pdfDoc, ref pfdPage, ref xGraphics, _translationProvider.Translate("OverviewSalesTax"), invoiceDoc, nsmgr!, _translationProvider, new Dictionary<string, string>
            {
                { "//ram:ApplicableTradeTax/ram:CategoryCode", "VatCategoryCode" }
            }, ref yPosition, fontTextHeader, fontBody);
            #endregion

            #region Miscellaneous (Sontiges)
            CreateContentTableWithLabel(pdfDoc, ref pfdPage, ref xGraphics, _translationProvider.Translate("OverviewVarios"), invoiceDoc, nsmgr!, _translationProvider, new Dictionary<string, string>
            {
               { "//ram:ApplicableHeaderTradeAgreement/ram:SpecifiedProjectId/ram:ID", "ProjectId" },
               { "//ram:ApplicableHeaderTradeAgreement/ram:SpecifiedProjectId/ram:Name", "ProjectName" },
               { "//ram:ApplicableHeaderTradeAgreement/ram:ContractReferencedDocument/ram:IssuerAssignedID", "ContractIissuerAssignedId" },
               { "//ram:ApplicableHeaderTradeAgreement/ram:BuyerOrderReferencedDocument/ram:IssuerAssignedID", "BuyerIssuerAssignedId" },
               { "//ram:ApplicableHeaderTradeAgreement/ram:SellerOrderReferencedDocument/ram:IssuerAssignedID", "SalesOrderReference" }
            }, ref yPosition, fontTextHeader, fontBody);
            #endregion
        }
        finally
        {
            xGraphics.Dispose();
        }
    }
    #endregion
}

