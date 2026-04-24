using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using QRCoder;
using Svg;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Imaging;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;
using tulo.XMLeInvoiceToPdf.Languages;
using tulo.XMLeInvoiceToPdf.Utilities;

namespace tulo.XMLeInvoiceToPdf.Services;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
public abstract class PdfGeneratorFromInvoiceBase(ITranslatorProvider translationProvider)
{
    protected readonly ITranslatorProvider TranslationProvider = translationProvider ?? throw new ArgumentNullException(nameof(translationProvider));
    protected ITranslatorProvider Translator => TranslationProvider;

    protected string _iconsPath = Path.Combine(AppContext.BaseDirectory, "icons");

    protected XmlDocument? invoiceDoc;
    protected XmlDocument? InvoiceDoc
    {
        get => invoiceDoc;
        set => invoiceDoc = value;
    }

    protected XmlNamespaceManager? nsmgr;
    protected XmlNamespaceManager? Nsmgr
    {
        get => nsmgr;
        set => nsmgr = value;
    }

    protected const string ContentNotFound = "N/A";

    protected CultureInfo culture = CultureInfo.GetCultureInfo("de-DE");

    private static bool _pdfSharpInitialized;
    private static readonly object _pdfSharpInitLock = new();


    protected static void EnsurePdfSharpInitialized()
    {
        if (_pdfSharpInitialized)
            return;

        lock (_pdfSharpInitLock)
        {
            if (_pdfSharpInitialized)
                return;

            GlobalFontSettings.FontResolver ??= new EmbeddedFontResolver();

            _pdfSharpInitialized = true;
        }
    }

    protected virtual void ApplyDocumentMetadata(PdfDocument pdfDocument)
    {
        if (pdfDocument == null)
            return;

        string invoiceNumber = SafeMetadataValue(TryGetInvoiceNumber());
        string invoiceDate = SafeMetadataValue(TryGetInvoiceDate());
        string sellerName = SafeMetadataValue(TryGetSellerName());
        string buyerName = SafeMetadataValue(TryGetBuyerName());
        string invoiceType = SafeMetadataValue(TryGetInvoiceTypeTitle());

        pdfDocument.Info.Title = $"E-Rechnung {invoiceNumber}";
        pdfDocument.Info.Author = sellerName;
        pdfDocument.Info.Subject = $"{invoiceType} vom {invoiceDate}";
        pdfDocument.Info.Keywords = $"E-Rechnung, XML, PDF, {invoiceNumber}, {sellerName}, {buyerName}";
        pdfDocument.Info.Creator = "tulo.XMLeInvoiceToPdf";
        //pdfDocument.Info.Producer = "PdfSharp Extended";
        pdfDocument.Info.CreationDate = DateTime.Now;
        pdfDocument.Info.ModificationDate = DateTime.Now;
    }

    protected virtual string TryGetInvoiceNumber()
    {
        try
        {
            return GetFirstExistingValue("/rsm:CrossIndustryInvoice/rsm:ExchangedDocument/ram:ID", "/*[local-name()='Invoice']/*[local-name()='ID']", "/*[local-name()='CreditNote']/*[local-name()='ID']");
        }
        catch
        {
            return ContentNotFound;
        }
    }

    protected virtual string TryGetInvoiceDate()
    {
        try
        {
            return GetFirstExistingValue("/rsm:CrossIndustryInvoice/rsm:ExchangedDocument/ram:IssueDateTime/udt:DateTimeString", "/*[local-name()='Invoice']/*[local-name()='IssueDate']", "/*[local-name()='CreditNote']/*[local-name()='IssueDate']");
        }
        catch
        {
            return ContentNotFound;
        }
    }

    protected virtual string TryGetSellerName()
    {
        try
        {
            return GetFirstExistingValue("/rsm:CrossIndustryInvoice/rsm:SupplyChainTradeTransaction/ram:ApplicableHeaderTradeAgreement/ram:SellerTradeParty/ram:Name", "/*[local-name()='Invoice']//*[local-name()='AccountingSupplierParty']//*[local-name()='Party']//*[local-name()='Name']", "/*[local-name()='CreditNote']//*[local-name()='AccountingSupplierParty']//*[local-name()='Party']//*[local-name()='Name']");
        }
        catch
        {
            return ContentNotFound;
        }
    }

    protected virtual string TryGetBuyerName()
    {
        try
        {
            return GetFirstExistingValue("/rsm:CrossIndustryInvoice/rsm:SupplyChainTradeTransaction/ram:ApplicableHeaderTradeAgreement/ram:BuyerTradeParty/ram:Name", "/*[local-name()='Invoice']//*[local-name()='AccountingCustomerParty']//*[local-name()='Party']//*[local-name()='Name']", "/*[local-name()='CreditNote']//*[local-name()='AccountingCustomerParty']//*[local-name()='Party']//*[local-name()='Name']");
        }
        catch
        {
            return ContentNotFound;
        }
    }

    protected virtual string TryGetInvoiceTypeTitle()
    {
        try
        {
            return GetTitleInvoiceTypeCode("/rsm:CrossIndustryInvoice/rsm:ExchangedDocument/ram:TypeCode");
        }
        catch
        {
            return "E-Rechnung";
        }
    }

    protected virtual string GetFirstExistingValue(params string[] xpaths)
    {
        if (InvoiceDoc == null || Nsmgr == null || xpaths == null || xpaths.Length == 0)
            return ContentNotFound;

        foreach (var xpath in xpaths)
        {
            try
            {
                var node = InvoiceDoc.SelectSingleNode(xpath, Nsmgr);
                var value = node?.InnerText?.Trim();

                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }
            catch
            {
            }
        }

        return ContentNotFound;
    }

    protected static string SafeMetadataValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value == ContentNotFound)
            return "N/A";

        return value.Trim();
    }

    #region Fonts
    protected static string familyFontName = "Roboto";
    protected XFont fontTitle = new(familyFontName, 12);
    protected XFont fontTableTitle = new(familyFontName, 10, XFontStyleEx.Bold);
    protected XFont fontBody = new(familyFontName, 8);
    protected XFont fontDisclamerHeader = new(familyFontName, 10);
    protected XFont fontTextHeader = new(familyFontName, 8);
    protected XFont fontTitleFooter = new(familyFontName, 8, XFontStyleEx.Bold);
    protected XFont fontFooter = new(familyFontName, 7);
    protected XFont fontTitleInfo = new(familyFontName, 12, XFontStyleEx.Bold);
    protected XFont fontColumnHeader = new(familyFontName, 8, XFontStyleEx.Bold);
    protected XFont fontTotalAmount = new(familyFontName, 10, XFontStyleEx.Bold);
    protected XFont fontTableBody = new(familyFontName, 8);
    #endregion

    #region Colors
    protected XColor _darkBlueColor = XColor.FromArgb(0, 102, 255);
    protected XSolidBrush _darkBlueBrushColor = new(XColor.FromArgb(0, 102, 255));
    protected XSolidBrush _blueBrushColor = new(XColor.FromArgb(230, 238, 246));
    protected XSolidBrush _lightBlueBrushColor = new(XColor.FromArgb(230, 247, 255));
    protected XColor _blueColor = XColor.FromArgb(230, 238, 246);
    protected XPen _bluePen = new(XColor.FromArgb(230, 238, 246), 0.5);
    protected XSolidBrush _grayBrushColor = new(XColor.FromArgb(230, 232, 235));
    protected XColor _grayColor = XColor.FromArgb(230, 232, 235);
    protected XPen _grayPen = new(XColor.FromArgb(230, 232, 235), 0.5);
    protected XSolidBrush _blackBrushColor = new(XColor.FromArgb(0, 0, 0));
    protected XSolidBrush _orangeBrushColor = new(XColor.FromArgb(245, 124, 0));
    protected XBrush _invoiceBoxBrush = new XSolidBrush(XColor.FromArgb(240, 243, 247));
    #endregion

    #region Alignment + paging state
    protected XStringFormat TopLeftAlignment { get; set; } = XStringFormats.TopLeft;
    protected XStringFormat TopRightAlignment { get; set; } = XStringFormats.TopRight;
    protected XStringFormat CenterAlignment { get; set; } = XStringFormats.Center;
    protected double _widthOffse4Text = 4;

    protected const int PageTopMargin = 20;
    protected const int PageBottomMargin = 20;
    protected const int PageLeftMargin = 20;
    protected const int PageRightMargin = 20;

    protected const int FooterReserve = 100;

    protected int _pageNumber = 1;
    protected string _currentHeaderTitle = string.Empty;
    protected string _currentHeaderText = string.Empty;
    protected string _currentFooterTitle = string.Empty;
    protected string _currentFooterText = string.Empty;
    #endregion

    /// <summary>
    /// Derived classes (CII/UBL) must register namespaces on <see cref="Nsmgr"/>.
    /// </summary>
    protected abstract void SetupNamespaces();

    /// <summary>
    /// Loads the XML invoice and initializes the namespace manager.
    /// </summary>
    public void LoadXmlInvoiceContent(string xmlInvoiceContent)
    {
        InvoiceDoc = new XmlDocument();
        InvoiceDoc.LoadXml(xmlInvoiceContent);
        Nsmgr = new XmlNamespaceManager(InvoiceDoc.NameTable);
        SetupNamespaces();
    }

    protected string GetTitleInvoiceTypeCode(string xpath)
    {
        string invoiceTypeCode = GetContentXNode(Nsmgr!, xpath, InvoiceDoc);

        return invoiceTypeCode switch
        {
            "380" => "OverviewInvoiceInfo", // Standardrechnung (Invoice/Commercial Invoice)
            "381" => "OverviewCreditInfo", // Gutschrift (Credit Note)
            "383" => "OverviewDirectDebit", // Lastschrift (Debit Note)
            "384" => "OverviewInvoiceCorrection", // Rechnungskorrektur (corrected Invoice)
            "389" => "OverviewSelfBilledCreditNoteInfo", // Selbstfakturierte Gutschrift (SelfBilledInvoice)
            //"388" => "OverviewConsolidatedInvoice", // Konsolidierte Rechnung (Consolidate Invoice)
            "572" => "OverviewAdvancePaymentInvoiceInfo", // Vorauszahlungsrechnung
            "875" => "OverviewProformaInvoiceInfo", // Proformarechnung
            _ => "InvoiceTypeCode not found"
        };
    }

    public void DrawTableTitle(ref XGraphics xGraphics, string tableTitle, ref int yPosition, XStringFormat customXStringFromat)
    {
        XSize textSize = xGraphics.MeasureString(tableTitle, fontTableTitle);
        double textWidth = textSize.Width;
        double textHeight = textSize.Height + _widthOffse4Text;
        xGraphics.DrawString(tableTitle, fontTableTitle, _darkBlueBrushColor, new XRect(40, yPosition, textWidth, textHeight), customXStringFromat);
        yPosition += 12;
    }

    public bool CheckIfXpathExist(XmlDocument? xmlDoc, XmlNamespaceManager nsmgr, string xpath)
    {
        XmlNode? node = xmlDoc!.SelectSingleNode(xpath, nsmgr);
        if (node == null)
            return false;
        else
            return true;
    }

    public void DrawLineText(ref XGraphics xGraphics, string customText, ref int yPosition, XSolidBrush customBrush, XStringFormat customXStringFromat, bool enableWriteWhenContainsNA = false)
    {
        if (enableWriteWhenContainsNA || !customText.Contains(ContentNotFound))
        {
            XSize textSize = xGraphics.MeasureString(customText, fontTitle);
            double textWidth = textSize.Width;
            double textHeight = textSize.Height + _widthOffse4Text;
            xGraphics.DrawString(customText, fontBody, customBrush, new XRect(40, yPosition, textWidth, textHeight), customXStringFromat);
            yPosition += 12;
        }
    }

    public void DrawHeader(ref XGraphics xGraphics, string headerDisclamerText, string additionalText, ref int yPosition, double pageWidth)
    {
        const double leftMargin = 40;
        const double rightMargin = 40;

        double usableWidth = pageWidth - leftMargin - rightMargin;

        // --- Disclaimer multiline ---
        if (!string.IsNullOrWhiteSpace(headerDisclamerText))
        {
            string[] disclaimerLines = WrapText(ref xGraphics, headerDisclamerText, fontDisclamerHeader, usableWidth);

            foreach (var line in disclaimerLines)
            {
                DrawText(xGraphics, line, fontDisclamerHeader, XBrushes.Black, yPosition, usableWidth, XStringFormats.TopLeft, leftMargin);
                yPosition += 12;
            }
        }
        else
        {
            yPosition += 12;
        }

        // --- Additional text multiline ---
        if (!string.IsNullOrWhiteSpace(additionalText))
        {
            string[] additionalLines = WrapText(ref xGraphics, additionalText, fontTextHeader, usableWidth);

            foreach (var line in additionalLines)
            {
                DrawText(xGraphics, line, fontTextHeader, XBrushes.Black, yPosition, usableWidth, XStringFormats.TopLeft, leftMargin);
                yPosition += 12;
            }
        }
        else
        {
            yPosition += 12;
        }

        // separator line
        double startX = leftMargin;
        double endX = pageWidth - rightMargin;

        xGraphics.DrawLine(_bluePen, startX, yPosition, endX, yPosition);
        yPosition += 2;
    }

    public void CreateContentTableWithLabel(PdfDocument pdfDoc, ref PdfPage pdfPage, ref XGraphics xGraphics, string title, XmlDocument? xmlDoc, XmlNamespaceManager nsmgr, ITranslatorProvider translationProvider, Dictionary<string, string> fields, ref int yPosition, XFont fontHeader, XFont fontBody, XRect? qrRect = null, PdfPage? qrPage = null)
    {
        AddNewPageIfNecessary(pdfDoc, ref pdfPage, ref xGraphics, ref yPosition);

        const int leftX = 40;
        const int rightMargin = 40;

        const int topLineGap = 6;
        const int afterTitleGap = 6;
        const int lineGap = 2;
        const int bottomGap = 8;

        XPen linePen = new(_grayColor, 0.4);

        XBrush headerBlue = _darkBlueBrushColor;
        XFont headerBold;
        try { headerBold = new XFont(fontHeader.FontFamily.Name, fontHeader.Size + 2, XFontStyleEx.Bold); }
        catch { headerBold = new XFont(fontHeader.FontFamily.Name, fontHeader.Size + 2); }

        double pageW = pdfPage.Width.Point;
        double contentW = pageW - leftX - rightMargin;

        PdfPage currentPage = pdfPage;

        bool QrIsOnThisPage() => qrRect.HasValue && qrPage != null && ReferenceEquals(currentPage, qrPage);

        bool OverlapsQrVertically(double yTop, double height)
        {
            if (!QrIsOnThisPage()) return false;
            var r = qrRect!.Value;
            double yBottom = yTop + height;
            double qrBottom = r.Y + r.Height;
            return yTop < qrBottom && yBottom > r.Y;
        }

        double ReservedRightWidth(double yTop, double height)
        {
            if (!OverlapsQrVertically(yTop, height)) return 0;
            return qrRect!.Value.Width + 12;
        }

        bool LooksLikeXPath(string s) => !string.IsNullOrWhiteSpace(s) && (s.StartsWith("/") || s.StartsWith("."));

        string ResolveValue(string xpathOrLiteral)
        {
            if (string.IsNullOrWhiteSpace(xpathOrLiteral))
                return ContentNotFound;

            if (!LooksLikeXPath(xpathOrLiteral))
                return xpathOrLiteral;

            try
            {
                XmlNode? node = xmlDoc?.SelectSingleNode(xpathOrLiteral, nsmgr);
                if (node == null) return ContentNotFound;
                var v = node.InnerText?.Trim();
                return string.IsNullOrWhiteSpace(v) ? ContentNotFound : v;
            }
            catch (XPathException)
            {
                return ContentNotFound;
            }
        }

        var visibleItems = new List<(string label, string value, string xpathOrLiteral, string fieldKey)>();
        foreach (var kvp in fields)
        {
            string xpathOrLiteral = kvp.Key;
            string fieldKey = kvp.Value;

            string value = ResolveValue(xpathOrLiteral);
            value = CheckPaymentTypeCode(translationProvider, fieldKey, value);
            value = CheckDifferrentsFieldKeys(translationProvider, xpathOrLiteral, fieldKey, value);
            value = ParseDateTimeToRightFormat(xpathOrLiteral, value);

            if (value == ContentNotFound || string.IsNullOrWhiteSpace(value))
                continue;

            string label = translationProvider.Translate(fieldKey, fieldKey);

            if (label.Contains("IBAN", StringComparison.OrdinalIgnoreCase))
                value = CheckAndFormatIBAN(value);

            visibleItems.Add((label, value, xpathOrLiteral, fieldKey));
        }

        if (visibleItems.Count == 0)
            return;

        double lineH = fontBody.GetHeight();
        double titleH = headerBold.GetHeight();
        double approxH = topLineGap + 1 + afterTitleGap + titleH + (visibleItems.Count * (lineH + lineGap)) + bottomGap + 1;

        AddNewPageIfNecessary(pdfDoc, ref pdfPage, ref xGraphics, ref yPosition, requiredHeight: (int)Math.Ceiling(approxH));

        yPosition += topLineGap;
        xGraphics.DrawLine(linePen, leftX, yPosition, leftX + contentW, yPosition);
        yPosition += afterTitleGap;

        xGraphics.DrawString(title, headerBold, headerBlue, new XRect(leftX, yPosition, contentW, titleH), XStringFormats.TopLeft);

        yPosition += (int)Math.Ceiling(titleH + afterTitleGap);


        foreach (var item in visibleItems)
        {
            AddNewPageIfNecessary(pdfDoc, ref pdfPage, ref xGraphics, ref yPosition);

            double reservedRight = ReservedRightWidth(yPosition, lineH);
            double usableW = contentW - reservedRight;
            if (usableW < 160) usableW = 160;

            string leftText = $"{item.label}: ";
            string full = leftText + item.value;

            string[] wrapped = WrapText(ref xGraphics, full, fontBody, (int)usableW);

            for (int i = 0; i < wrapped.Length; i++)
            {
                xGraphics.DrawString(wrapped[i], fontBody, XBrushes.Black, new XRect(leftX, yPosition, usableW, lineH), XStringFormats.TopLeft);

                yPosition += (int)Math.Ceiling(lineH + lineGap);
            }
        }

        yPosition += 2;
        xGraphics.DrawLine(linePen, leftX, yPosition, leftX + contentW, yPosition);
        yPosition += bottomGap;
    }

    #region Utilities for Method CreateSellerBuyerInfoTable
    private string ResolveXmlOrValue(XmlDocument? doc, XmlNamespaceManager mgr, string xpathOrValue)
    {
        if (string.IsNullOrWhiteSpace(xpathOrValue))
            return string.Empty;

        string s = xpathOrValue.Trim();

        // XPaths starten bei dir mit "/" oder "."
        bool looksLikeXPath = s.StartsWith("/", StringComparison.Ordinal) || s.StartsWith(".", StringComparison.Ordinal);

        if (!looksLikeXPath)
            return ReplaceLForCRorTabByEmptyString(s);   // Direktwert (z.B. TaxId zusammengebaut)

        try
        {
            XmlNode? node = doc?.SelectSingleNode(s, mgr);
            if (node == null) return ContentNotFound;
            return ReplaceLForCRorTabByEmptyString(node.InnerText.Trim());
        }
        catch (System.Xml.XPath.XPathException)
        {
            return ContentNotFound;
        }
    }

    private static string NormalizeIconKey(string fieldKey)
    {
        if (string.IsNullOrWhiteSpace(fieldKey))
            return fieldKey;

        if (fieldKey.StartsWith("Seller", StringComparison.OrdinalIgnoreCase))
            return fieldKey.Substring("Seller".Length);

        if (fieldKey.StartsWith("Buyer", StringComparison.OrdinalIgnoreCase))
            return fieldKey.Substring("Buyer".Length);

        // z.B. TaxId, Address, Name, ContactName, Phone, Email
        return fieldKey;
    }
    #endregion

    public void CreateBuyerInvocieDataSellerBlock(PdfDocument pdfDoc, ref PdfPage pdfPage, ref XGraphics xGraphics, XmlDocument? xmlDoc, XmlNamespaceManager nsmgr, ITranslatorProvider translationProvider, List<(string xpathOrValue, string fieldKey)> sellerRows, List<(string xpathOrValue, string fieldKey)> buyerRows, string invoiceBoxTitle, Dictionary<string, string> invoiceFields, ref int yPosition, XFont fontTitleInfo, XFont fontBody, Dictionary<string, string> svgIconNames, bool keepIconsForPhoneEmailOnly = false, string? logoPath = null)
    {
        const double leftMargin = 40;
        const double rightMargin = 40;
        const double gapToInvoiceBox = 24;

        const double invoiceBoxWidth = 260;
        const double invoiceBoxPadding = 12;

        const double logoMaxWidth = 210;
        const double logoMaxHeight = 80;

        const double topBlockHeight = 95;
        const double gapTopToBottom = 10;
        const double gapBottomToLine = 1;

        XPen linePen = new XPen(_grayColor, 0.8);

        double pageWidth = pdfPage.Width.Point;
        double contentWidth = pageWidth - leftMargin - rightMargin;

        double leftBlockWidth = contentWidth - invoiceBoxWidth - gapToInvoiceBox;

        if (leftBlockWidth < 180)
            leftBlockWidth = 180;

        double rightBlockX = leftMargin + leftBlockWidth + gapToInvoiceBox;

        XFont titleBold;
        try
        {
            titleBold = new XFont(fontTitleInfo.FontFamily.Name, fontTitleInfo.Size, XFontStyleEx.Bold);
        }
        catch
        {
            titleBold = fontTitleInfo;
        }

        XFont nameBold;
        try
        {
            nameBold = new XFont(fontBody.FontFamily.Name, fontBody.Size + 4, XFontStyleEx.Bold);
        }
        catch
        {
            nameBold = new XFont(fontBody.FontFamily.Name, fontBody.Size + 4);
        }

        double lineH = fontBody.GetHeight();
        double titleH = titleBold.GetHeight();
        const int invoiceRowH = 12;

        // SELLER LINES
        var sellerLines = new List<(string text, XFont font, XBrush brush, string iconKey)>();

        foreach (var (xpathOrValue, fieldKey) in sellerRows)
        {
            string value = ResolveXmlOrValue(xmlDoc, nsmgr, xpathOrValue);

            if (string.IsNullOrWhiteSpace(value) || value == ContentNotFound)
                continue;

            string normalizedIconKey = NormalizeIconKey(fieldKey);

            bool allowIcon =
                keepIconsForPhoneEmailOnly &&
                (
                    normalizedIconKey.Equals("Phone", StringComparison.OrdinalIgnoreCase) ||
                    normalizedIconKey.Equals("Email", StringComparison.OrdinalIgnoreCase)
                );

            bool isCompanyName =
                fieldKey.Contains("Name", StringComparison.OrdinalIgnoreCase) &&
                !fieldKey.Contains("Contact", StringComparison.OrdinalIgnoreCase);

            XFont f = isCompanyName ? nameBold : fontBody;
            XBrush b = isCompanyName ? _darkBlueBrushColor : _blackBrushColor;

            sellerLines.Add((value, f, b, allowIcon ? normalizedIconKey : string.Empty));
        }

        // BUYER LINES
        var buyerLines = new List<(string text, XFont font, XBrush brush, string iconKey)>();

        foreach (var (xpathOrValue, fieldKey) in buyerRows)
        {
            string value = ResolveXmlOrValue(xmlDoc, nsmgr, xpathOrValue);

            if (string.IsNullOrWhiteSpace(value) || value == ContentNotFound)
                continue;

            string normalizedIconKey = NormalizeIconKey(fieldKey);

            bool allowIcon =
                keepIconsForPhoneEmailOnly &&
                (
                    normalizedIconKey.Equals("Phone", StringComparison.OrdinalIgnoreCase) ||
                    normalizedIconKey.Equals("Email", StringComparison.OrdinalIgnoreCase)
                );

            bool isCompanyName =
                fieldKey.Contains("Name", StringComparison.OrdinalIgnoreCase) &&
                !fieldKey.Contains("Contact", StringComparison.OrdinalIgnoreCase);

            XFont f = isCompanyName ? nameBold : fontBody;
            XBrush b = isCompanyName ? _darkBlueBrushColor : _blackBrushColor;

            buyerLines.Add((value, f, b, allowIcon ? normalizedIconKey : string.Empty));
        }

        // HEIGHTS
        double sellerH = 0;

        foreach (var l in sellerLines)
        {
            sellerH += WrapForWidth(
                ref xGraphics,
                l.text,
                l.font,
                invoiceBoxWidth - 2 * invoiceBoxPadding).Length * lineH;

            if (l.font == nameBold)
                sellerH += 4;
        }

        double buyerH = 0;

        foreach (var l in buyerLines)
        {
            buyerH += WrapForWidth(
                ref xGraphics,
                l.text,
                l.font,
                leftBlockWidth).Length * lineH;

            if (l.font == nameBold)
                buyerH += 4;
        }

        double invoiceBoxH =
            invoiceBoxPadding * 2 +
            titleH +
            8 +
            invoiceFields.Count * invoiceRowH;

        double realTopBlockHeight = Math.Max(topBlockHeight, sellerH);

        double bottomBlockHeight = Math.Max(buyerH, invoiceBoxH);

        double requiredH =
            realTopBlockHeight +
            gapTopToBottom +
            bottomBlockHeight +
            gapBottomToLine +
            1 +
            8;

        AddNewPageIfNecessary(pdfDoc, ref pdfPage, ref xGraphics, ref yPosition, requiredHeight: (int)Math.Ceiling(requiredH));

        double topY = yPosition;
        
        // 1. LOGO LEFT TOP
        if (!string.IsNullOrWhiteSpace(logoPath) && File.Exists(logoPath))
        {
            try
            {
                using XImage logo = XImage.FromFile(logoPath);

                double logoW = logo.PixelWidth;
                double logoH = logo.PixelHeight;

                double scale = Math.Min(
                    logoMaxWidth / logoW,
                    logoMaxHeight / logoH);

                double drawW = logoW * scale;
                double drawH = logoH * scale;

                xGraphics.DrawImage(
                    logo, leftMargin, topY, drawW, drawH);
            }
            catch
            {
                // Do not render the logo if the file is unreadable.
            }
        }

        // 2. SELLER RIGHT TOP
        double sellerY = topY + 8;

        foreach (var (text, f, brush, iconKey) in sellerLines)
        {
            double textX = rightBlockX + invoiceBoxPadding;

            if (!string.IsNullOrEmpty(iconKey))
            {
                DrawSvgIconIfAvailable(
                    ref xGraphics,
                    svgIconNames,
                    iconKey,
                    (int)textX,
                    (int)sellerY);

                textX += 14;
            }

            var lines = WrapForWidth(
                ref xGraphics,
                text,
                f,
                invoiceBoxWidth - 2 * invoiceBoxPadding);

            foreach (string line in lines)
            {
                xGraphics.DrawString(
                    line,
                    f,
                    brush,
                    new XRect(
                        textX,
                        sellerY,
                        invoiceBoxWidth - 2 * invoiceBoxPadding,
                        lineH),
                    XStringFormats.TopLeft);

                sellerY += lineH;
            }

            if (f == nameBold)
                sellerY += 4;
        }

        // 3. BUYER LEFT BELOW LOGO
        double bottomY = topY + realTopBlockHeight + gapTopToBottom;
        double buyerY = bottomY;

        const double buyerIndent = 30;

        foreach (var (text, f, brush, iconKey) in buyerLines)
        {
            double textX = leftMargin + buyerIndent;

            if (!string.IsNullOrEmpty(iconKey))
            {
                DrawSvgIconIfAvailable(ref xGraphics, svgIconNames, iconKey, (int)textX, (int)buyerY);

                textX += 14;
            }

            var lines = WrapForWidth(ref xGraphics, text, f, leftBlockWidth - buyerIndent);

            foreach (string line in lines)
            {
                xGraphics.DrawString(line, f, brush, new XRect(textX, buyerY, leftBlockWidth - buyerIndent, lineH), XStringFormats.TopLeft);

                buyerY += lineH;
            }

            if (f == nameBold)
                buyerY += 4;
        }

        // 4. INVOICE BOX RIGHT BELOW SELLER
        double invoiceY = bottomY;

        var boxRect = new XRect(rightBlockX, invoiceY, invoiceBoxWidth, invoiceBoxH);

        xGraphics.DrawRectangle(_invoiceBoxBrush, boxRect);

        double innerY = invoiceY + invoiceBoxPadding;

        xGraphics.DrawString(invoiceBoxTitle, titleBold, XBrushes.Black, new XRect(rightBlockX + invoiceBoxPadding, innerY, invoiceBoxWidth - 2 * invoiceBoxPadding, titleH), XStringFormats.TopLeft);

        innerY += titleH + 8;

        xGraphics.DrawLine(linePen, rightBlockX, innerY - 4, rightBlockX + invoiceBoxWidth, innerY - 4);

        double labelX = rightBlockX + invoiceBoxPadding;
        const double valueW = 120;

        double valueX = rightBlockX + invoiceBoxWidth - invoiceBoxPadding - valueW;

        foreach (var kvp in invoiceFields)
        {
            string xpath = kvp.Key;
            string fieldKey = kvp.Value;

            string label = translationProvider.Translate(fieldKey, fieldKey);

            XmlNode? node = xmlDoc?.SelectSingleNode(xpath, nsmgr);
            string value = node != null ? node.InnerText : ContentNotFound;

            value = ParseDateTimeToRightFormat(xpath, value);

            xGraphics.DrawString($"{label}:", fontBody, XBrushes.Black, new XRect(labelX, innerY, valueX - labelX - 8, invoiceRowH), XStringFormats.TopLeft);

            xGraphics.DrawString(value, fontBody, XBrushes.Black, new XRect(valueX, innerY, valueW, invoiceRowH), XStringFormats.TopRight);

            innerY += invoiceRowH;
        }

        // 5. LINE UNDER BUYER / INVOICE BOX
        double lineY = bottomY + bottomBlockHeight + gapBottomToLine;

        xGraphics.DrawLine(linePen, leftMargin, lineY, leftMargin + contentWidth, lineY);

        yPosition = (int)Math.Ceiling(lineY + 8);
    }
    private string[] WrapForWidth(ref XGraphics g, string? text, XFont font, double width)
    {
        return WrapText(ref g, text ?? string.Empty, font, (int)width);
    }

    public string GetContentXNode(XmlNamespaceManager nsmgr, string xpath, XmlDocument? xmlDoc)
    {
        XmlNode? node = xmlDoc?.SelectSingleNode(xpath, nsmgr);
        if (node == null)
            GetXpathErrorMessage(xpath);
        var fieldValue = node?.InnerText ?? ContentNotFound;
        return ParseDateTimeToRightFormat(xpath, fieldValue);
    }

    public void CreateInvoicePositionsTable(PdfDocument pdfDoc, ref PdfPage pdfPage, ref XGraphics xGraphics, XmlDocument? xmlDoc, XmlNamespaceManager nsmgr, ITranslatorProvider translationProvider, Dictionary<string, string> fields, ref int yPosition, XFont fontColumnHeader, XFont fontTableBody, string lineItemsNode)
    {
        const int startX = 40;

        // 9 columns: #, item description, item number/EAN, quantity, unit, net unit price, tax %, net amount, gross amount
        int[] columnWidths = [30, 185, 75, 33, 30, 36, 38, 44, 44];
        int tableWidth = columnWidths.Sum();

        // Header slightly higher because “item number” + “EAN” appear as 2 lines in the header
        const int rowHeaderHeight = 22;
        const int rowItemHeight = 14;
        const int lineHeight = 12;

        string[] headers = { translationProvider.Translate("InvoiceTableNr"), translationProvider.Translate("InvoiceTablePosText"), translationProvider.Translate("InvoiceTableItemNr"), translationProvider.Translate("InvoiceTableQuantity"), translationProvider.Translate("InvoiceTableUnit"), translationProvider.Translate("InvoiceTableUnitNetPrice"), translationProvider.Translate("InvoiceTableTaxPrecentage"), translationProvider.Translate("InvoiceTableNetAmount"), translationProvider.Translate("InvoiceTableGrossAmount") };

        XPen gridPen = new XPen(_grayColor, 0.4);

        const double tableLeft = startX;
        double tableRight = startX + tableWidth;

        string FormatDisplayLineId(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            value = value.Trim();

            if (value.Contains('.'))
            {
                var parts = value
                    .Split('.', StringSplitOptions.RemoveEmptyEntries)
                    .Select(p =>
                    {
                        var trimmed = p.TrimStart('0');
                        return string.IsNullOrEmpty(trimmed) ? "0" : trimmed;
                    });

                return string.Join(".", parts);
            }

            var plain = value.TrimStart('0');
            return string.IsNullOrEmpty(plain) ? "0" : plain;
        }

        bool IsCompactSubLineNumber(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            value = value.Trim();

            if (value.Contains('.'))
                return false;

            return value.Length >= 3 && value.All(char.IsDigit);
        }

        int GetCompactMainNumber(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0;

            value = value.Trim();

            if (IsCompactSubLineNumber(value))
            {
                // z.B. 101 -> 1, 203 -> 2, 1201 -> 12
                var mainPart = value.Substring(0, value.Length - 2).TrimStart('0');
                if (string.IsNullOrWhiteSpace(mainPart))
                    mainPart = "0";

                return int.TryParse(mainPart, out var n) ? n : 0;
            }

            if (value.Contains('.'))
            {
                var first = value.Split('.', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "0";
                first = first.TrimStart('0');
                if (string.IsNullOrWhiteSpace(first))
                    first = "0";

                return int.TryParse(first, out var n) ? n : 0;
            }

            var plain = value.TrimStart('0');
            if (string.IsNullOrWhiteSpace(plain))
                plain = "0";

            return int.TryParse(plain, out var main) ? main : 0;
        }

        int GetCompactSubNumber(string? value)
        {
            if (!IsCompactSubLineNumber(value))
                return 0;

            value = value!.Trim();

            // z.B. 101 -> 1, 102 -> 2, 203 -> 3
            var subPart = value.Substring(value.Length - 2).TrimStart('0');
            if (string.IsNullOrWhiteSpace(subPart))
                subPart = "0";

            return int.TryParse(subPart, out var n) ? n : 0;
        }

        bool IsSubLine(string? lineId, string? parentLineId)
        {
            return !string.IsNullOrWhiteSpace(parentLineId)
                   || (!string.IsNullOrWhiteSpace(lineId) && lineId.Contains('.'))
                   || IsCompactSubLineNumber(lineId);
        }

        string FormatNumber(string? value, string format)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            if (double.TryParse(NormalizeDecimalValue(value), NumberStyles.Any, culture, out var number))
                return number.ToString(format, culture);

            return value.Trim();
        }

        string CalculateGross(string? netAmountValue, string? taxValue)
        {
            if (string.IsNullOrWhiteSpace(netAmountValue) || string.IsNullOrWhiteSpace(taxValue))
                return string.Empty;

            if (double.TryParse(NormalizeDecimalValue(netAmountValue), NumberStyles.Any, culture, out var netAmount) &&
                double.TryParse(NormalizeDecimalValue(taxValue), NumberStyles.Any, culture, out var taxPercentage))
            {
                var grossAmount = netAmount + netAmount * taxPercentage / 100.0;
                grossAmount = Math.Round(grossAmount, 2, MidpointRounding.AwayFromZero);
                return grossAmount.ToString("N2", culture);
            }

            return string.Empty;
        }

        // Header background
        for (int i = 0; i < headers.Length; i++)
        {
            double x = startX + GetColumnOffset(columnWidths, i);
            var headerRect = new XRect(x, yPosition, columnWidths[i], rowHeaderHeight);
            xGraphics.DrawRectangle(_blueBrushColor, headerRect);
        }

        // Header lines
        xGraphics.DrawLine(gridPen, tableLeft, yPosition, tableRight, yPosition);
        xGraphics.DrawLine(gridPen, tableLeft, yPosition + rowHeaderHeight, tableRight, yPosition + rowHeaderHeight);

        for (int i = 1; i < headers.Length; i++)
        {
            double x = startX + GetColumnOffset(columnWidths, i);
            xGraphics.DrawLine(gridPen, x, yPosition, x, yPosition + rowHeaderHeight);
        }

        // Header text
        for (int i = 0; i < headers.Length; i++)
        {
            double x = startX + GetColumnOffset(columnWidths, i);

            if (i == 2)
            {
                // Description Item Nr / EAN Header
                var topRect = new XRect(x + 4, yPosition + 2, columnWidths[i] - 8, lineHeight);
                var botRect = new XRect(x + 4, yPosition + 2 + lineHeight, columnWidths[i] - 8, lineHeight);
                xGraphics.DrawString("Artikelnummer", fontColumnHeader, _blackBrushColor, topRect, XStringFormats.TopLeft);
                xGraphics.DrawString("EAN", fontColumnHeader, _blackBrushColor, botRect, XStringFormats.TopLeft);
            }
            else
            {
                var textRect = new XRect(x + 4, yPosition, columnWidths[i] - 8, rowHeaderHeight);
                var fmt = (i == 1) ? XStringFormats.CenterLeft : XStringFormats.Center;
                xGraphics.DrawString(headers[i], fontColumnHeader, _blackBrushColor, textRect, fmt);
            }
        }

        yPosition += rowHeaderHeight;

        XmlNodeList? lineItems = xmlDoc?.SelectNodes(lineItemsNode, nsmgr);
        if (lineItems == null || lineItems.Count == 0)
            return;

        var rawRows = new List<string[]>();

        foreach (XmlNode lineItem in lineItems)
        {
            string[] itemRow = fields.Keys
                .Select(xpath => lineItem.SelectSingleNode(xpath, nsmgr)?.InnerText ?? string.Empty)
                .ToArray();

            if (itemRow.Length >= 11)
                rawRows.Add(itemRow);
        }

        var sortedRows = rawRows
            .OrderBy(r => GetCompactMainNumber(!string.IsNullOrWhiteSpace(r[1]) ? r[1] : r[0]))
            .ThenBy(r => IsSubLine(r[0], r[1]) ? 1 : 0)   // first main position
            .ThenBy(r =>
            {
                var lineId = r[0]?.Trim() ?? string.Empty;

                if (IsCompactSubLineNumber(lineId))
                    return GetCompactSubNumber(lineId);

                if (lineId.Contains('.'))
                {
                    var parts = lineId.Split('.', StringSplitOptions.RemoveEmptyEntries);
                    var last = parts.LastOrDefault() ?? "0";
                    last = last.TrimStart('0');
                    if (string.IsNullOrWhiteSpace(last))
                        last = "0";

                    return int.TryParse(last, out var n) ? n : 0;
                }

                return 0;
            })
            .ToList();

        foreach (var itemRow in sortedRows)
        {
            AddNewPageIfNecessary(pdfDoc, ref pdfPage, ref xGraphics, ref yPosition);

            string lineIdRaw = itemRow[0]?.Trim() ?? string.Empty;
            string parentLineIdRaw = itemRow[1]?.Trim() ?? string.Empty;
            string nameRaw = itemRow[2]?.Trim() ?? string.Empty;
            string descRaw = itemRow[3]?.Trim() ?? string.Empty;
            string sellerIdRaw = itemRow[4]?.Trim() ?? string.Empty;
            string globalIdRaw = itemRow[5]?.Trim() ?? string.Empty;
            string quantityRaw = itemRow[6]?.Trim() ?? string.Empty;
            string unitRaw = itemRow[7]?.Trim() ?? string.Empty;
            string netUnitPriceRaw = itemRow[8]?.Trim() ?? string.Empty;
            string taxRaw = itemRow[9]?.Trim() ?? string.Empty;
            string netAmountRaw = itemRow[10]?.Trim() ?? string.Empty;

            bool isSubLine = IsSubLine(lineIdRaw, parentLineIdRaw);

            string displayLineId = FormatDisplayLineId(lineIdRaw);

            string positionBeschreibung;
            if (isSubLine)
            {
                string subTop = nameRaw;
                string subBottom = descRaw;
                string subText = string.IsNullOrWhiteSpace(subBottom) ? subTop : string.IsNullOrWhiteSpace(subTop) ? subBottom : $"{subTop}\n{subBottom}";
                positionBeschreibung = subText;
            }
            else
                positionBeschreibung = string.IsNullOrWhiteSpace(descRaw) ? nameRaw : string.IsNullOrWhiteSpace(nameRaw) ? descRaw : $"{descRaw}\n{nameRaw}";

            string artikelUndEan = string.IsNullOrWhiteSpace(globalIdRaw) ? sellerIdRaw : string.IsNullOrWhiteSpace(sellerIdRaw) ? globalIdRaw : $"{sellerIdRaw}\n{globalIdRaw}";

            string qtyStr = FormatNumber(quantityRaw, "N2");

            string unitStr = unitRaw;
            string unitTranslated = translationProvider.Translate(unitStr);
            if (!string.IsNullOrWhiteSpace(unitTranslated))
                unitStr = unitTranslated;

            string unitPriceStr = FormatNumber(netUnitPriceRaw, "N2");
            string taxStr = FormatNumber(taxRaw, "N2");
            string netAmountStr = FormatNumber(netAmountRaw, "N2");
            string grossStr = CalculateGross(netAmountRaw, taxRaw);

            string[] extendedRow = { displayLineId, positionBeschreibung, artikelUndEan, qtyStr, unitStr, unitPriceStr, taxStr, netAmountStr, grossStr };

            string[] wrappedDesc = WrapMultilineCell(ref xGraphics, extendedRow[1], fontTableBody, columnWidths[1] - (isSubLine ? 12 : 8));
            string[] wrappedArt = WrapMultilineCell(ref xGraphics, extendedRow[2], fontTableBody, columnWidths[2] - 8);

            int maxLines = Math.Max(wrappedDesc.Length, wrappedArt.Length);
            int rowHeight = Math.Max(rowItemHeight, maxLines * lineHeight);

            double yTop = yPosition;
            double yBot = yPosition + rowHeight;

            // Grid
            xGraphics.DrawLine(gridPen, tableLeft, yTop, tableRight, yTop);
            xGraphics.DrawLine(gridPen, tableLeft, yBot, tableRight, yBot);

            for (int i = 1; i < extendedRow.Length; i++)
            {
                double x = startX + GetColumnOffset(columnWidths, i);
                xGraphics.DrawLine(gridPen, x, yTop, x, yBot);
            }

            // No. | Description  | Item/EAN  | Quantity  | Unit  | Unit price  | VAT  | Net  | Gross
            XStringFormat[] aligns = { XStringFormats.CenterRight, XStringFormats.TopLeft, XStringFormats.TopLeft, XStringFormats.Center, XStringFormats.Center, XStringFormats.CenterRight, XStringFormats.CenterRight, XStringFormats.CenterRight, XStringFormats.CenterRight };

            double descIndent = isSubLine ? 4 : 0;

            // Draw text
            for (int i = 0; i < extendedRow.Length; i++)
            {
                double x = startX + GetColumnOffset(columnWidths, i);

                if (i == 1)
                {
                    // Position description (multiple lines)
                    int yOff = 0;
                    foreach (var line in wrappedDesc)
                    {
                        var lineRect = new XRect(
                            x + 4 + descIndent,
                            yPosition + yOff + 1,
                            columnWidths[i] - 8 - descIndent,
                            lineHeight);

                        xGraphics.DrawString(line, fontTableBody, _blackBrushColor, lineRect, XStringFormats.TopLeft);
                        yOff += lineHeight;
                    }
                }
                else if (i == 2)
                {
                    // Item number / EAN (multiple lines)
                    int yOff = 0;
                    foreach (var line in wrappedArt)
                    {
                        var lineRect = new XRect(x + 4, yPosition + yOff + 1, columnWidths[i] - 8, lineHeight);
                        xGraphics.DrawString(line, fontTableBody, _blackBrushColor, lineRect, XStringFormats.TopLeft);
                        yOff += lineHeight;
                    }
                }
                else if (i == 0)
                {
                    var textRect = new XRect(x + 2, yPosition, columnWidths[i] - 4, rowHeight);
                    xGraphics.DrawString(extendedRow[i], fontTableBody, _blackBrushColor, textRect, XStringFormats.CenterRight);
                }
                else
                {
                    var textRect = new XRect(x + 4, yPosition, columnWidths[i] - 8, rowHeight);
                    xGraphics.DrawString(extendedRow[i], fontTableBody, _blackBrushColor, textRect, aligns[i]);
                }
            }

            yPosition += rowHeight;
        }
    }

    public void CreateSubTable(PdfDocument pdfDoc, ref PdfPage pdfPage, ref XGraphics xGraphics, XmlDocument? xmlDoc, XmlNamespaceManager nsmgr, ITranslatorProvider translationProvider, Dictionary<string, string> fields, ref int yPosition, XFont fontColumnHeader, XFont fontTableBody, string lineItemsNode, string[] columnHeaders, int[] columnWidths, XStringFormat[] alignments)
    {
        const int startX = 40;
        int currentHeaderX = startX;
        const int rowHeaderHeight = 12;
        const int rowItemHeight = 12;
        int tableWidth = columnWidths.Sum();

        double tableLeft = startX;
        double tableRight = startX + tableWidth;

        for (int i = 0; i < columnHeaders.Length; i++)
        {
            var headerRect = new XRect(currentHeaderX, yPosition, columnWidths[i] - 1, rowHeaderHeight);
            xGraphics.DrawRectangle(_blueBrushColor, headerRect);
            currentHeaderX += columnWidths[i];
        }

        xGraphics.DrawLine(_grayPen, tableLeft, yPosition, tableRight, yPosition);
        xGraphics.DrawLine(_grayPen, tableLeft, yPosition + rowHeaderHeight, tableRight, yPosition + rowHeaderHeight);

        for (int i = 1; i < columnHeaders.Length; i++)
        {
            double x = startX + GetColumnOffset(columnWidths, i);
            xGraphics.DrawLine(_grayPen, x, yPosition, x, yPosition + rowHeaderHeight);
        }

        for (int i = 0; i < columnHeaders.Length; i++)
        {
            var headerRect = new XRect(startX + GetColumnOffset(columnWidths, i) + 2, yPosition, columnWidths[i], rowHeaderHeight);
            xGraphics.DrawString(columnHeaders[i], fontColumnHeader, XBrushes.Black, headerRect, alignments[i]);
        }

        yPosition += rowHeaderHeight;

        XmlNodeList? lineItems = xmlDoc?.SelectNodes(lineItemsNode, nsmgr);

        yPosition += 1;

        if (lineItems != null)
        {
            foreach (XmlNode lineItem in lineItems)
            {
                AddNewPageIfNecessary(pdfDoc, ref pdfPage, ref xGraphics, ref yPosition);

                string[] itemRow = fields.Keys.Select(xpath => lineItem.SelectSingleNode(xpath, nsmgr)?.InnerText ?? string.Empty).ToArray();

                if (!string.IsNullOrWhiteSpace(itemRow[0]))
                {
                    string raw0 = itemRow[0].Trim();

                    if (bool.TryParse(raw0, out bool changeIndicator))
                    {
                        itemRow[0] = changeIndicator
                            ? translationProvider.Translate("ChargeIndicatorFee")
                            : translationProvider.Translate("ChargeIndicatorDiscount");
                    }
                    else
                    {
                        if (int.TryParse(raw0, NumberStyles.Integer, CultureInfo.InvariantCulture, out int posNr))
                            itemRow[0] = posNr.ToString(CultureInfo.InvariantCulture);
                        else
                            itemRow[0] = raw0;
                    }
                }

                if (itemRow.Length > 2)
                {
                    decimal percentage = 0;

                    if (Regex.IsMatch(itemRow[2], @"^-?\d+(\.\d+)?$"))
                    {
                        percentage = decimal.Parse(NormalizeDecimalValue(itemRow[2]), NumberStyles.Any, culture);
                        itemRow[2] = percentage.ToString("N2", culture);
                    }

                    decimal baseAmount = decimal.Parse(NormalizeDecimalValue(itemRow[3]), NumberStyles.Any, culture);
                    decimal amount = decimal.Parse(NormalizeDecimalValue(itemRow[4]), NumberStyles.Any, culture);

                    itemRow[3] = baseAmount.ToString("N2", culture);
                    itemRow[4] = amount.ToString("N2", culture);

                    if (itemRow.Length > 5)
                    {
                        if (itemRow[5] == string.Empty)
                        {
                            var calculatedAmountTemp = amount + amount * (baseAmount / 100);
                            decimal rounded = Math.Round(calculatedAmountTemp, 2);
                            itemRow[5] = rounded.ToString("F2");
                        }
                        else
                        {
                            decimal calculatedAmount = decimal.Parse(NormalizeDecimalValue(itemRow[5]), NumberStyles.Any, culture);
                            itemRow[5] = calculatedAmount.ToString("N2", culture);
                        }
                    }
                }

                int calcRawHeightByDescription = rowItemHeight;
                string[] wrappedDescription = WrapText(ref xGraphics, itemRow[1], fontTableBody, columnWidths[1]);

                if (wrappedDescription.Length >= 2)
                    calcRawHeightByDescription = wrappedDescription.Length * 12;

                double yTop = yPosition;
                double yBot = yPosition + calcRawHeightByDescription;

                xGraphics.DrawLine(_grayPen, tableLeft, yTop, tableRight, yTop);
                xGraphics.DrawLine(_grayPen, tableLeft, yBot, tableRight, yBot);

                // vertikale Innenlinien (nur zwischen Spalten, keine Außenkanten)
                for (int i = 1; i < itemRow.Length; i++)
                {
                    double x = startX + GetColumnOffset(columnWidths, i);
                    xGraphics.DrawLine(_grayPen, x, yTop, x, yBot);
                }

                int currentTextX = startX;
                int maxRowHeight = rowItemHeight;

                for (int i = 0; i < itemRow.Length; i++)
                {

                    if (i == 1)
                    {
                        var adjustedFont = fontTableBody;
                        if (calcRawHeightByDescription > rowItemHeight)
                            adjustedFont = new XFont(fontTableBody.FontFamily.Name, fontTableBody.Size - 1, fontTableBody.Style);

                        string[] wrappedText = WrapText(ref xGraphics, itemRow[i], fontTableBody, columnWidths[i] - 2);
                        const int lineHeight = 12;
                        int lineYOffset = 0;

                        foreach (var line in wrappedText)
                        {
                            var lineRect = new XRect(startX + GetColumnOffset(columnWidths, i), yPosition + lineYOffset, columnWidths[i] - 2, lineHeight);
                            xGraphics.DrawString(line, adjustedFont, XBrushes.Black, lineRect, XStringFormats.TopLeft);
                            lineYOffset += lineHeight;
                        }

                        maxRowHeight = Math.Max(maxRowHeight, 12 * wrappedText.Length);
                    }
                    else
                    {
                        xGraphics.DrawString(
                            itemRow[i],
                            fontTableBody,
                            XBrushes.Black,
                            new XRect(startX + GetColumnOffset(columnWidths, i), yPosition, columnWidths[i] - 2, rowItemHeight),
                            alignments[i]);
                    }

                    currentTextX += columnWidths[i];
                }

                yPosition += Math.Max(maxRowHeight, calcRawHeightByDescription);
                AddNewPageIfNecessary(pdfDoc, ref pdfPage, ref xGraphics, ref yPosition);
            }
        }
    }

    public void CreateTableWithTitle(PdfDocument pdfDoc, ref PdfPage pdfPage, ref XGraphics xGraphics, XmlDocument? xmlDoc, XmlNamespaceManager nsmgr, ITranslatorProvider translationProvider, Dictionary<string, string> fields, ref int yPosition, XFont fontColumnHeader, XFont fontTableBody, string lineItemsNode, string titleText, XStringFormat titleAlignment, string[] columnHeaders, int[] columnWidths, XStringFormat[] alignments)
    {
        int titleH = (int)Math.Ceiling(fontColumnHeader.GetHeight()) + 8;

        const int rowHeaderHeight = 12;
        const int rowItemHeight = 12;

        int tableH = rowHeaderHeight + 1;

        XmlNodeList? lineItems = xmlDoc?.SelectNodes(lineItemsNode, nsmgr);
        if (lineItems != null)
        {
            foreach (XmlNode lineItem in lineItems)
            {
                string[] itemRow = fields.Keys
                    .Select(xpath => lineItem.SelectSingleNode(xpath, nsmgr)?.InnerText ?? string.Empty)
                    .ToArray();

                string[] wrappedDescription = WrapText(ref xGraphics, itemRow[1], fontTableBody, columnWidths[1]);

                int rowH = rowItemHeight;
                if (wrappedDescription.Length >= 2)
                    rowH = wrappedDescription.Length * 12;

                tableH += rowH;
            }
        }

        int required = titleH + tableH + 6;

        AddNewPageIfNecessary(pdfDoc, ref pdfPage, ref xGraphics, ref yPosition, requiredHeight: required);

        DrawTableTitle(ref xGraphics, titleText, ref yPosition, titleAlignment);
        CreateSubTable(pdfDoc, ref pdfPage, ref xGraphics, xmlDoc, nsmgr, translationProvider,
            fields, ref yPosition, fontColumnHeader, fontTableBody, lineItemsNode, columnHeaders, columnWidths, alignments);
    }

    public void CreateInvoiceResultTable(PdfDocument pdfDoc, ref PdfPage pdfPage, ref XGraphics xGraphics, XmlDocument? xmlDoc, XmlNamespaceManager nsmgr, ITranslatorProvider translationProvider, Dictionary<string, string> fields, ref int yPosition, XFont fontColumnHeader, XFont fontTableBody, string currency, string boldFieldKey)
    {
        // Align to your main table width (40 + 405 + 110 = 555)
        const int startX = 40;
        const int mainTableWidth = 405 + 110;
        double mainTableRight = startX + mainTableWidth;

        // Compact layout
        const double boxWidth = 260;               // width of summary area
        const double padX = 6;
        const double rowH = 14;                   // smaller than before
        const double rowGap = 1;                  // tiny gap
        const double rightInnerPadding = 36;      // inportant moves amounts LEFT

        double boxX = mainTableRight - boxWidth;
        if (boxX < startX) boxX = startX; // safety

        XPen linePen = new XPen(_grayColor, 0.4);

        XFont totalFont;
        try { totalFont = new XFont(fontTableBody.FontFamily.Name, fontTableBody.Size + 1, XFontStyleEx.Bold); }
        catch { totalFont = fontTableBody; }

        // Measure needed height
        double neededH = fields.Count * (rowH + rowGap) + 2;
        AddNewPageIfNecessary(pdfDoc, ref pdfPage, ref xGraphics, ref yPosition, requiredHeight: (int)Math.Ceiling(neededH));

        // Inner columns
        double innerW = boxWidth - 2 * padX;
        double labelW = innerW * 0.62;
        double valueW = innerW - labelW;

        double y = yPosition;

        foreach (var kvp in fields)
        {
            AddNewPageIfNecessary(pdfDoc, ref pdfPage, ref xGraphics, ref yPosition);

            string xpath = kvp.Key;
            string fieldKey = kvp.Value;

            XmlNode? node = xmlDoc?.SelectSingleNode(xpath, nsmgr);
            string fieldValue = node != null ? node.InnerText : ContentNotFound;
            string fieldLabel = translationProvider.Translate(fieldKey, fieldKey);

            if (decimal.TryParse(fieldValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal amount))
                fieldValue = amount.ToString("N2", new CultureInfo("de-DE"));

            // ✅ 1) N/A not render
            if (string.IsNullOrWhiteSpace(fieldValue) ||
                fieldValue == ContentNotFound ||
                fieldValue.Equals("N/A", StringComparison.OrdinalIgnoreCase) ||
                fieldValue.Equals("n/a", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            bool isTotal = !string.IsNullOrWhiteSpace(boldFieldKey) &&
                           string.Equals(fieldKey, boldFieldKey, StringComparison.OrdinalIgnoreCase);

            // Horizontal separator
            xGraphics.DrawLine(linePen, boxX, y, boxX + boxWidth, y);

            if (isTotal)
            {
                var hl = new XRect(boxX, y, boxWidth, rowH);
                xGraphics.DrawRectangle(_blueBrushColor, hl);
            }

            var font = isTotal ? totalFont : fontTableBody;

            // Label (immer schwarz)
            xGraphics.DrawString(fieldLabel + ":", font, XBrushes.Black, new XRect(boxX + padX, y, labelW, rowH), XStringFormats.CenterLeft);


            XBrush valueBrush = isTotal ? _darkBlueBrushColor : _blackBrushColor;

            xGraphics.DrawString(fieldValue + " " + currency, font, valueBrush, new XRect(boxX + padX + labelW, y, valueW - rightInnerPadding, rowH), XStringFormats.CenterRight);

            y += rowH + rowGap;
        }

        // Bottom separator
        xGraphics.DrawLine(linePen, boxX, y, boxX + boxWidth, y);

        yPosition = (int)Math.Ceiling(y + 2);
    }

    public List<string> ReadLineNotes(XmlDocument xmlDoc, XmlNamespaceManager nsmgr, string notesNodeXpath, string contentRelativeXpath, string? subjectRelativeXpath = null, bool includeSubject = true, string[]? subjectFilter = null)
    {
        var result = new List<string>();

        HashSet<string>? filter = null;
        if (subjectFilter != null && subjectFilter.Length > 0)
            filter = new HashSet<string>(subjectFilter, StringComparer.OrdinalIgnoreCase);

        XmlNodeList? notes = xmlDoc.SelectNodes(notesNodeXpath, nsmgr);
        if (notes == null) return result;

        foreach (XmlNode note in notes)
        {
            string content = note.SelectSingleNode(contentRelativeXpath, nsmgr)?.InnerText?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(content))
                continue;

            content = content.Replace("\r", " ").Replace("\n", " ").Replace("\t", " ").Trim();

            string subject = "";
            if (!string.IsNullOrWhiteSpace(subjectRelativeXpath))
                subject = note.SelectSingleNode(subjectRelativeXpath, nsmgr)?.InnerText?.Trim() ?? "";

            if (filter != null)
            {
                if (string.IsNullOrWhiteSpace(subject) || !filter.Contains(subject))
                    continue;
            }

            if (includeSubject && !string.IsNullOrWhiteSpace(subjectRelativeXpath))
            {
                if (!string.IsNullOrWhiteSpace(subject))
                    result.Add($"{subject}: {content}");
                else
                    result.Add(content);
            }
            else
            {
                result.Add(content);
            }
        }

        return result;
    }

    public string ReadLineNotesAsText(XmlDocument xmlDoc, XmlNamespaceManager nsmgr, string notesNodeXpath, string contentRelativeXpath, string? subjectRelativeXpath = null, bool includeSubject = true, string separator = "\r\n", string[]? subjectFilter = null)
    {
        var lines = ReadLineNotes(xmlDoc, nsmgr, notesNodeXpath, contentRelativeXpath, subjectRelativeXpath, includeSubject, subjectFilter);

        if (lines == null || lines.Count == 0)
            return string.Empty;

        var cleanedLines = lines
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .ToList();

        return string.Join(separator, cleanedLines);
    }

    public string GetContentXNodesAsText(XmlNamespaceManager nsmgr, string xpath, XmlDocument? xmlDoc, string separator = "  ")
    {
        if (xmlDoc == null) return ContentNotFound;

        try
        {
            XmlNodeList? nodes = xmlDoc.SelectNodes(xpath, nsmgr);
            if (nodes == null || nodes.Count == 0)
            {
                GetXpathErrorMessage(xpath);
                return ContentNotFound;
            }

            var parts = new List<string>();

            foreach (XmlNode node in nodes)
            {
                var t = node?.InnerText?.Trim();
                if (!string.IsNullOrWhiteSpace(t))
                    parts.Add(t);
            }

            if (parts.Count == 0)
                return ContentNotFound;

            string joined = string.Join(separator, parts);
            return ParseDateTimeToRightFormat(xpath, joined);
        }
        catch (XPathException)
        {
            return xpath;
        }
    }
    public bool HasAnyNonZeroAmount(XmlDocument invoiceDoc, XmlNamespaceManager nsmgr, string amountXpath)
    {
        var nodes = invoiceDoc.SelectNodes(amountXpath, nsmgr);
        if (nodes == null || nodes.Count == 0)
            return false;

        foreach (XmlNode node in nodes)
        {
            var raw = (node?.InnerText ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(raw))
                continue;

            raw = raw.Replace(" ", "");

            if (decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var value) && value != 0m)
                return true;

            if (decimal.TryParse(raw, NumberStyles.Any, CultureInfo.GetCultureInfo("de-DE"), out value) && value != 0m)
                return true;
        }

        return false;
    }

    #region Common Utilities
    private string[] WrapMultilineCell(ref XGraphics g, string text, XFont font, int maxWidth)
    {
        if (string.IsNullOrWhiteSpace(text)) return Array.Empty<string>();

        // Normalize (also NBSP) + avoid double line breaks
        text = text.Replace("\u00A0", " ")
                   .Replace("\r\n", "\n")
                   .Replace("\r", "\n");

        // optional: combine multiple \n in a row
        while (text.Contains("\n\n"))
            text = text.Replace("\n\n", "\n");

        var parts = text.Split('\n');
        var lines = new List<string>();

        foreach (var p in parts)
        {
            var t = (p ?? string.Empty).Replace("\u00A0", " ").Trim();
            if (t.Length == 0) continue;

            var wrapped = WrapText(ref g, t, font, maxWidth);

            // <<< VERY IMPORTANT: filter empty lines out of WrapText
            foreach (var w in wrapped)
            {
                var ww = (w ?? string.Empty).Replace("\u00A0", " ").Trim();
                if (ww.Length == 0) continue;
                lines.Add(ww);
            }
        }

        return lines.ToArray();
    }

    private int GetColumnOffset(int[] columnWidths, int index)
    {
        int offset = 0;
        for (int i = 0; i < index; i++)
        {
            offset += columnWidths[i];
        }
        return offset;
    }

    private string CheckAndFormatIBAN(string iban)
    {
        string cleanIBAN = iban.Replace(" ", "").ToUpper();

        if (cleanIBAN.Length == 22)
        {
            if (Regex.IsMatch(iban, @"^([A-Z0-9]{4} ){5}[A-Z0-9]{2}$"))//check if IBAN Format is okay
            {
                return iban.ToUpper();
            }
            //if not, IBAN is formated
            var formattedIBAN = string.Join(" ", Enumerable.Range(0, (iban.Length + 3) / 4).Select(i => iban.Substring(i * 4, Math.Min(4, iban.Length - i * 4))));
            return formattedIBAN.ToUpper();
        }
        return iban;
    }

    private void ChangeSvgFillColor(SvgDocument svgDoc, XColor color)
    {
        // --- XColor -> System.Drawing.Color (robust für double 0..1 oder 0..255) ---
        int To255(double v)
        {
            if (v <= 1.0) return (int)Math.Round(v * 255.0);
            return (int)Math.Round(v);
        }

        int a = To255(color.A);
        int r = To255(color.R);
        int g = To255(color.G);
        int b = To255(color.B);

        var drawColor = System.Drawing.Color.FromArgb(a, r, g, b);

        // --- recurse through ALL svg elements and change fill for paths ---
        void ApplyFill(SvgElement element)
        {
            foreach (var child in element.Children)
            {
                if (child is SvgPath path)
                    path.Fill = new SvgColourServer(drawColor);

                if (child.Children != null && child.Children.Count > 0)
                    ApplyFill(child);
            }
        }

        ApplyFill(svgDoc);
    }

    private string NormalizeDecimalValue(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "0";

        int pointCount = input.Count(c => c == '.');
        int commaCount = input.Count(c => c == ',');

        if (pointCount > 1 && commaCount == 0)
        {
            int lastPointIndex = input.LastIndexOf('.');
            input = input.Remove(lastPointIndex).Replace(".", "") + input.Substring(lastPointIndex);
        }
        else if (pointCount == 1 && commaCount == 0)
        {
            //noting to do
        }
        else if (commaCount > 0)
        {
            input = input.Replace(".", "").Replace(",", ".");
        }

        if (decimal.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedValue))
        {
            return parsedValue.ToString("N2", CultureInfo.GetCultureInfo("de-DE"));
        }

        return "0";
    }

    private void DrawMultilineTextByBlanks(ref XGraphics xGraphics, string text, XFont font, XBrush brush, XRect rect, XStringFormat format)
    {
        var lines = text.Split(new[] { ' ' }, StringSplitOptions.None);
        double lineHeight = font.GetHeight();
        double currentY = rect.Top;

        foreach (var line in lines)
        {
            xGraphics.DrawString(line, font, brush, new XRect(rect.X, currentY, rect.Width, lineHeight), format);
            currentY += lineHeight;
        }
    }

    private void DrawMultilineByNewline(XGraphics gfx, string text, XFont font, XBrush brush, XRect rect, XStringFormat format)
    {
        // Normalize newlines
        string normalized = text.Replace("\r\n", "\n").Replace("\r", "\n");
        string[] paragraphs = normalized.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        double lineHeight = font.GetHeight();
        double y = rect.Top;

        foreach (var p in paragraphs)
        {
            string paragraph = p.Trim();
            if (paragraph.Length == 0) continue;

            // Wrap this paragraph into multiple lines by width
            foreach (var line in WrapLineByWidth(gfx, paragraph, font, rect.Width))
            {
                if (y + lineHeight > rect.Bottom)
                    break; // no more vertical space

                gfx.DrawString(line, font, brush, new XRect(rect.X, y, rect.Width, lineHeight), format);
                y += lineHeight;
            }
        }
    }

    private static IEnumerable<string> WrapLineByWidth(XGraphics gfx, string text, XFont font, double maxWidth)
    {
        // split by spaces (words), but we DO NOT render one-word-per-line;
        // we build lines that fit into maxWidth
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (words.Length == 0)
            yield break;

        string current = words[0];

        for (int i = 1; i < words.Length; i++)
        {
            string next = current + " " + words[i];
            double w = gfx.MeasureString(next, font).Width;

            if (w <= maxWidth)
            {
                current = next;
            }
            else
            {
                yield return current;

                // start new line with current word
                current = words[i];

                // edge case: one single word longer than maxWidth -> hard cut
                while (gfx.MeasureString(current, font).Width > maxWidth && current.Length > 1)
                {
                    int cut = current.Length;
                    while (cut > 1 && gfx.MeasureString(current.Substring(0, cut), font).Width > maxWidth)
                        cut--;

                    yield return current.Substring(0, cut);
                    current = current.Substring(cut).TrimStart();
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(current))
            yield return current;
    }

    private string[] WrapText(ref XGraphics xGraphics, string text, XFont font, double maxWidth)
    {
        string cleanedText = ReplaceLForCRorTabByEmptyString(text);
        List<string> lines = new List<string>();
        string[] words = cleanedText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        string currentLine = string.Empty;

        foreach (string word in words)
        {
            string testLine = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;
            double textWidth = xGraphics.MeasureString(testLine, font).Width;

            if (textWidth > maxWidth)
            {
                lines.Add(currentLine.Trim());
                currentLine = word;
            }
            else
            {
                currentLine = testLine;
            }
        }

        if (!string.IsNullOrEmpty(currentLine))
        {
            lines.Add(currentLine.Trim());
        }

        return lines.ToArray();
    }

    private static string ReplaceLForCRorTabByEmptyString(string text)
    {
        return text.Replace("\r", " ").Replace("\n", " ").Replace("\t", " ");
    }

    private void DrawText(XGraphics xGraphics, string text, XFont font, XBrush brush, double yPosition, double pageWidth, XStringFormat alignment, double xOffset = 0)
    {
        xGraphics.DrawString(text, font, brush, new XRect(xOffset, yPosition, pageWidth, 20), alignment);
    }

    private static string ParseDateTimeToRightFormat(string xpath, string fieldValue)
    {
        if (xpath.Contains("DateTimeString") && DateTime.TryParseExact(fieldValue, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
            fieldValue = parsedDate.ToString("dd.MM.yyyy");

        if (DateTime.TryParseExact(fieldValue, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime extendedParsedDate))
            fieldValue = extendedParsedDate.ToString("dd.MM.yyyy");

        return fieldValue;
    }

    private string CheckDifferrentsFieldKeys(ITranslatorProvider translationProvider, string xpath, string fieldKey, string fieldValue)
    {
        if (fieldKey == "VatCategoryCode")
        {
            string extraDescription = translationProvider.Translate(fieldValue, fieldValue); //extended from `de.xml`
            fieldValue = $"{fieldValue} {extraDescription}";
        }

        else if (xpath == "UBL" || xpath == "CII")
        {
            fieldValue = xpath;
        }

        else if (IsMatchingVersionPattern(xpath))
        {
            fieldValue = xpath;
        }

        return fieldValue;
    }

    private bool IsMatchingVersionPattern(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        string pattern = @"^v\d+\.\d+\.\d+$";
        var result = Regex.IsMatch(input, pattern);
        return result;
    }

    private static string CheckPaymentTypeCode(ITranslatorProvider translationProvider, string fieldKey, string fieldValue)
    {
        if (fieldKey == "PaymentTypeCode")
        {
            string extraDescription = translationProvider.Translate(fieldValue, fieldValue);
            fieldValue = $"{fieldValue} {extraDescription}";
        }

        return fieldValue;
    }

    private static void GetXpathErrorMessage(string xpath)
    {
        Debug.WriteLine($"Warning: the XPath '{xpath}' has not found a valid node .");
    }
    #endregion

    #region Footer & Page Numbers
    protected void ApplyFooterToAllPages(PdfDocument pdfDoc)
    {
        int total = pdfDoc.Pages.Count;

        for (int i = 0; i < total; i++)
        {
            PdfPage page = pdfDoc.Pages[i];

            using XGraphics xGraphics = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append);

            double pageWidth = page.Width.Point;
            double pageHeight = page.Height.Point;

            double footerHeight = FooterReserve;
            double footerTopY = pageHeight - footerHeight;

            double titleHeight = string.IsNullOrWhiteSpace(_currentFooterTitle) ? 0 : 12;
            const double pageNrHeight = 12;

            // Title (center)
            if (!string.IsNullOrWhiteSpace(_currentFooterTitle))
            {
                xGraphics.DrawString(_currentFooterTitle, fontTitleFooter, XBrushes.Gray, new XRect(40, footerTopY, pageWidth - 80, 12), XStringFormats.TopCenter);
            }

            // Text (center) - single line or multi line based on CR/LF
            if (!string.IsNullOrWhiteSpace(_currentFooterText))
            {
                var text = _currentFooterText.Trim();

                var textRect = new XRect(40, footerTopY + titleHeight + 2, pageWidth - 80, footerHeight - titleHeight - pageNrHeight - 6);

                if (text.Contains('\n') || text.Contains('\r'))
                {
                    DrawMultilineByNewline(xGraphics, text, fontFooter, XBrushes.Gray, textRect, XStringFormats.TopCenter);
                }
                else
                {
                    DrawWrappedFooterText(xGraphics, text, fontFooter, XBrushes.Gray, textRect);
                }
            }

            // Page number (bottom right)
            string pageNr = $"{i + 1}/{total}";
            xGraphics.DrawString(pageNr, fontFooter, XBrushes.Gray, new XRect(40, pageHeight - 18, pageWidth - 80, 12), XStringFormats.TopRight);
        }
    }

    private static void DrawWrappedFooterText(XGraphics gfx, string text, XFont font, XBrush brush, XRect rect)
    {
        text = string.Join(" ", text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

        var tf = new XTextFormatter(gfx)
        {
            Alignment = XParagraphAlignment.Center
        };

        tf.DrawString(text, font, brush, rect, XStringFormats.TopLeft);
    }
    #endregion

    #region EnsurePage
    private bool EnsurePage(PdfDocument pdfDoc, ref PdfPage page, ref XGraphics xGraphics, ref int y, int requiredHeight = 0, Action? onNewPage = null)
    {
        double pageWidth = page.Width.Point;
        double pageHeight = page.Height.Point;

        // Keep enough room for the footer and bottom margin.
        double limit = pageHeight - FooterReserve - PageBottomMargin;

        // If the next block fits, do nothing.
        if (y + requiredHeight <= limit)
            return false;
        // Footer is applied at the end via ApplyFooterToAllPages().
        xGraphics.Dispose();

        // Create and start a new page.
        page = pdfDoc.AddPage();
        page.Size = PageSize.A4;
        xGraphics = XGraphics.FromPdfPage(page);

        _pageNumber++;
        y = PageTopMargin;

        // Redraw the header on the new page.
        DrawHeader(ref xGraphics, _currentHeaderTitle, _currentHeaderText, ref y, page.Width.Point);
        y += 4;

        onNewPage?.Invoke();
        return true;
    }

    private void AddNewPageIfNecessary(PdfDocument pdfDoc, ref PdfPage pdfPage, ref XGraphics gfx, ref int yPosition, int requiredHeight = 0, Action? onNewPage = null)
        => EnsurePage(pdfDoc, ref pdfPage, ref gfx, ref yPosition, requiredHeight, onNewPage);

    private delegate void DrawOperation(PdfDocument pdfDoc, ref PdfPage pdfPage, ref XGraphics gfx, ref int yPosition);

    #endregion

    #region QR / Payment helpers (additive)
    protected void CreateQrCode(PdfDocument pdfDoc, ref PdfPage pdfPage, ref XGraphics xGraphics, XmlDocument? xmlDoc, XmlNamespaceManager nsmgr, ITranslatorProvider translationProvider, IEnumerable<string> xPaths4QrCode, ref int yPosition, XFont fontHeader, XFont fontBody, out XRect? qrRect, out PdfPage? qrOnPage)
    {
        qrRect = null;
        qrOnPage = null;

        if (xmlDoc == null) return;

        const double qrSizePoints = 72;
        const int qrPixelSize = 320;

        AddNewPageIfNecessary(pdfDoc, ref pdfPage, ref xGraphics, ref yPosition, requiredHeight: (int)qrSizePoints + 10);

        static string Clean(string? s) => string.IsNullOrWhiteSpace(s) ? string.Empty : s.Replace("\r", " ").Replace("\n", " ").Replace("\t", " ").Trim();

        // [0]=IBAN, [1]=BIC, [2]=Name, [3]=Currency, [4]=Amount, [5]=PaymentReference
        string[] qrRow = xPaths4QrCode.Select(xpath => Clean(xmlDoc.SelectSingleNode(xpath, nsmgr)?.InnerText)).ToArray();

        if (qrRow.Length < 6) return;

        string iban = (qrRow[0] ?? "").Replace(" ", "");
        string bic = (qrRow[1] ?? "").Replace(" ", "");
        string name = qrRow[2] ?? "";
        string currency = (qrRow[3] ?? "").ToUpperInvariant();
        string amountRaw = qrRow[4] ?? "";
        string remittance = qrRow[5] ?? "";

        if (string.IsNullOrWhiteSpace(iban) || string.IsNullOrWhiteSpace(name)) return;
        if (currency != "EUR") return;

        if (iban.Length > 34) iban = iban.Substring(0, 34);
        if (bic.Length > 11) bic = bic.Substring(0, 11);
        if (name.Length > 70) name = name.Substring(0, 70);
        if (remittance.Length > 140) remittance = remittance.Substring(0, 140);

        if (!decimal.TryParse(amountRaw, NumberStyles.Any, CultureInfo.InvariantCulture, out var a) &&
            !decimal.TryParse(amountRaw, NumberStyles.Any, CultureInfo.GetCultureInfo("de-DE"), out a))
            return;

        if (a <= 0) return;

        string amountLine = "EUR" + a.ToString("0.00", CultureInfo.InvariantCulture);
        var epcPayload = string.Join("\n", new[] { "BCD", "002", "1", "SCT", bic, name, iban, amountLine, "", remittance, "" });

        using var generator = new QRCodeGenerator();

        // Important because of logo in the middle: high error correction
        using var data = generator.CreateQrCode(epcPayload, QRCodeGenerator.ECCLevel.Q);

        var svgQr = new SvgQRCode(data);
        var svg = svgQr.GetGraphic(pixelsPerModule: 8);

        //Render SVG -> PNG and draw into PDF
        byte[] pngBytes = RenderSvgToPngBytes(svg, qrPixelSize);
        if (pngBytes.Length == 0) return;

        // ===== Logo in the center (from icons/TuloTeam.svg) =====
        SvgDocument? logoDoc = LoadSvgIconDocument("TuloTeam.svg");
        if (logoDoc != null)
        {
            ChangeSvgFillColor(logoDoc, _darkBlueColor);
            pngBytes = OverlayCenterLogoOnQrPngBytes(pngBytes, logoDoc, qrPixelSize);
        }
        // =====================================================

        const double qrOffsetLeft = 14;
        const double qrOffsetDown = 18;

        double x = pdfPage.Width.Point - PageRightMargin - qrSizePoints - qrOffsetLeft;
        double y = yPosition + qrOffsetDown;

        using var img = XImage.FromStream(new MemoryStream(pngBytes));
        img.Interpolate = false;
        xGraphics.DrawImage(img, x, y, qrSizePoints, qrSizePoints);

        qrRect = new XRect(x, y, qrSizePoints, qrSizePoints);
        qrOnPage = pdfPage;
    }

    private static byte[] RenderSvgToPngBytes(string svgString, int pixelSize)
    {
        using var svgStream = new MemoryStream(Encoding.UTF8.GetBytes(svgString));
        var svgDoc = SvgDocument.Open<SvgDocument>(svgStream);

        using var bitmap = svgDoc.Draw(pixelSize, pixelSize);
        using var ms = new MemoryStream();
        bitmap.Save(ms, format: ImageFormat.Png);
        return ms.ToArray();
    }

    private static byte[] OverlayCenterLogoOnQrPngBytes(byte[] qrPngBytes, SvgDocument logoSvgDoc, int qrPixelSize)
    {
        int logoSize = (int)(qrPixelSize * 0.14);
        if (logoSize < 24) logoSize = 24;

        using var qrMs = new MemoryStream(qrPngBytes);
        using var qrBmp = new System.Drawing.Bitmap(qrMs);

        using var logoBmp = logoSvgDoc.Draw(logoSize, logoSize);

        using (var g = System.Drawing.Graphics.FromImage(qrBmp))
        {
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

            int cx = (qrBmp.Width - logoSize) / 2;
            int cy = (qrBmp.Height - logoSize) / 2;

            // White background behind the logo (scanner-stable)
            int pad = (int)(logoSize * 0.1);
            var bgRect = new System.Drawing.Rectangle(cx - pad, cy - pad, logoSize + 2 * pad, logoSize + 2 * pad);

            using var whiteBrush = new System.Drawing.SolidBrush(System.Drawing.Color.White);
            g.FillRectangle(whiteBrush, bgRect);

            g.DrawImage(logoBmp, new System.Drawing.Rectangle(cx, cy, logoSize, logoSize));
        }

        using var outMs = new MemoryStream();
        qrBmp.Save(outMs, ImageFormat.Png);
        return outMs.ToArray();
    }
    #endregion

    #region SVG Utilities
    private SvgDocument? LoadSvgIconDocument(string iconFileName)
    {
        SvgDocument? svgDoc = null;

        string svgIconPathStr = Path.Combine(_iconsPath, iconFileName);
        if (File.Exists(svgIconPathStr))
        {
            svgDoc = SvgDocument.Open(svgIconPathStr);
        }
        else
        {
            var assembly = Assembly.GetExecutingAssembly();
            string baseNamespace = assembly.GetName().Name!;
            string resourceNamePath = $"{baseNamespace}.{_iconsPath}.{iconFileName}";

            using Stream? stream = assembly.GetManifestResourceStream(resourceNamePath);
            if (stream == null)
            {
                Trace.WriteLine($"Warnung: Ressource '{resourceNamePath}' nicht gefunden.");
                return null;
            }

            svgDoc = SvgDocument.Open<SvgDocument>(stream);
        }

        return svgDoc;
    }

    private void DrawSvgIconIfAvailable(ref XGraphics xGraphics, Dictionary<string, string> svgIconNames, string iconKey, double x, int y)
    {
        if (!svgIconNames.TryGetValue(iconKey, out string? iconFileName))
            return;

        SvgDocument? svgDoc = null;

        string svgIconPathStr = Path.Combine(_iconsPath, iconFileName);
        if (File.Exists(svgIconPathStr))
        {
            svgDoc = SvgDocument.Open(svgIconPathStr);
        }
        else
        {
            var assembly = Assembly.GetExecutingAssembly();
            string baseNamespace = assembly.GetName().Name!;
            string resourceNamePath = $"{baseNamespace}.{_iconsPath}.{iconFileName}";

            using Stream? stream = assembly.GetManifestResourceStream(resourceNamePath);
            if (stream == null)
            {
                Trace.WriteLine($"Warnung: Ressource '{resourceNamePath}' nicht gefunden.");
                return;
            }
            svgDoc = SvgDocument.Open<SvgDocument>(stream);
        }

        if (svgDoc == null)
            return;

        ChangeSvgFillColor(svgDoc, _darkBlueColor);

        using var bitmap = svgDoc.Draw(256, 256);
        using var ms = new MemoryStream();
        bitmap.Save(ms, ImageFormat.Png);
        ms.Seek(0, SeekOrigin.Begin);
        using XImage xImage = XImage.FromStream(ms);
        xImage.Interpolate = false;
        xGraphics.DrawImage(xImage, x, y, 10, 10);
    }
    #endregion
}
