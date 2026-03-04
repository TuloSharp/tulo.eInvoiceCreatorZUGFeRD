using System.Collections.ObjectModel;
using tulo.CommonMVVM.Collector;
using tulo.CommonMVVM.ViewModels;
using tulo.eInvoice.eInvoiceApp.Options;
using tulo.LoadingSpinnerControl.ViewModels;

namespace tulo.eInvoice.eInvoiceApp.ViewModels.Sellers;

public class SellerViewModel : BaseViewModel
{
    //private readonly ICollectorCollection _collectorCollection;
    private readonly IAppOptions _appOptions;

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
        //_collectorCollection = collectorCollection;
        _appOptions = collectorCollection.GetService<IAppOptions>();

        FillAllSellerToolTips();
        FillAllSellerPlaceholders();
     
        MapperInvoiceSellerOptions(_appOptions);
        MapperInvoicePaymentOptions(_appOptions);
        MapperInvoiceNotesOptions(_appOptions);
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
        ToolTipCompanySellerParty = "Seller company / legal name.";
        ToolTipFiscalIdSellerParty = "Seller fiscal ID / tax number (often schemeID=FC).";
        ToolTipVatIdSellerParty = "Seller VAT ID (often schemeID=VA).";
        ToolTipErpCustomerNumberSellerParty = "Seller ERP customer number (if applicable).";
        ToolTipLeitwegIdSellerParty = "Leitweg-ID (if required).";

        ToolTipPersonSellerParty = "Contact person full name.";
        ToolTipAddressSellerParty = "Full address (street, house number, postal code, city, country).";
        ToolTipPhoneSellerParty = "Contact phone number.";
        ToolTipEmailAddressSellerParty = "Contact email address.";

        // Tooltips Payment
        ToolTipBankNameSellerParty = "Bank name (if available).";
        ToolTipIbanSellerParty = "Seller IBAN for payments.";
        ToolTipBicSellerParty = "Seller BIC / SWIFT.";
        ToolTipAccountHolderSellerParty = "Account holder name.";

        //Invoice Notes
        ToolTipSubjectCodeInvoiceNote = "Note Subject Code (i.e. REG/AAI/PTM)";
        ToolTipContentInvoiceNote = "Note text / description";
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
        PlaceholderCompanySellerParty = "Company Name";
        PlaceholderVatIdSellerParty = "VAT ID (schemeID=VA)";
        PlaceholderFiscalIdSellerParty = "Tax number (schemeID=FC)";
        PlaceholderErpCustomerNumberSellerParty = "ERP Customer Number";
        PlaceholderLeitwegIdSellerParty = "Leitweg-ID";

        PlaceholderPersonSellerParty = "Person Fullname";
        PlaceholderAddressSellerParty = "Full Address (street, house number, postal code, city, country)";
        PlaceholderPhoneSellerParty = "Phone";
        PlaceholderEmailAddressSellerParty = "E-Mail Address";

        PlaceholderBankNameSellerParty = "Bank Name";
        PlaceholderIbanSellerParty = "IBAN";
        PlaceholderBicSellerParty = "BIC";
        PlaceholderAccountHolderSellerParty = "Account name";

        PlaceholderContentInvoiceNote = "Type your invoice note here...";
    }
    #endregion
}
