using Microsoft.Extensions.Options;
using System.Collections.ObjectModel;
using tulo.CommonMVVM.Collector;
using tulo.CommonMVVM.ViewModels;
using tulo.CoreLib.Translators;
using tulo.eInvoice.eInvoiceApp.Options;

namespace tulo.eInvoice.eInvoiceApp.ViewModels.Sellers;

public class SellerViewModel : BaseViewModel
{
    #region Get Services / Stores from CollectorCollection
    private readonly IOptions<AppOptions> _appOptions;
    private readonly ITranslatorUiProvider _translatorUiProvider;
    #endregion

    #region Seller Party
    private string _erpCustomerNumberSellerParty = string.Empty;
    public string ErpCustomerNumberSellerParty
    {
        get => _erpCustomerNumberSellerParty;
        set => SetField(ref _erpCustomerNumberSellerParty, value);
    }

    private string _fiscalIdSellerParty = string.Empty;
    public string FiscalIdSellerParty
    {
        get => _fiscalIdSellerParty;
        set => SetField(ref _fiscalIdSellerParty, value);
    }

    private string _vatIdSellerParty = string.Empty;
    public string VatIdSellerParty
    {
        get => _vatIdSellerParty;
        set => SetField(ref _vatIdSellerParty, value);
    }

    private string _leitwegIdSellerParty = string.Empty;
    public string LeitwegIdSellerParty
    {
        get => _leitwegIdSellerParty;
        set => SetField(ref _leitwegIdSellerParty, value);
    }

    private string _companySellerParty = string.Empty;
    public string CompanySellerParty
    {
        get => _companySellerParty;
        set => SetField(ref _companySellerParty, value);
    }

    private string _personSellerParty = string.Empty;
    public string PersonSellerParty
    {
        get => _personSellerParty;
        set => SetField(ref _personSellerParty, value);
    }

    private string _addressSellerParty = string.Empty;
    public string AddressSellerParty
    {
        get => _addressSellerParty;
        set => SetField(ref _addressSellerParty, value);
    }

    private string _phoneSellerParty = string.Empty;
    public string PhoneSellerParty
    {
        get => _phoneSellerParty;
        set => SetField(ref _phoneSellerParty, value);
    }

    private string _emailAddressSellerParty = string.Empty;
    public string EmailAddressSellerParty
    {
        get => _emailAddressSellerParty;
        set => SetField(ref _emailAddressSellerParty, value);
    }
    #endregion

    #region Invoice Notes
    private string _subjectCodeInvoiceNote = "REG";
    public string SubjectCodeInvoiceNote
    {
        get => _subjectCodeInvoiceNote;
        set => SetField(ref _subjectCodeInvoiceNote, value);
    }

    private string _contentInvoiceNote = string.Empty;
    public string ContentInvoiceNote
    {
        get => _contentInvoiceNote;
        set => SetField(ref _contentInvoiceNote, value);
    }

    public ObservableCollection<InvoiceNote> InvoiceNoteItems { get; } = new();
    #endregion

    #region Payment
    private string _bankNameSellerParty = string.Empty;
    public string BankNameSellerParty
    {
        get => _bankNameSellerParty;
        set => SetField(ref _bankNameSellerParty, value);
    }

    private string _accountHolderSellerParty = string.Empty;
    public string AccountHolderSellerParty
    {
        get => _accountHolderSellerParty;
        set => SetField(ref _accountHolderSellerParty, value);
    }

    private string _ibanSellerParty = string.Empty;
    public string IbanSellerParty
    {
        get => _ibanSellerParty;
        set => SetField(ref _ibanSellerParty, value);
    }

    private string _bicSellerParty = string.Empty;
    public string BicSellerParty
    {
        get => _bicSellerParty;
        set => SetField(ref _bicSellerParty, value);
    }
    #endregion

    public SellerViewModel(ICollectorCollection collectorCollection)
    {
        #region Get Services / Stores from CollectorCollection
        _appOptions = collectorCollection.GetService<IOptions<AppOptions>>();
        _translatorUiProvider = collectorCollection.GetService<ITranslatorUiProvider>();
        #endregion

        FillAllSellerToolTips();
        FillAllSellerPlaceholders();
        FillAllSellerContents();

        MapperInvoiceSellerOptions(_appOptions.Value);
        MapperInvoicePaymentOptions(_appOptions.Value);
        MapperInvoiceNotesOptions(_appOptions.Value);
    }

    private void MapperInvoiceSellerOptions(IAppOptions appOptions)
    {
        var appOptionsSeller = appOptions.Invoice?.Seller;

        if (appOptionsSeller == null) return;

        ErpCustomerNumberSellerParty = appOptionsSeller.ID ?? string.Empty;
        FiscalIdSellerParty = appOptionsSeller.FiscalId ?? string.Empty;
        VatIdSellerParty = appOptionsSeller.VatId ?? string.Empty;
        LeitwegIdSellerParty = appOptionsSeller.LeitwegId ?? string.Empty;
        CompanySellerParty = appOptionsSeller.Name ?? string.Empty;
        PersonSellerParty = appOptionsSeller.ContactPersonName ?? string.Empty;
        AddressSellerParty = BuildAddress(appOptionsSeller.Street, appOptionsSeller.Zip, appOptionsSeller.City, appOptionsSeller.CountryCode);
        PhoneSellerParty = appOptionsSeller.ContactPhone ?? string.Empty;
        EmailAddressSellerParty = !string.IsNullOrWhiteSpace((string)appOptionsSeller.ContactEmail)
            ? appOptionsSeller.ContactEmail!
            : (appOptionsSeller.GeneralEmail ?? string.Empty);
    }

    private string BuildAddress(string? street, string? zip, string? city, string? countryCode) => string.Join(", ", new[]
     {
        (street ?? string.Empty).Trim(),
        string.Join(" ", new[] { (zip ?? string.Empty).Trim(), (city ?? string.Empty).Trim() }.Where(s => !string.IsNullOrWhiteSpace(s))).Trim()
     }
     .Where(s => !string.IsNullOrWhiteSpace(s))
     .Select((s, i) => i == 1 && !string.IsNullOrWhiteSpace((countryCode ?? string.Empty).Trim()) ? $"{s}, {(countryCode ?? string.Empty).Trim()}" : s));

    private void MapperInvoicePaymentOptions(IAppOptions appOptions)
    {
        var pay = appOptions.Invoice?.Payment;
        if (pay == null) return;

        // BankName does not exist directly in your JSON → I do NOT map BIC to BankName.
        // If you have BankName somewhere else, replace it here.
        BankNameSellerParty = string.Empty;

        AccountHolderSellerParty = pay.AccountName ?? string.Empty;
        IbanSellerParty = pay.Iban ?? string.Empty;
        BicSellerParty = pay.Bic ?? string.Empty;
    }

    private void MapperInvoiceNotesOptions(IAppOptions appOptions)
    {
        var notes = appOptions.Invoice.Notes;

        InvoiceNoteItems.Clear();
        if (notes == null) return;

        foreach (var n in notes)
        {
            InvoiceNoteItems.Add(new InvoiceNote
            {
                SubjectCodeInvoiceNote = string.IsNullOrWhiteSpace(n.SubjectCode) ? "REG" : n.SubjectCode,
                ContentInvoiceNote = n.Content ?? string.Empty,
                ToolTipSubjectCodeInvoiceNote = ToolTipSubjectCodeInvoiceNote,
                ToolTipContentInvoiceNote = ToolTipContentInvoiceNote,
                PlaceholderContentInvoiceNote = PlaceholderContentInvoiceNote
            });
        }
    }

    public class InvoiceNote
    {
        public string? ContentInvoiceNote { get; set; }
        public string? SubjectCodeInvoiceNote { get; set; }
        public string ToolTipSubjectCodeInvoiceNote { get; set; } = string.Empty;
        public string ToolTipContentInvoiceNote { get; set; } = string.Empty;
        public string PlaceholderContentInvoiceNote { get; set; } = string.Empty;
    }

    #region ToolTips
    // Seller 
    public string ToolTipCompanySellerParty { get; private set; } = string.Empty;
    public string ToolTipFiscalIdSellerParty { get; private set; } = string.Empty;
    public string ToolTipVatIdSellerParty { get; private set; } = string.Empty;
    public string ToolTipErpCustomerNumberSellerParty { get; private set; } = string.Empty;
    public string ToolTipLeitwegIdSellerParty { get; private set; } = string.Empty;

    public string ToolTipPersonSellerParty { get; private set; } = string.Empty;
    public string ToolTipAddressSellerParty { get; private set; } = string.Empty;

    public string ToolTipPhoneSellerParty { get; private set; } = string.Empty;
    public string ToolTipEmailAddressSellerParty { get; private set; } = string.Empty;

    // Bank connection
    public string ToolTipBankNameSellerParty { get; set; } = string.Empty;
    public string ToolTipIbanSellerParty { get; private set; } = string.Empty;
    public string ToolTipBicSellerParty { get; private set; } = string.Empty;
    public string ToolTipAccountHolderSellerParty { get; private set; } = string.Empty;

    //invoice notes
    public string ToolTipSubjectCodeInvoiceNote { get; set; } = string.Empty;
    public string ToolTipContentInvoiceNote { get; private set; } = string.Empty;
    private void FillAllSellerToolTips()
    {
        // Seller ToolTips
        ToolTipCompanySellerParty = _translatorUiProvider.Translate("ToolTipCompanySellerParty");
        ToolTipFiscalIdSellerParty = _translatorUiProvider.Translate("ToolTipFiscalIdSellerParty");
        ToolTipVatIdSellerParty = _translatorUiProvider.Translate("ToolTipVatIdSellerParty");
        ToolTipErpCustomerNumberSellerParty = _translatorUiProvider.Translate("ToolTipErpCustomerNumberSellerParty");
        ToolTipLeitwegIdSellerParty = _translatorUiProvider.Translate("ToolTipLeitwegIdSellerParty");

        ToolTipPersonSellerParty = _translatorUiProvider.Translate("ToolTipPersonSellerParty");
        ToolTipAddressSellerParty = _translatorUiProvider.Translate("ToolTipAddressSellerParty");
        ToolTipPhoneSellerParty = _translatorUiProvider.Translate("ToolTipPhoneSellerParty");
        ToolTipEmailAddressSellerParty = _translatorUiProvider.Translate("ToolTipEmailAddressSellerParty");

        // Bank connection ToolTips
        ToolTipBankNameSellerParty = _translatorUiProvider.Translate("ToolTipBankNameSellerParty");
        ToolTipIbanSellerParty = _translatorUiProvider.Translate("ToolTipIbanSellerParty");
        ToolTipBicSellerParty = _translatorUiProvider.Translate("ToolTipBicSellerParty");
        ToolTipAccountHolderSellerParty = _translatorUiProvider.Translate("ToolTipAccountHolderSellerParty");

        // Invoice Notes ToolTips
        ToolTipSubjectCodeInvoiceNote = _translatorUiProvider.Translate("ToolTipSubjectCodeInvoiceNote");
        ToolTipContentInvoiceNote = _translatorUiProvider.Translate("ToolTipContentInvoiceNote");
    }
    #endregion

    #region Placeholders
    // Seller - Placeholders 
    public string PlaceholderCompanySellerParty { get; private set; } = string.Empty;
    public string PlaceholderVatIdSellerParty { get; private set; } = string.Empty;
    public string PlaceholderFiscalIdSellerParty { get; private set; } = string.Empty;
    public string PlaceholderErpCustomerNumberSellerParty { get; private set; } = string.Empty;
    public string PlaceholderLeitwegIdSellerParty { get; private set; } = string.Empty;

    public string PlaceholderPersonSellerParty { get; private set; } = string.Empty;
    public string PlaceholderAddressSellerParty { get; private set; } = string.Empty;


    public string PlaceholderPhoneSellerParty { get; private set; } = string.Empty;
    public string PlaceholderEmailAddressSellerParty { get; private set; } = string.Empty;

    // Bank connection - Placeholders (one-way, initialized once)
    public string PlaceholderBankNameSellerParty { get; private set; } = string.Empty;
    public string PlaceholderIbanSellerParty { get; private set; } = string.Empty;
    public string PlaceholderBicSellerParty { get; private set; } = string.Empty;
    public string PlaceholderAccountHolderSellerParty { get; private set; } = string.Empty;

    // Invoice Notes
    public string PlaceholderContentInvoiceNote { get; private set; } = string.Empty;

    private void FillAllSellerPlaceholders()
    {
        // Seller Placeholders
        PlaceholderCompanySellerParty = _translatorUiProvider.Translate("PlaceholderCompanySellerParty");
        PlaceholderVatIdSellerParty = _translatorUiProvider.Translate("PlaceholderVatIdSellerParty");
        PlaceholderFiscalIdSellerParty = _translatorUiProvider.Translate("PlaceholderFiscalIdSellerParty");
        PlaceholderErpCustomerNumberSellerParty = _translatorUiProvider.Translate("PlaceholderErpCustomerNumberSellerParty");
        PlaceholderLeitwegIdSellerParty = _translatorUiProvider.Translate("PlaceholderLeitwegIdSellerParty");

        PlaceholderPersonSellerParty = _translatorUiProvider.Translate("PlaceholderPersonSellerParty");
        PlaceholderAddressSellerParty = _translatorUiProvider.Translate("PlaceholderAddressSellerParty");
        PlaceholderPhoneSellerParty = _translatorUiProvider.Translate("PlaceholderPhoneSellerParty");
        PlaceholderEmailAddressSellerParty = _translatorUiProvider.Translate("PlaceholderEmailAddressSellerParty");

        // Bank connection Placeholders
        PlaceholderBankNameSellerParty = _translatorUiProvider.Translate("PlaceholderBankNameSellerParty");
        PlaceholderIbanSellerParty = _translatorUiProvider.Translate("PlaceholderIbanSellerParty");
        PlaceholderBicSellerParty = _translatorUiProvider.Translate("PlaceholderBicSellerParty");
        PlaceholderAccountHolderSellerParty = _translatorUiProvider.Translate("PlaceholderAccountHolderSellerParty");

        // Invoice Notes Placeholders
        PlaceholderContentInvoiceNote = _translatorUiProvider.Translate("PlaceholderContentInvoiceNote");
    }
    #endregion

    #region Contents
    public string ContentSellerInformation { get; set; } = string.Empty;
    public string ContenBankAccountDetails { get; set; } = string.Empty;
    public string ContentInvoiceNotes { get; set; } = string.Empty;

    private void FillAllSellerContents()
    {
        // Seller Contents
        ContentSellerInformation = _translatorUiProvider.Translate("ContentSellerInformation");
        ContenBankAccountDetails = _translatorUiProvider.Translate("ContenBankAccountDetails");
        ContentInvoiceNotes = _translatorUiProvider.Translate("ContentInvoiceNotes");
    }
    #endregion
}
