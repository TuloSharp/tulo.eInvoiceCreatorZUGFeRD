using tulo.XMLeInvoiceToPdf.Languages;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace tulo.XMLeInvoiceToPdf.Services;

public class PdfGeneratorFromInvoiceUbl(ITranslatorProvider translationProvider) : PdfGeneratorFromInvoiceBase(translationProvider), IPdfGeneratorFromInvoice
{
    private readonly ITranslatorProvider _translationProvider = translationProvider;
    public string Name => "UBL";

    protected override void SetupNamespaces()
    {
        nsmgr!.AddNamespace("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
        nsmgr.AddNamespace("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
    }

    #region UBL Invoice File
    public string GeneratePdfFile(string pdfPath, string xmlInvoiceFileName, string xmlInvoiceContent, bool hasToRenderHeader, string companyLogoPath)
    {
        LoadXmlInvoiceContent(xmlInvoiceContent);

        EnsurePdfSharpInitialized();

        using (PdfDocument pdfDoc = new PdfDocument())
        {
            ApplyDocumentMetadata(pdfDoc);

            CreatePdfContent(xmlInvoiceFileName, xmlInvoiceContent, hasToRenderHeader, pdfDoc, companyLogoPath);
            ApplyFooterToAllPages(pdfDoc);
            pdfDoc.Save(pdfPath);
        }

        return pdfPath;
    }
    #endregion

    #region UBL Invoice Stream
    public MemoryStream GeneratePdfStream(string xmlInvoiceFileName, string xmlInvoiceContent, bool hasToRenderHeader, string companyLogoPath)
    {
        LoadXmlInvoiceContent(xmlInvoiceContent);

        EnsurePdfSharpInitialized();

        MemoryStream pdfStream = new MemoryStream();
        using (PdfDocument pdfDoc = new PdfDocument())
        {
            ApplyDocumentMetadata(pdfDoc);

            CreatePdfContent(xmlInvoiceFileName, xmlInvoiceContent, hasToRenderHeader, pdfDoc, companyLogoPath);
            ApplyFooterToAllPages(pdfDoc);
            pdfDoc.Save(pdfStream, false);
        }

        pdfStream.Position = 0;
        return pdfStream;
    }
    #endregion

    #region Create PDf Content
    private void CreatePdfContent(string xmlInvoiceFileName, string xmlInvoiceContent, bool hasToRenderHeader, PdfDocument pdfDoc, string companyLogoPath)
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
            // UBL notes (if present) should behave like CII notes: prefer notes over payment footer
            var notesText = GetContentXNodesAsText(nsmgr!, "//cbc:Note", invoiceDoc!, separator: " - ");
            if (!string.IsNullOrWhiteSpace(notesText))
            {
                _currentFooterText = notesText;
                _currentFooterTitle = string.Empty;
            }
            #endregion

            #region section header infos
            var infoHeader = xmlInvoiceFileName;
            if (hasToRenderHeader)
            {
                DrawHeader(ref xGraphics, _translationProvider.Translate("PdfDisclamerText"), infoHeader, ref yPosition, pageWidth);
                yPosition += 30;
            }
            else
                yPosition += 42;
            #endregion

            #region Buyer, Seller & Invoice-Info Tables
            // build dictionary for icons
            var iconDictionary = new Dictionary<string, string>
            {
                { "Name", "Company.svg" },
                { "ContactName",  "Person.svg" },
                { "Address" , "Address.svg" },
                { "Phone" , "phone.svg" },
                { "Email" , "EmailAddress.svg" },
                { "TaxId", "IdCard.svg" }
            };

            var vatIdLabel = _translationProvider.Translate("VatID", "VatID");
            var taxNumberLabel = _translationProvider.Translate("TaxNumber", "TaxNumber");

            var sellerTaxNrSchemeVA = taxNumberLabel + ":" + GetContentXNode(nsmgr!, "//cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme[cac:TaxScheme/cbc:ID='VAT']/cbc:CompanyID", invoiceDoc);
            var sellerTaxIDSchemeFC = vatIdLabel + ":" + GetContentXNode(nsmgr!, "//cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme[cac:TaxScheme/cbc:ID='FC']/cbc:CompanyID", invoiceDoc);

            var buildedSellerAddress = GetContentXNode(nsmgr!, "//cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:StreetName", invoiceDoc) + ", " +
                GetContentXNode(nsmgr!, "//cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:PostalZone", invoiceDoc) + " " +
                GetContentXNode(nsmgr!, "//cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cbc:CityName", invoiceDoc) + ", " +
                GetContentXNode(nsmgr!, "//cac:AccountingSupplierParty/cac:Party/cac:PostalAddress/cac:Country/cbc:IdentificationCode", invoiceDoc);

            var buyerTaxNrSchemeVA = taxNumberLabel + ":" + GetContentXNode(nsmgr!, "//cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme[cac:TaxScheme/cbc:ID='VAT']/cbc:CompanyID", invoiceDoc);
            var buyerTaxIDSchemeFC = vatIdLabel + ":" + GetContentXNode(nsmgr!, "//cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme[cac:TaxScheme/cbc:ID='FC']/cbc:CompanyID", invoiceDoc);

            var buildedBuyerAddress = GetContentXNode(nsmgr!, "//cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:StreetName", invoiceDoc) + ", " +
                GetContentXNode(nsmgr!, "//cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:PostalZone", invoiceDoc) + " " +
                GetContentXNode(nsmgr!, "//cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:CityName", invoiceDoc) + ", " +
                GetContentXNode(nsmgr!, "//cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cac:Country/cbc:IdentificationCode", invoiceDoc);

            var sellerRows = new List<(string, string)>
            {
                ("//cac:AccountingSupplierParty/cac:Party/cac:PartyLegalEntity/cbc:RegistrationName", "SellerName"),
                (sellerTaxIDSchemeFC, "TaxId"),
                (sellerTaxNrSchemeVA, "TaxNr"),
                ("//cac:AccountingSupplierParty/cac:Party/cac:Contact/cbc:Name", "SellerContactName"),
                (buildedSellerAddress, "Address"),
                ("//cac:AccountingSupplierParty/cac:Party/cac:Contact/cbc:Telephone", "SellerPhone"),
                ("//cac:AccountingSupplierParty/cac:Party/cac:Contact/cbc:ElectronicMail", "SellerEmail"),
            };

            var buyerRows = new List<(string, string)>
            {
                ("//cac:AccountingCustomerParty/cac:Party/cac:PartyLegalEntity/cbc:RegistrationName", "BuyerName"),
                (buyerTaxIDSchemeFC, "TaxId"),
                (buyerTaxNrSchemeVA, "TaxNr"),
                ("//cac:AccountingCustomerParty/cac:Party/cac:Contact/cbc:Name", "BuyerContactName"),
                (buildedBuyerAddress, "Address"),
                ("//cac:AccountingCustomerParty/cac:Party/cac:Contact/cbc:Telephone", "BuyerPhone"),
                ("//cac:AccountingCustomerParty/cac:Party/cac:Contact/cbc:ElectronicMail", "BuyerEmail"),
            };

            var titleInvoiceTypeCode = GetTitleInvoiceTypeCode("//cbc:InvoiceTypeCode");
            var invoiceFields = new Dictionary<string, string>
            {
                { "//cbc:ID", "InvoiceNr" },
                { "//cbc:IssueDate", "InvoiceIssueDate" },
                { "//cbc:BuyerReference", "BuyerRefId" },
                { "//cbc:InvoiceTypeCode", "InvoiceTypeCode" }
            };

            CreateBuyerInvocieDataSellerBlock(pdfDoc, ref pfdPage, ref xGraphics, invoiceDoc, nsmgr!, _translationProvider, sellerRows, buyerRows, _translationProvider.Translate(titleInvoiceTypeCode), invoiceFields, ref yPosition, fontTitleInfo, fontBody, iconDictionary, true, companyLogoPath);
            yPosition += 16;
            #endregion

            #region occurrence date (UBL: delivery / invoice period / due date)
            var result = string.Empty;
            // RAW values
            var occurrenceRaw = GetContentXNode(nsmgr!, "//cac:Delivery/cbc:ActualDeliveryDate", invoiceDoc);
            var billingStartRaw = GetContentXNode(nsmgr!, "//cac:InvoicePeriod/cbc:StartDate", invoiceDoc);
            var billingEndRaw = GetContentXNode(nsmgr!, "//cac:InvoicePeriod/cbc:EndDate", invoiceDoc);
            var dueDateRaw = GetContentXNode(nsmgr!, "(//cbc:DueDate)[1]", invoiceDoc);

            string? occurrenceDate = !occurrenceRaw.Contains(ContentNotFound) ? _translationProvider.Translate("OccurrenceDateTime") + occurrenceRaw : null;

            string? billingPeriod = (!billingStartRaw.Contains(ContentNotFound) || !billingEndRaw.Contains(ContentNotFound))
                ? _translationProvider.Translate("BillingDateTimeFromTo")
                    .Replace("StartDate", billingStartRaw.Contains(ContentNotFound) ? "" : billingStartRaw)
                    .Replace("EndDate", billingEndRaw.Contains(ContentNotFound) ? "" : billingEndRaw)
                : null;

            string? dueDate = !dueDateRaw.Contains(ContentNotFound) ? _translationProvider.Translate("DueDate") + dueDateRaw : null;

            // Combine
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
            CreateInvoicePositionsTable(pdfDoc, ref pfdPage, ref xGraphics, invoiceDoc, nsmgr!, _translationProvider,
                new Dictionary<string, string>
                {
                    { ".//cbc:ID", "InvoiceTableNr" },
                    { ".//cbc:ParentDocumentLineReference/cbc:LineID", "ParentLineId" },
                    { ".//cac:Item/cbc:Name", "InvoiceTablePosText" },
                    { ".//cac:Item/cbc:Description", "InvoiceTablePosText" },
                    { ".//cac:Item/cac:SellersItemIdentification/cbc:ID", "InvoiceTablePosText" },
                    { ".//cac:Item/cac:StandardItemIdentification/cbc:ID", "InvoiceTablePosText" },
                    { ".//cbc:InvoicedQuantity", "InvoiceTableQuantity" },
                    { ".//cbc:InvoicedQuantity/@unitCode", "InvoiceTableUnit" },
                    { ".//cac:Price/cbc:PriceAmount", "InvoiceTableUnitNetPrice" },
                    { ".//cac:Item/cac:ClassifiedTaxCategory/cbc:Percent", "InvoiceTableTaxPrecentage" },
                    { ".//cbc:LineExtensionAmount", "InvoiceTableNetAmount" }
                },
                ref yPosition, fontColumnHeader, fontTableBody, "//cac:InvoiceLine");
            #endregion

            var currentCurrency = _translationProvider.Translate(GetContentXNode(nsmgr!, "//cbc:DocumentCurrencyCode", invoiceDoc));

            #region total amount table
            CreateInvoiceResultTable(pdfDoc, ref pfdPage, ref xGraphics, invoiceDoc, nsmgr!, _translationProvider,
                new Dictionary<string, string>
                {
                    { "//cac:LegalMonetaryTotal/cbc:ChargeTotalAmount", "InvoiceChargeTotalAmount" },
                    { "//cac:LegalMonetaryTotal/cbc:AllowanceTotalAmount", "InvoiceAllowanceTotalAmount" },
                    { "//cac:LegalMonetaryTotal/cbc:TaxExclusiveAmount", "InvoiceTotalAmountWithoutVat" },
                    { "//cac:TaxTotal/cbc:TaxAmount", "InvoiceTotalVatAmount" },
                    { "//cac:LegalMonetaryTotal/cbc:TaxInclusiveAmount", "SumInvoiceLineNetAmount" }
                },
                ref yPosition, fontColumnHeader, fontTableBody, currentCurrency, "SumInvoiceLineNetAmount");
            #endregion

            #region Prepaid & Payable amount table
            CreateInvoiceResultTable(pdfDoc, ref pfdPage, ref xGraphics, invoiceDoc, nsmgr!, _translationProvider,
                new Dictionary<string, string>
                {
                    { "//cac:LegalMonetaryTotal/cbc:PrepaidAmount", "PaidAmount" },
                    { "//cac:LegalMonetaryTotal/cbc:PayableAmount", "AmountDueForPayment" }
                },
                ref yPosition, fontColumnHeader, fontTableBody, currentCurrency, "AmountDueForPayment");
            #endregion

            #region Payment data (Zahlungsdaten)
            var pmNode = GetContentXNode(nsmgr!, "//cac:PaymentMeans/cbc:PaymentMeansCode", invoiceDoc);

            XRect? qrRect = null;
            PdfPage? qrPage = null;

            if (pmNode == "58" || pmNode == "30")
            {
                CreateQrCode(pdfDoc, ref pfdPage, ref xGraphics, invoiceDoc, nsmgr!, _translationProvider,
                    [
                        "//cac:PaymentMeans/cac:PayeeFinancialAccount/cbc:ID",
                        "//cac:PaymentMeans/cac:PayeeFinancialAccount/cac:FinancialInstitutionBranch/cbc:ID",
                        "//cac:AccountingSupplierParty/cac:Party/cac:PartyName/cbc:Name",
                        "//cbc:DocumentCurrencyCode",
                        "//cbc:PayableAmount",
                        "//cac:PaymentMeans/cbc:PaymentID"
                    ],
                    ref yPosition, fontTextHeader, fontBody,
                    out qrRect, out qrPage);
            }

            yPosition += 8;
            var descriptionPaymentTerms = GetContentXNodesAsText(nsmgr!, "//cac:PaymentTerms/cbc:Note", invoiceDoc, separator: " - ");

            CreateContentTableWithLabel(pdfDoc, ref pfdPage, ref xGraphics, _translationProvider.Translate("OverviewPaymentsData"), invoiceDoc, nsmgr!, _translationProvider,
                new Dictionary<string, string>
                {
                    { "//cac:PaymentMeans/cac:PayeeFinancialAccount/cbc:ID", "PaymentAccountId" },
                    { "//cac:PaymentMeans/cac:PayeeFinancialAccount/cac:FinancialInstitutionBranch/cbc:ID", "PaymentServiceProviderId" },
                    { "//cbc:DocumentCurrencyCode", "PaymentCurrencyCode" },
                    { "//cac:PaymentMeans/cbc:PaymentID", "RemittanceInfo" },
                    { "//cac:PaymentMeans/cbc:PaymentMeansCode", "PaymentTypeCode" },
                    { "//cac:AccountingSupplierParty/cac:Party/cac:PartyIdentification[cbc:ID/@schemeID='SEPA']/cbc:ID", "DirectDebitCreditorId" },
                    { "//cac:PaymentMeans/cac:PaymentMandate/cbc:ID", "DirectDebitMandantRef" },
                    { "//cac:PaymentMeans/cac:PaymentMandate/cac:PayerFinancialAccount/cbc:ID", "DirectDebitIban" },
                    { "//cac:PaymentMeans/cac:CardAccount/cbc:PrimaryAccountNumberID", "PaymentCardPrimaryAccountNr" },
                    { "//cac:PaymentMeans/cac:CardAccount/cbc:HolderName", "PaymentCardHolderName" },
                    { descriptionPaymentTerms, "PaymentTerms" }
                },
                ref yPosition, fontTextHeader, fontBody, qrRect, qrPage);
            #endregion

            #region Discount table (AllowanceCharge on line level)
            var allowanceChargeExistsXpath = "//cac:InvoiceLine/cac:AllowanceCharge";
            if (CheckIfXpathExist(invoiceDoc, nsmgr!, allowanceChargeExistsXpath))
            {
                string[] headers =
                {
                    _translationProvider.Translate("InvoiceTableNr"),
                    _translationProvider.Translate("InvoiceTablePosDescription"),
                    _translationProvider.Translate("InvoiceLineAllowanceReason"),
                    _translationProvider.Translate("ItemPriceDiscount"),
                    _translationProvider.Translate("InvoiceTableNetAmount")
                };

                int[] widths = { 20, 250, 105, 70, 70 };
                XStringFormat[] aligns = { XStringFormats.TopLeft, XStringFormats.TopLeft, XStringFormats.TopLeft, XStringFormats.TopRight, XStringFormats.TopRight };

                CreateTableWithTitle(pdfDoc, ref pfdPage, ref xGraphics, invoiceDoc, nsmgr!, _translationProvider,
                    new Dictionary<string, string>
                    {
                        { ".//cbc:ID", "InvoiceTableNr" },
                        { ".//cac:Item/cbc:Name", "InvoiceTablePosDescription" },
                        { ".//cac:AllowanceCharge/cbc:AllowanceChargeReason", "InvoiceLineAllowanceReason" },
                        { ".//cac:AllowanceCharge/cbc:Amount", "ItemPriceDiscount" },
                        { ".//cbc:LineExtensionAmount", "InvoiceTableNetAmount" }
                    },
                    ref yPosition, fontColumnHeader, fontTableBody, "//cac:InvoiceLine", _translationProvider.Translate("OverviewDiscount"), TopLeftAlignment, headers, widths, aligns);

                yPosition += 6;
            }
            #endregion

            #region TaxSubtotal (Steuer - Zwischensumme)
            string taxSubtotalXpath = "//cac:TaxTotal/cac:TaxSubtotal";
            if (CheckIfXpathExist(invoiceDoc, nsmgr!, taxSubtotalXpath))
            {
                yPosition += 6;
                string[] headers =
                {
                    _translationProvider.Translate("SubtotalTaxTableCategoryCode"),
                    _translationProvider.Translate("SubtotalTaxTableTypeCode"),
                    _translationProvider.Translate("SubtotalTaxTablePercent"),
                    _translationProvider.Translate("SubtotalTaxTableBaseAmount"),
                    _translationProvider.Translate("SubtotalTaxTableAmount")
                };

                int[] widths = { 95, 230, 60, 65, 65 };
                XStringFormat[] aligns = { XStringFormats.TopLeft, XStringFormats.TopLeft, XStringFormats.TopRight, XStringFormats.TopRight, XStringFormats.TopRight };

                CreateTableWithTitle(pdfDoc, ref pfdPage, ref xGraphics, invoiceDoc, nsmgr!, _translationProvider,
                    new Dictionary<string, string>
                    {
                        { ".//cac:TaxCategory/cbc:ID", "SubtotalTaxTableCategoryCode" },
                        { ".//cac:TaxCategory/cac:TaxScheme/cbc:ID", "SubtotalTaxTableTypeCode" },
                        { ".//cac:TaxCategory/cbc:Percent", "SubtotalTaxTablePercent" },
                        { ".//cbc:TaxableAmount", "SubtotalTaxTableBaseAmount" },
                        { ".//cbc:TaxAmount", "SubtotalTaxTableAmount" }
                    },
                    ref yPosition, fontColumnHeader, fontTableBody, taxSubtotalXpath, _translationProvider.Translate("OverviewSalesTax"), TopLeftAlignment, headers, widths, aligns);

                yPosition += 6;
            }
            #endregion

            #region Breakdown of sales tax (Aufschlüsselung der Umsatzsteuer)
            CreateContentTableWithLabel(pdfDoc, ref pfdPage, ref xGraphics, _translationProvider.Translate("OverviewSalesTax"), invoiceDoc, nsmgr!, _translationProvider,
                new Dictionary<string, string>
                {
                    { "//cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:ID", "VatCategoryCode" }
                },
                ref yPosition, fontTextHeader, fontBody);
            #endregion

            #region Miscellaneous (Sontiges)
            CreateContentTableWithLabel(pdfDoc, ref pfdPage, ref xGraphics, _translationProvider.Translate("OverviewVarios"), invoiceDoc, nsmgr!, _translationProvider,
                new Dictionary<string, string>
                {
                    { "//cac:ProjectReference/cbc:ID", "ProjectId" },
                    { "//cac:ProjectReference/cbc:Name", "ProjectName" },
                    { "//cac:ContractDocumentReference/cbc:ID", "ContractIissuerAssignedId" },
                    { "//cac:OrderReference/cbc:ID", "BuyerIssuerAssignedId" },
                    { "//cac:OrderReference/cbc:SalesOrderID", "SalesOrderReference" }
                },
                ref yPosition, fontTextHeader, fontBody);
            #endregion

        }
        finally
        {
            xGraphics.Dispose();
        }
    }
    #endregion
}
