using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Input;
using tulo.CommonMVVM.Collector;
using tulo.CommonMVVM.Commands;
using tulo.CommonMVVM.GlobalProperties;
using tulo.CommonMVVM.ViewModels;
using tulo.eInvoice.eInvoiceApp.Commands;
using tulo.eInvoice.eInvoiceApp.Commands.Invoices;
using tulo.eInvoice.eInvoiceApp.DTOs;
using tulo.eInvoice.eInvoiceApp.Options;
using tulo.eInvoice.eInvoiceApp.Services;
using tulo.eInvoice.eInvoiceApp.Stores.Invoices;
using tulo.eInvoiceXmlGeneratorCii.Models;
using tulo.ResourcesWpfLib.Commands;
using tulo.ResourcesWpfLib.Viewmodels;

namespace tulo.eInvoice.eInvoiceApp.ViewModels.Invoices;

public class InvoiceViewModel : BaseViewModel
{
    #region Services / Stores filled via CollectorCollection
    private readonly IGlobalPropsUiManage _globalPropsUiManage;
    private readonly IInvoicePositionService _invoicePositionService;
    private readonly ISelectedInvoicePositionStore _selectedInvoicePositionStore;
    private readonly ICollectorCollection _collectorCollection;
    //private readonly IRenavigationService _renavServiceEmployeCardList;
    private readonly IAppOptions _appOptions;
    #endregion

    public Invoice Invoice { get; private set; } = new Invoice();

    public double? NormalWidthBeforePreview { get; set; } = 740;

    #region InvoicePositions
    private readonly ObservableCollection<InvoicePositionCardItemViewModel> _invoicePositionCardListItemViewModel;
    public ICollectionView InvoicePositionCardListItemCollectionView { get; set; }

    public InvoicePositionCardItemViewModel SelectedInvoicePositionCardListItemViewModel
    {
        get
        {
            var selected = _invoicePositionCardListItemViewModel;
            if (selected == null || !_selectedInvoicePositionStore.SelectedInvoicePositionId.HasValue) return null!;

            return _invoicePositionCardListItemViewModel.FirstOrDefault(invPos => invPos.InvoicePositionId == _selectedInvoicePositionStore.SelectedInvoicePositionId)!;
        }
        set
        {
            _selectedInvoicePositionStore.SelectedInvoicePositionId = value?.InvoicePositionId;
            _selectedInvoicePositionStore.SelectedInvoicePosition = value?.InvoicePositionDetails!;
            HasSelectedInvoicePosition = _selectedInvoicePositionStore.SelectedInvoicePosition != null;

            OnPropertyChanged(nameof(SelectedInvoicePositionCardListItemViewModel));
            OnPropertyChanged(nameof(HasSelectedInvoicePosition));
        }
    }
    #endregion

    #region Invoice Header
    private string _invoiceNumber = string.Empty;
    public string InvoiceNumber
    {
        get => _invoiceNumber;
        set => SetField(ref _invoiceNumber, value);
    }

    private string _currency = string.Empty;
    public string Currency
    {
        get => _currency;
        set => SetField(ref _currency, value);
    }

    private string _documentName = string.Empty;
    public string DocumentName
    {
        get => _documentName;
        set => SetField(ref _documentName, value);
    }

    private string _documentTypeCode = string.Empty;
    public string DocumentTypeCode
    {
        get => _documentTypeCode;
        set => SetField(ref _documentTypeCode, value);
    }
    #endregion

    #region Buyer Party

    private string _companyBuyerParty = string.Empty;
    public string CompanyBuyerParty
    {
        get => _companyBuyerParty;
        set => SetField(ref _companyBuyerParty, value);
    }

    private string _fiscalIdBuyerParty = string.Empty;
    public string FiscalIdBuyerParty
    {
        get => _fiscalIdBuyerParty;
        set => SetField(ref _fiscalIdBuyerParty, value);
    }

    private string _vatIdBuyerParty = string.Empty;
    public string VatIdBuyerParty
    {
        get => _vatIdBuyerParty;
        set => SetField(ref _vatIdBuyerParty, value);
    }

    private string _erpCustomerNumberBuyerParty = string.Empty;
    public string ErpCustomerNumberBuyerParty
    {
        get => _erpCustomerNumberBuyerParty;
        set => SetField(ref _erpCustomerNumberBuyerParty, value);
    }

    private string _leitwegIdBuyerParty = string.Empty;
    public string LeitwegIdBuyerParty
    {
        get => _leitwegIdBuyerParty;
        set => SetField(ref _leitwegIdBuyerParty, value);
    }

    private string _personBuyerParty = string.Empty;
    public string PersonBuyerParty
    {
        get => _personBuyerParty;
        set => SetField(ref _personBuyerParty, value);
    }

    private string _streetBuyerParty = string.Empty;
    public string StreetBuyerParty
    {
        get => _streetBuyerParty;
        set => SetField(ref _streetBuyerParty, value);
    }

    private string _houseNumberBuyerParty = string.Empty;
    public string HouseNumberBuyerParty
    {
        get => _houseNumberBuyerParty;
        set => SetField(ref _houseNumberBuyerParty, value);
    }

    private string _postalCodeBuyerParty = string.Empty;
    public string PostalCodeBuyerParty
    {
        get => _postalCodeBuyerParty;
        set => SetField(ref _postalCodeBuyerParty, value);
    }

    private string _cityBuyerParty = string.Empty;
    public string CityBuyerParty
    {
        get => _cityBuyerParty;
        set => SetField(ref _cityBuyerParty, value);
    }

    private string _countryCodeBuyerParty = string.Empty;
    public string CountryCodeBuyerParty
    {
        get => _countryCodeBuyerParty;
        set => SetField(ref _countryCodeBuyerParty, value);
    }

    private string _phoneBuyerParty = string.Empty;
    public string PhoneBuyerParty
    {
        get => _phoneBuyerParty;
        set => SetField(ref _phoneBuyerParty, value);
    }

    private string _emailAddressBuyerParty = string.Empty;
    public string EmailAddressBuyerParty
    {
        get => _emailAddressBuyerParty;
        set => SetField(ref _emailAddressBuyerParty, value);
    }
    #endregion

    #region Payment Infos
    private string _paymentMeansCode = string.Empty;
    public string PaymentMeansCode
    {
        get => _paymentMeansCode;
        set => SetField(ref _paymentMeansCode, value);
    }


    private string _paymentReference = string.Empty;
    public string PaymentReference
    {
        get => _paymentReference;
        set => SetField(ref _paymentReference, value);
    }

    private string _paymentTerms = string.Empty;
    public string PaymentTerms
    {
        get => _paymentTerms;
        set => SetField(ref _paymentTerms, value);
    }

    private static readonly CultureInfo _de = CultureInfo.GetCultureInfo("de-DE");
    private static readonly DateTime _minDate = new(1900, 1, 1);
    private static readonly DateTime _maxDate = new(2099, 12, 31);
    private const string DateFormat = "dd.MM.yyyy";

    private DateOnly? _paymentDueDate;
    public DateOnly? PaymentDueDate
    {
        get => _paymentDueDate;
        set => SetField(ref _paymentDueDate, value);
    }

    private string _paymentDueDateText = string.Empty;
    public string PaymentDueDateText
    {
        get => _paymentDueDateText;
        set
        {
            if (_paymentDueDateText == value) return;
            _paymentDueDateText = value;
            OnPropertyChanged(nameof(PaymentDueDateText));

            HasDatePickerError = false;
            DatePickerErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(value))
            {
                PaymentDueDate = null;
                return;
            }

            if (value.Length == 10 && DateTime.TryParseExact(value, DateFormat, _de, DateTimeStyles.None, out var dt))
            {
                if (dt.Date < _minDate || dt.Date > _maxDate)
                {
                    PaymentDueDate = null;
                    HasDatePickerError = true;
                    DatePickerErrorMessage = $"Date must be between {_minDate.ToString(DateFormat, _de)} & {_maxDate.ToString(DateFormat, _de)}.";

                    return;
                }

                PaymentDueDate = DateOnly.FromDateTime(dt);
            }
            else
            {
                HasDatePickerError = true;
                DatePickerErrorMessage = "Date is invalid.";
            }
        }
    }

    private bool _hasDatePickerError;
    public bool HasDatePickerError
    {
        get => _hasDatePickerError;
        set => SetField(ref _hasDatePickerError, value);
    }

    private string _datePikerErrorMessage = string.Empty;
    public string DatePickerErrorMessage
    {
        get => _datePikerErrorMessage;
        set => SetField(ref _datePikerErrorMessage, value);
    }


    #endregion

    #region Local Properties
    private bool _hasSelectedInvoicePosition;
    public bool HasSelectedInvoicePosition
    {
        get => _hasSelectedInvoicePosition;
        set => SetField(ref _hasSelectedInvoicePosition, value);
    }
    #endregion

    #region Pdf Preview
    private bool _isPreviewEnabled;
    public bool IsPreviewEnabled
    {
        get => _isPreviewEnabled;
        set => SetField(ref _isPreviewEnabled, value);
    }

    private string _documentSource = string.Empty;
    public string DocumentSource
    {
        get => _documentSource;
        set => SetField(ref _documentSource, value);
    }

    private bool _resetSlideButton;
    public bool ResetSlideButton
    {
        get => _resetSlideButton;
        set => SetField(ref _resetSlideButton, value);
    }
    #endregion

    #region UI Control Properties + IUiControlPropsViewModel
    private bool _isEnabledSaveRequestInUI;
    public bool IsEnabledSaveRequestInUI
    {
        get => _isEnabledSaveRequestInUI;
        set => SetField(ref _isEnabledSaveRequestInUI, value);
    }

    private bool _isAltShortcutKeyPressed;
    public bool IsAltShortcutKeyPressed
    {
        get => _isAltShortcutKeyPressed;
        set => SetField(ref _isAltShortcutKeyPressed, value);
    }

    private bool _isShortcutKeyAlreadyPressed;
    public bool IsAltShortcutKeyAlreadyPressed
    {
        get => _isShortcutKeyAlreadyPressed;
        set => SetField(ref _isShortcutKeyAlreadyPressed, value);
    }

    private bool _isDuplicate;
    public bool IsDuplicate
    {
        get => _isDuplicate;
        set => SetField(ref _isDuplicate, value);
    }

    public MessageViewModel StatusMessageViewModel { get; }
    public string StatusMessage
    {
        set => StatusMessageViewModel.Message = value;
    }

    public static string SelectedViewModel => nameof(InvoiceViewModel);
    public string CurrentViewModelName => SelectedViewModel;
    #endregion

    #region Commands
    public ICommand UpdatePreviewInvoicePdfCommand { get; }
    public ICommand OpenAddInvoicePositionViewCommand { get; }
    public ICommand LoadInvoicePositionsCommand { get; }
    public ICommand CreateElectronicInvoiceComponentsCommand { get; }

    #endregion

    #region Common Commands
    public ICommand CloseSpinnerMessageCommand { get; }
    public ICommand OpenSpinnerMessageCommand { get; }
    public ICommand RequestBringIntoViewCommand { get; }
    public ICommand IsAltShortcutKeyReleasedCommand { get; }
    public ICommand IsAltShortcutKeyPressedCommand { get; }
    #endregion

    public InvoiceViewModel(ICollectorCollection collectorCollection)
    {
        _collectorCollection = collectorCollection;

        #region Get Services / Stores from CollectorCollection
        _selectedInvoicePositionStore = collectorCollection.GetService<ISelectedInvoicePositionStore>();
        _invoicePositionService = collectorCollection.GetService<IInvoicePositionService>();
        _globalPropsUiManage = collectorCollection.GetService<IGlobalPropsUiManage>();
        //_renavServiceEmployeCardList = collectorCollection.GetService<IRenavigationService<EmployeeCardListViewModel>>();
        _appOptions = collectorCollection.GetService<IAppOptions>();
        #endregion

        _invoicePositionCardListItemViewModel = [];
        InvoicePositionCardListItemCollectionView = CollectionViewSource.GetDefaultView(_invoicePositionCardListItemViewModel);

        StatusMessageViewModel = new MessageViewModel();

        UpdatePreviewInvoicePdfCommand = new UpdatePreviewInvoicePdfCommand(this, _collectorCollection);
        OpenAddInvoicePositionViewCommand = new OpenModalStackCommand(collectorCollection, () => new AddInvoicePositionViewModel(_collectorCollection), typeof(AddInvoicePositionViewModel));

        #region Common Commands
        OpenSpinnerMessageCommand = new OpenModalStackCommand(collectorCollection, () => new SpinnerMessageViewModel(), typeof(SpinnerMessageViewModel));
        CloseSpinnerMessageCommand = new CloseModalStackCommand(collectorCollection, typeof(SpinnerMessageViewModel));
        RequestBringIntoViewCommand = new RequestBringIntoViewCommand();
        IsAltShortcutKeyReleasedCommand = new IsAltShortcutKeyReleasedCommand(collectorCollection);
        IsAltShortcutKeyPressedCommand = new IsAltShortcutKeyPressedCommand(collectorCollection);
        #endregion

        LoadInvoicePositionsCommand = new LoadInvoicePositionsCommand(this, _collectorCollection);
        CreateElectronicInvoiceComponentsCommand = new CreateElectronicInvoiceComponentsCommand(this, _collectorCollection);

        _selectedInvoicePositionStore!.SelectedInvoicePositionChanged += OnSelectedInvoicePositionChanged;
        _invoicePositionCardListItemViewModel.CollectionChanged += OnInvoicePositionCollectionChanged;

        _invoicePositionService.InvoicePositionCreated += OnInvoicePositionCreated;
        _invoicePositionService.InvoicePositionUpdated += OnInvoicePositionUpdated;
        _invoicePositionService.InvoicePositionDeleted += OnInvoicePositionDeleted;

        _invoicePositionService.InvoicePositionsLoaded += OnInvoicePositionsLoaded;

        FillAllInvoiceToolTips();
        FillAllInvoicePlaceholders();
        FillAllInvoiceLabelsAndContents();

        //Only for UI Tests
        SeedTestSellerData();
        SeedTestInvoicePositions();
    }

    private void OnInvoicePositionCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(SelectedInvoicePositionCardListItemViewModel));
    }

    private void OnSelectedInvoicePositionChanged()
    {
        HasSelectedInvoicePosition = _selectedInvoicePositionStore.SelectedInvoicePosition != null;
        OnPropertyChanged(nameof(SelectedInvoicePositionCardListItemViewModel));
    }

    private void AddInvoicePostion(InvoicePositionDetailsDTO invPosDetailsDTO, ICollectorCollection collectorCollection)
    {
        InvoicePositionCardItemViewModel invoicePositionCardItemViewModel = new(invPosDetailsDTO, collectorCollection);
        _invoicePositionCardListItemViewModel.Add(invoicePositionCardItemViewModel);
    }

    private void OnInvoicePositionsLoaded(List<InvoicePositionDetailsDTO> invoicePositions)
    {
        _invoicePositionCardListItemViewModel.Clear();

        foreach (var invoicePosition in invoicePositions)
        {
            AddInvoicePostion(invoicePosition, _collectorCollection);
        }

        InvoicePositionCardListItemCollectionView.Refresh();
    }

    private void OnInvoicePositionCreated(InvoicePositionDetailsDTO invoicePositionDetailsDTO)
    {
        var invPosViewModel = new InvoicePositionCardItemViewModel(invoicePositionDetailsDTO, _collectorCollection);

        // 2) Add to the UI list (so that it can be displayed at all)
        _invoicePositionCardListItemViewModel.Add(invPosViewModel);

        // 3) Select (uses YOUR setter -> writes to SelectedStore)
        SelectedInvoicePositionCardListItemViewModel = invPosViewModel;

        OnSelectedInvoicePositionChanged();
    }

    private void OnInvoicePositionUpdated(InvoicePositionDetailsDTO invoicePositionDetailsDTO)
    {
        var existingItemViewModel = _invoicePositionCardListItemViewModel
            .FirstOrDefault(invPos => invPos.InvoicePositionId == invoicePositionDetailsDTO.Id);

        if (existingItemViewModel == null) return;

        existingItemViewModel.Update(invoicePositionDetailsDTO);
        InvoicePositionCardListItemCollectionView.Refresh();

    }

    private void OnInvoicePositionDeleted(Guid id)
    {
        var existingItemViewModel = _invoicePositionCardListItemViewModel
            .FirstOrDefault(x => x.InvoicePositionId == id);

        if (existingItemViewModel != null)
            _invoicePositionCardListItemViewModel.Remove(existingItemViewModel);

        // if deleted item was selected -> clear selection
        if (_selectedInvoicePositionStore.SelectedInvoicePositionId == id)
        {
            SelectedInvoicePositionCardListItemViewModel = null!;
            OnSelectedInvoicePositionChanged();
        }

        InvoicePositionCardListItemCollectionView.Refresh();
    }

    public static InvoiceViewModel LoadInvoiceViewModel(ICollectorCollection collectorCollection)
    {
        InvoiceViewModel invoiceViewModel = new(collectorCollection);
        invoiceViewModel.LoadInvoicePositionsCommand.Execute(null);
        return invoiceViewModel;
    }

    #region Tooltips
    public string ToolTipCompanyBuyerParty { get; private set; } = string.Empty;
    public string ToolTipFiscalIdBuyerParty { get; private set; } = string.Empty;
    public string ToolTipVatIdBuyerParty { get; private set; } = string.Empty;
    public string ToolTipErpCustomerNumberBuyerParty { get; private set; } = string.Empty;
    public string ToolTipLeitwegIdBuyerParty { get; private set; } = string.Empty;
    public string ToolTipPersonBuyerParty { get; private set; } = string.Empty;
    public string ToolTipStreetBuyerParty { get; private set; } = string.Empty;
    public string ToolTipHouseNumberBuyerParty { get; private set; } = string.Empty;
    public string ToolTipPostalCodeBuyerParty { get; private set; } = string.Empty;
    public string ToolTipCityBuyerParty { get; private set; } = string.Empty;
    public string ToolTipCountryCodeBuyerParty { get; private set; } = string.Empty;
    public string ToolTipPhoneBuyerParty { get; private set; } = string.Empty;
    public string ToolTipEmailAddressBuyerParty { get; private set; } = string.Empty;

    public string ToolTipPaymentMeansCode { get; private set; } = string.Empty;
    public string ToolTipPaymentDueDateText { get; private set; } = string.Empty;
    public string ToolTipPaymentReference { get; private set; } = string.Empty;
    public string ToolTipPaymentTerms { get; private set; } = string.Empty;

    //header
    public string ToolTipInvoiceNumber { get; private set; } = string.Empty;
    public string ToolTipCurrency { get; private set; } = string.Empty;
    public string ToolTipDocumentName { get; private set; } = string.Empty;
    public string ToolTipDocumentTypeCode { get; private set; } = string.Empty;

    public string ToolTipCreatePreviewElectronicInvoice { get; private set; } = string.Empty;
    public string ToolTipCleanUpInvoiceView { get; private set; } = string.Empty;
    public string ToolTipCreateElectronicInvoice { get; private set; } = string.Empty;

    private void FillAllInvoiceToolTips()
    {
        // Buyer Party tooltips
        ToolTipCompanyBuyerParty = "Buyer company / legal name.";
        ToolTipFiscalIdBuyerParty = "Buyer fiscal ID / tax number (often schemeID=FC).";
        ToolTipVatIdBuyerParty = "Buyer VAT ID (often schemeID=VA).";
        ToolTipErpCustomerNumberBuyerParty = "Internal customer number (ERP reference).";
        ToolTipLeitwegIdBuyerParty = "Leitweg-ID (required for German XRechnung public sector).";
        ToolTipPersonBuyerParty = "Contact person full name.";
        ToolTipStreetBuyerParty = "Street name (without house number if separated).";
        ToolTipHouseNumberBuyerParty = "House number (if separated).";
        ToolTipPostalCodeBuyerParty = "ZIP / postal code.";
        ToolTipCityBuyerParty = "City.";
        ToolTipCountryCodeBuyerParty = "Country code (e.g. DE).";
        ToolTipPhoneBuyerParty = "Contact phone number.";
        ToolTipEmailAddressBuyerParty = "Contact email address.";

        // Payment tooltips
        ToolTipPaymentMeansCode = "Payment means type code (e.g. 58 = SEPA credit transfer).";
        ToolTipPaymentDueDateText = "Payment due date (text input).";
        ToolTipPaymentReference = "Payment reference / remittance information.";
        ToolTipPaymentTerms = "Payment terms / conditions text.";

        //header
        ToolTipInvoiceNumber = "Unique invoice number of the document.";
        ToolTipCurrency = "Currency as ISO 4217 code (e.g., EUR, USD, CHF).";
        ToolTipDocumentName = "Document display name (e.g., INVOICE).";
        ToolTipDocumentTypeCode = "Document type code according to UNTDID 1001 (e.g., 380 = invoice, 381 = credit note).";

        ToolTipCreatePreviewElectronicInvoice = "Create a preview of the electronic invoice.";
        ToolTipCleanUpInvoiceView = "Clear data from te inovice view.";
        ToolTipCreateElectronicInvoice = "Create and export the electronic PdfA3 invoice.";
    }
    #endregion

    #region Placeholders
    public string PlaceholderCompanyBuyerParty { get; private set; } = string.Empty;
    public string PlaceholderFiscalIdBuyerParty { get; private set; } = string.Empty;
    public string PlaceholderVatIdBuyerParty { get; private set; } = string.Empty;
    public string PlaceholderErpCustomerNumberBuyerParty { get; private set; } = string.Empty;
    public string PlaceholderLeitwegIdBuyerParty { get; private set; } = string.Empty;
    public string PlaceholderPersonBuyerParty { get; private set; } = string.Empty;
    public string PlaceholderStreetBuyerParty { get; private set; } = string.Empty;
    public string PlaceholderHouseNumberBuyerParty { get; private set; } = string.Empty;
    public string PlaceholderPostalCodeBuyerParty { get; private set; } = string.Empty;
    public string PlaceholderCityBuyerParty { get; private set; } = string.Empty;
    public string PlaceholderCountryCodeBuyerParty { get; private set; } = string.Empty;
    public string PlaceholderPhoneBuyerParty { get; private set; } = string.Empty;
    public string PlaceholderEmailAddressBuyerParty { get; private set; } = string.Empty;

    public string PlaceholderPaymentReference { get; private set; } = string.Empty;
    public string PlaceholderPaymentTerms { get; private set; } = string.Empty;

    public string PlaceholderInvoiceNumber { get; private set; } = string.Empty;
    public string PlaceholderCurrency { get; private set; } = string.Empty;
    public string PlaceholderDocumentName { get; private set; } = string.Empty;
    public string PlaceholderDocumentTypeCode { get; private set; } = string.Empty;

    private void FillAllInvoicePlaceholders()
    {
        // Buyer Party placeholders
        PlaceholderCompanyBuyerParty = "Company Name";
        PlaceholderFiscalIdBuyerParty = "Tax number (schemeID=FC)";
        PlaceholderVatIdBuyerParty = "VAT ID (schemeID=VA)";
        PlaceholderErpCustomerNumberBuyerParty = "ERP Customer-Nr.";
        PlaceholderLeitwegIdBuyerParty = "Leitweg-ID";
        PlaceholderPersonBuyerParty = "Person Fullname";
        PlaceholderStreetBuyerParty = "Street";
        PlaceholderHouseNumberBuyerParty = "Nr.";
        PlaceholderPostalCodeBuyerParty = "Zip code";
        PlaceholderCityBuyerParty = "City";
        PlaceholderCountryCodeBuyerParty = "Country";
        PlaceholderPhoneBuyerParty = "Phone";
        PlaceholderEmailAddressBuyerParty = "E-Mail Address";

        // Payment placeholders
        PlaceholderPaymentReference = "Payment reference";
        PlaceholderPaymentTerms = "Payment terms";

        //header
        PlaceholderInvoiceNumber = "Invoice nr.";
        PlaceholderDocumentName = "Document name";

    }
    #endregion

    #region Labels & Contents
    public string LabelPaymentDueDate { get; private set; } = string.Empty;
    public string LabelPaymentMeansCode { get; private set; } = string.Empty;

    public string LabelCurrency { get; private set; } = string.Empty;
    public string LabelDocumentTypeCode { get; private set; } = string.Empty;

    public string LabelContentInvoiceView { get; private set; } = string.Empty;
    public string LabelContentPreview { get; private set; } = string.Empty;
    public string ContentSlideText { get; private set; } = string.Empty;
    public string ContentSlideConfirmedText { get; private set; } = string.Empty;
    public string LabelContenBuyerInformation { get; private set; } = string.Empty;
    public string LabelContentHeader { get; private set; } = string.Empty;
    public string LabelContentPaymentInformation { get; private set; } = string.Empty;
    public string LabelContentPositionsList { get; private set; } = string.Empty;

    private void FillAllInvoiceLabelsAndContents()
    {
        LabelPaymentDueDate = "Due Date";
        LabelPaymentMeansCode = "Payment means code";

        LabelCurrency = "Currency";
        LabelDocumentTypeCode = "Document type";

        LabelContentInvoiceView = "Invoice View";
        LabelContentPreview = "Preview";
        ContentSlideText = "Create";
        ContentSlideConfirmedText = "Created";
        LabelContenBuyerInformation = "Buyer Information";
        LabelContentHeader = "Header";
        LabelContentPaymentInformation = "Payment Information";
        LabelContentPositionsList = "Positions List";
    }
    #endregion

    #region Dispose
    public override void Dispose()
    {
        //_globalProps4UiControl.IsAltKeyPressedChanged -= OnIsAltKeyIsChanged_IsAltKeyPressedChanged;

        _selectedInvoicePositionStore.SelectedInvoicePositionChanged -= OnSelectedInvoicePositionChanged;

        _invoicePositionService.InvoicePositionCreated -= OnInvoicePositionCreated;
        _invoicePositionService.InvoicePositionUpdated -= OnInvoicePositionUpdated;
        _invoicePositionService.InvoicePositionDeleted -= OnInvoicePositionDeleted;
        _invoicePositionService.InvoicePositionsLoaded -= OnInvoicePositionsLoaded;

        base.Dispose();
    }
    #endregion

    #region Only for UI Test
    private void SeedTestSellerData()
    {
        InvoiceNumber = "6063636771001";
        Currency = "EUR";
        DocumentName = "RECHNUNG";
        DocumentTypeCode = "380";

        CompanyBuyerParty = "Musterkunde GmbH & Co Name 2 Musterkunde";
        FiscalIdBuyerParty = "77777/01234";
        VatIdBuyerParty = "DE2012129398";
        ErpCustomerNumberBuyerParty = "9900880077";
        LeitwegIdBuyerParty = "04011000-1234512345-35";
        PersonBuyerParty = "Herr Test Monteur";

        StreetBuyerParty = "Musterstrasse";
        HouseNumberBuyerParty = "44";
        PostalCodeBuyerParty = "40789";
        CityBuyerParty = "Musterstadt";
        CountryCodeBuyerParty = "DE";
        PhoneBuyerParty = "02173 9364";
        EmailAddressBuyerParty = "mike.maier@lieferant.com";

        PaymentMeansCode = "58";
        PaymentReference = "Kundennummer:. 9900880077 Rechnungsnummer:. 6063636771001";
        PaymentTerms = "Zahlbar innerhalb von 14 Tagen ohne Abzug.";
        PaymentDueDate = new DateOnly(2026, 9, 16);
        PaymentDueDateText = "16.09.2026";
    }

    private void SeedTestInvoicePositions()
    {
        _invoicePositionService.AddInvoicePositionAsync(new InvoicePositionDetailsDTO
        {
            InvoicePositionNr = 1,
            InvoicePositionDescription = "GWDSTG-DIN976-A-4.8-(A2K)-M10X1000",
            InvoicePositionProductDescription = "Gewindestange",
            InvoicePositionItemNr = "0595810 25",
            InvoicePositionEan = "7711231873598",
            InvoicePositionQuantity = 25m,
            InvoicePostionUnit = "H87",
            InvoicePositionUnitPrice = 2.06m,
            InvoicePositionVatRate = 19,
            InvoicePositionVatCategoryCode = "S",
            InvoicePositionNetAmount = 0m,
            InvoicePositionGrossAmount = 0m,
            InvoicePositionDiscountReason = string.Empty,
            InvoicePositionDiscountNetAmount = 0m,
            InvoicePositionNetAmountAfterDiscount = null,
            InvoicePositionOrderDate = new DateOnly(2026, 8, 27),
            InvoicePositionOrderId = "Abholung 1",
            InvoicePositionDeliveryNoteDate = new DateOnly(2026, 8, 27),
            InvoicePositionDeliveryNoteId = "8408230045",
            InvoicePositionDeliveryNoteLineId = "000010",
            InvoicePositionRefDocId = "2156307416",
            InvoicePositionRefDocType = "130",
            InvoicePositionRefDocRefType = "VN",
            InvoicePositionSelectedVatCategory = null
        }).GetAwaiter().GetResult();

        _invoicePositionService.AddInvoicePositionAsync(new InvoicePositionDetailsDTO
        {
            InvoicePositionNr = 2,
            InvoicePositionDescription = "MUELLSACK-EXTRASTARK-BLAU-700X1100X0,07",
            InvoicePositionProductDescription = "Müllsack, -beutel",
            InvoicePositionItemNr = "05899800555 150",
            InvoicePositionEan = "7748539263943",
            InvoicePositionQuantity = 150m,
            InvoicePostionUnit = "H87",
            InvoicePositionUnitPrice = 49.29m,
            InvoicePositionVatRate = 19,
            InvoicePositionVatCategoryCode = "S",
            InvoicePositionNetAmount = 0m,
            InvoicePositionGrossAmount = 0m,
            InvoicePositionDiscountReason = string.Empty,
            InvoicePositionDiscountNetAmount = 0m,
            InvoicePositionNetAmountAfterDiscount = null,
            InvoicePositionOrderDate = new DateOnly(2026, 8, 27),
            InvoicePositionOrderId = "Abholung 1",
            InvoicePositionDeliveryNoteDate = new DateOnly(2026, 8, 27),
            InvoicePositionDeliveryNoteId = "8408230045",
            InvoicePositionDeliveryNoteLineId = "000020",
            InvoicePositionRefDocId = "2156307416",
            InvoicePositionRefDocType = "130",
            InvoicePositionRefDocRefType = "VN",
            InvoicePositionSelectedVatCategory = null
        }).GetAwaiter().GetResult();

        _invoicePositionService.AddInvoicePositionAsync(new InvoicePositionDetailsDTO
        {
            InvoicePositionNr = 3,
            InvoicePositionDescription = "SHR-AW30-(A2K)-7,5X152",
            InvoicePositionProductDescription = "Abstandsmontageschraube Rahmen",
            InvoicePositionItemNr = "05234830152 200",
            InvoicePositionEan = "7738898142591",
            InvoicePositionQuantity = 400m,
            InvoicePostionUnit = "H87",
            InvoicePositionUnitPrice = 32.76m,
            InvoicePositionVatRate = 19,
            InvoicePositionVatCategoryCode = "S",
            InvoicePositionNetAmount = 0m,
            InvoicePositionGrossAmount = 0m,
            InvoicePositionDiscountReason = string.Empty,
            InvoicePositionDiscountNetAmount = 0m,
            InvoicePositionNetAmountAfterDiscount = null,
            InvoicePositionOrderDate = new DateOnly(2026, 8, 27),
            InvoicePositionOrderId = "Abholung 2",
            InvoicePositionDeliveryNoteDate = new DateOnly(2026, 8, 27),
            InvoicePositionDeliveryNoteId = "8408230046",
            InvoicePositionDeliveryNoteLineId = "000010",
            InvoicePositionRefDocId = "2156307417",
            InvoicePositionRefDocType = "130",
            InvoicePositionRefDocRefType = "VN",
            InvoicePositionSelectedVatCategory = null
        }).GetAwaiter().GetResult();
    }
    #endregion
}