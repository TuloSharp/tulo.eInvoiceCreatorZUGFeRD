using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using tulo.CommonMVVM.Collector;
using tulo.CommonMVVM.Commands;
using tulo.CommonMVVM.GlobalProperties;
using tulo.CommonMVVM.ViewModels;
using tulo.CoreLib.Interfaces.SnapShots;
using tulo.CoreLib.Translators;
using tulo.eInvoice.eInvoiceApp.Commands.Invoices;
using tulo.eInvoice.eInvoiceApp.DTOs;
using tulo.eInvoice.eInvoiceApp.Options;
using tulo.eInvoice.eInvoiceApp.Stores.Invoices;

namespace tulo.eInvoice.eInvoiceApp.ViewModels.Invoices;
public class InvoicePositionDetailsFormViewModel : BaseViewModel
{
    #region Services / Stores filled via CollectorCollection
    private readonly ILogger<InvoicePositionDetailsFormViewModel> _logger;
    private readonly IAppOptions _appOptions;
    private readonly ISnapShotService _snapShot;
    private readonly IGlobalPropsUiManage _globalPropsUiManage;
    private readonly ITranslatorUiProvider _translatorUiProvider;
    #endregion

    private readonly ISelectedInvoicePositionStore? _selectedInvoicePositionStore;
    private InvoicePositionDetailsDTO SelectedInvoicePositionCard => _selectedInvoicePositionStore!.SelectedInvoicePosition;
    public bool HasSelectedInvoicePositionCard => SelectedInvoicePositionCard is not null;

    private readonly InvoicePositionDetailsDTO? _tmpSelectedInvPos;

    private void UpdateToEnableSaveControl()
    {
        if (_tmpSelectedInvPos == null)
        {
            _globalPropsUiManage.HasUnsavedChanges = false;
            _globalPropsUiManage.IsSaveRequestMessageVisible = false;
            _globalPropsUiManage.IsEnableToSaveData = false;
            IsEnableToSaveData = false;
            return;
        }
        // Check if porperty had changed
        bool hasChanges = !string.Equals(_tmpSelectedInvPos.InvoicePositionDescription?.Trim(), InvoicePositionDescription?.Trim(), StringComparison.Ordinal) ||
                          _tmpSelectedInvPos.InvoicePositionQuantity != InvoicePositionQuantity ||
                          !string.Equals(_tmpSelectedInvPos.InvoicePostionUnit?.Trim(), InvoicePostionUnit?.Trim(), StringComparison.Ordinal) ||
                          _tmpSelectedInvPos.InvoicePositionUnitPrice != InvoicePositionUnitPrice ||
                          _tmpSelectedInvPos.InvoicePositionVatRate != InvoicePositionVatRate ||
                          //_tmpSelectedInvPos.InvoicePositionNetAmount != InvoicePositionNetAmount ||
                          _tmpSelectedInvPos.InvoicePositionGrossAmount != InvoicePositionGrossAmount ||
                          !string.Equals(_tmpSelectedInvPos.InvoicePositionDiscountReason?.Trim(), InvoicePositionDiscountReason?.Trim(), StringComparison.Ordinal) ||
                          _tmpSelectedInvPos.InvoicePositionDiscountNetAmount != InvoicePositionDiscountNetAmount;
        //_tmpSelectedInvPos.InvoicePositionNetAmountAfterDiscount != InvoicePositionNetAmountAfterDiscount;
        // Active on changes
        _globalPropsUiManage.HasUnsavedChanges = hasChanges;
        IsEnableToSaveData = hasChanges;
        _globalPropsUiManage.IsEnableToSaveData = IsEnableToSaveData;

    }

    #region Local Properties
    private bool _isLoading;
    public bool IsLoading
    {
        get => _globalPropsUiManage.IsLoading;
        set
        {
            if (!SetField(ref _isLoading, value)) return;
            if (_globalPropsUiManage.IsLoading != value)
                _globalPropsUiManage.IsLoading = value;
        }
    }

    //private bool _isEnableAdditionalButton;
    //public bool IsEnableAdditionalButton
    //{
    //    get => _isEnableAdditionalButton;
    //    set => SetField(ref _isEnableAdditionalButton, value);
    //}

    private bool _isInvalidStateAtInputField;
    public bool IsInvalidStateAtInputField
    {
        get => _isInvalidStateAtInputField;
        set => SetField(ref _isInvalidStateAtInputField, value);
    }

    private bool _isEnable4ShowButtons;
    public bool IsEnable4ShowButtons
    {
        get => _isEnable4ShowButtons;
        set => SetField(ref _isEnable4ShowButtons, value);
    }
    #endregion

    #region Invoice Position Details Properties
    public Guid Id { get; set; }

    private int _invoicePositionNr;
    public int InvoicePositionNr
    {
        get => _invoicePositionNr;
        set => SetField(ref _invoicePositionNr, value);
    }

    private string _invoicePositionDescription = string.Empty;
    public string InvoicePositionDescription
    {
        get => _invoicePositionDescription;
        set
        {
            if (SetField(ref _invoicePositionDescription, value))
                UpdateToEnableSaveControl();
        }
    }

    private string _invoicePositionProductDescription = string.Empty;
    public string InvoicePositionProductDescription
    {
        get => _invoicePositionProductDescription;
        set => SetField(ref _invoicePositionProductDescription, value);
    }

    private string _invoicePositionItemNr = string.Empty;
    public string InvoicePositionItemNr
    {
        get => _invoicePositionItemNr;
        set => SetField(ref _invoicePositionItemNr, value);
    }

    private string _invoicePositionEan = string.Empty;
    public string InvoicePositionEan
    {
        get => _invoicePositionEan;
        set => SetField(ref _invoicePositionEan, value);
    }

    private decimal _invoicePositionQuantity;
    public decimal InvoicePositionQuantity
    {
        get => _invoicePositionQuantity;
        set
        {
            if (SetField(ref _invoicePositionQuantity, value))
                UpdateToEnableSaveControl();
        }
    }



    private decimal _invoicePositionUnitPrice;
    public decimal InvoicePositionUnitPrice
    {
        get => _invoicePositionUnitPrice;
        set
        {
            if (SetField(ref _invoicePositionUnitPrice, value))
                UpdateToEnableSaveControl();
        }
    }

    private int _invoicePositionVatRate;
    public int InvoicePositionVatRate
    {
        get => _invoicePositionVatRate;
        set
        {
            if (SetField(ref _invoicePositionVatRate, value))
                UpdateToEnableSaveControl();
        }
    }

    private decimal _invoicePositionNetAmount;
    public decimal InvoicePositionNetAmount
    {
        get => _invoicePositionNetAmount;
        set => SetField(ref _invoicePositionNetAmount, value);
    }

    private decimal _invoicePositionGrossAmount;
    public decimal InvoicePositionGrossAmount
    {
        get => _invoicePositionGrossAmount;
        set
        {
            if (SetField(ref _invoicePositionGrossAmount, value))
                UpdateToEnableSaveControl();
        }
    }

    private string _invoicePositionDiscountReason = string.Empty;
    public string InvoicePositionDiscountReason
    {
        get => _invoicePositionDiscountReason;
        set
        {
            if (SetField(ref _invoicePositionDiscountReason, value))
                UpdateToEnableSaveControl();
        }
    }

    private decimal _invoicePositionDiscountNetAmount;
    public decimal InvoicePositionDiscountNetAmount
    {
        get => _invoicePositionDiscountNetAmount;
        set
        {
            if (SetField(ref _invoicePositionDiscountNetAmount, value))
                UpdateToEnableSaveControl();
        }
    }

    private decimal? _invoicePositionNetAmountAfterDiscount;
    public decimal? InvoicePositionNetAmountAfterDiscount
    {
        get => _invoicePositionNetAmountAfterDiscount;
        set => SetField(ref _invoicePositionNetAmountAfterDiscount, value);
    }

    private DateOnly? _invoicePositionOrderDate;
    public DateOnly? InvoicePositionOrderDate
    {
        get => _invoicePositionOrderDate;
        set => SetField(ref _invoicePositionOrderDate, value);
    }

    private string _invoicePositionOrderDateText = string.Empty;
    public string InvoicePositionOrderDateText
    {
        get => _invoicePositionOrderDateText;
        set => SetField(ref _invoicePositionOrderDateText, value);
    }

    private string _invoicePositionOrderId = string.Empty;
    public string InvoicePositionOrderId
    {
        get => _invoicePositionOrderId;
        set => SetField(ref _invoicePositionOrderId, value);
    }

    private string _invoicePositionDeliveryNoteId = string.Empty;
    public string InvoicePositionDeliveryNoteId
    {
        get => _invoicePositionDeliveryNoteId;
        set => SetField(ref _invoicePositionDeliveryNoteId, value);
    }

    private string _invoicePositionDeliveryNoteLineId = string.Empty;
    public string InvoicePositionDeliveryNoteLineId
    {
        get => _invoicePositionDeliveryNoteLineId;
        set => SetField(ref _invoicePositionDeliveryNoteLineId, value);
    }

    private DateOnly? _invoicePositionDeliveryNoteDate;
    public DateOnly? InvoicePositionDeliveryNoteDate
    {
        get => _invoicePositionDeliveryNoteDate;
        set => SetField(ref _invoicePositionDeliveryNoteDate, value);
    }

    private string _invoicePositionDeliveryNoteDateText = string.Empty;
    public string InvoicePositionDeliveryNoteDateText
    {
        get => _invoicePositionDeliveryNoteDateText;
        set => SetField(ref _invoicePositionDeliveryNoteDateText, value);
    }

    private string _invoicePositionRefDocId = string.Empty;
    public string InvoicePositionRefDocId
    {
        get => _invoicePositionRefDocId;
        set => SetField(ref _invoicePositionRefDocId, value);
    }

    private string _invoicePositionRefDocType = string.Empty;
    public string InvoicePositionRefDocType
    {
        get => _invoicePositionRefDocType;
        set => SetField(ref _invoicePositionRefDocType, value);
    }

    private string _invoicePositionRefDocRefType = string.Empty;
    public string InvoicePositionRefDocRefType
    {
        get => _invoicePositionRefDocRefType;
        set => SetField(ref _invoicePositionRefDocRefType, value);
    }

    #endregion

    #region MessageViewModels
    public MessageViewModel StatusMessage { get; }
    #endregion

    #region Manage Enable Buttons in UI
    //private bool _isEnableToSaveData;
    //public bool IsEnableToSaveData
    //{
    //    get => _isEnableToSaveData;
    //    set
    //    {
    //        if (!SetField(ref _isEnableToSaveData, value)) return;

    //        if (SubmitCommand is AsyncBaseCommand asyncCommand)
    //            asyncCommand.RaiseCanExecuteChanged();
    //    }
    //}

    private bool _isEnableToSaveData;
    public bool IsEnableToSaveData
    {
        get => _globalPropsUiManage.IsEnableToSaveData;
        set
        {
            if (!SetField(ref _isEnableToSaveData, value)) return;
            if (_globalPropsUiManage.IsEnableToSaveData != value)
                _globalPropsUiManage.IsEnableToSaveData = value;
        }
    }
    //private bool _isEnableToSaveData;
    //public bool IsEnableToSaveData
    //{
    //    get => _isEnableToSaveData;
    //    set => SetField(ref _isEnableToSaveData, value);
    //}

    private bool _isRequiredField;
    public bool IsRequiredField
    {
        get => _globalPropsUiManage.IsRequiredField;
        set
        {
            if (!SetField(ref _isRequiredField, value)) return;
            if (_globalPropsUiManage.IsRequiredField != value)
                _globalPropsUiManage.IsRequiredField = value;
        }
    }
    #endregion

    #region Request Message Controls
    //ShowSaveRequestMessageControl
    private bool _isSaveRequestMessageVisible;
    public bool IsSaveRequestMessageVisible
    {
        get => _globalPropsUiManage.IsSaveRequestMessageVisible;
        set
        {
            if (!SetField(ref _isSaveRequestMessageVisible, value)) return;

            if (_globalPropsUiManage.IsSaveRequestMessageVisible != value)
                _globalPropsUiManage.IsSaveRequestMessageVisible = value;
        }
    }

    private bool _hasUnsavedChanges;
    public bool HasUnsavedChanges
    {
        get => _globalPropsUiManage.HasUnsavedChanges;
        set
        {
            if (!SetField(ref _hasUnsavedChanges, value)) return;
            if (_globalPropsUiManage.HasUnsavedChanges != value)
                _globalPropsUiManage.HasUnsavedChanges = value;
        }
    }

    public ICommand HideCommand { get; }
    #endregion

    #region Common Commands
    public ICommand CloseCommand { get; }
    public ICommand SubmitCommand { get; }
    public ICommand CalculateTotalsAtInvoicePositionCommand { get; }
    #endregion

    #region Vat % and Code List
    public sealed class VatCategoryItem
    {
        public string Code { get; }
        public string TooltipKey { get; }   // Key für Translation

        public VatCategoryItem(string code, string tooltipKey)
        {
            Code = code;
            TooltipKey = tooltipKey;
        }
    }
    public ObservableCollection<VatCategoryItem> VatCategoriesObservableCollection { get; } = new()
    {
        new VatCategoryItem("S",  "ToolTipVatCategory_S"),
        new VatCategoryItem("Z",  "ToolTipVatCategory_Z"),
        new VatCategoryItem("E",  "ToolTipVatCategory_E"),
        new VatCategoryItem("AE", "ToolTipVatCategory_AE"),
        new VatCategoryItem("K",  "ToolTipVatCategory_K"),
        new VatCategoryItem("G",  "ToolTipVatCategory_G"),
    };

    public string SelectedVatCategoryTooltip => InvoicePositionSelectedVatCategory?.Code switch
    {
        "S" => ToolTipVatCategory_S,
        "Z" => ToolTipVatCategory_Z,
        "E" => ToolTipVatCategory_E,
        "AE" => ToolTipVatCategory_AE,
        "K" => ToolTipVatCategory_K,
        "G" => ToolTipVatCategory_G,
        _ => ""
    };

    private VatCategoryItem? _invoicePositionSelectedVatCategory;
    public VatCategoryItem? InvoicePositionSelectedVatCategory
    {
        get => _invoicePositionSelectedVatCategory;
        set
        {
            if (!SetField(ref _invoicePositionSelectedVatCategory, value))
                return;
            OnPropertyChanged(nameof(SelectedVatCategoryTooltip));
            InvoicePositionVatCategoryCode = value?.Code ?? string.Empty;
        }
    }

    private string _invoicePositionVatCategoryCode = string.Empty;
    public string InvoicePositionVatCategoryCode
    {
        get => _invoicePositionVatCategoryCode;
        set
        {
            if (SetField(ref _invoicePositionVatCategoryCode, value))
            {
                // Code -> SelectedVatCategory sync
                var item = string.IsNullOrWhiteSpace(value) ? null : VatCategoriesObservableCollection.FirstOrDefault(v => v.Code == value);

                if (!ReferenceEquals(_invoicePositionSelectedVatCategory, item))
                {
                    _invoicePositionSelectedVatCategory = item;
                    OnPropertyChanged(nameof(InvoicePositionSelectedVatCategory));
                    OnPropertyChanged(nameof(SelectedVatCategoryTooltip));
                }
            }
        }
    }

    public List<int> VatList { get; }
    public ICollectionView VatListItemCollectionView { get; }

    private int _invoicePositionSelectedVat;
    public int InvoicePositionSelectedVat
    {
        get => _invoicePositionSelectedVat;
        set
        {
            if (!SetField(ref _invoicePositionSelectedVat, value)) return;
            InvoicePositionVatRate = InvoicePositionSelectedVat;
        }
    }

    #endregion

    #region Unit codes & List  
    private string _invoicePostionUnit = string.Empty;
    public string InvoicePostionUnit
    {
        get => _invoicePostionUnit;
        set
        {
            if (SetField(ref _invoicePostionUnit, value))
            {
                // Code -> SelectedUnit sync
                var unit = string.IsNullOrWhiteSpace(value)
                    ? null
                    : UnitsObservableCollection.FirstOrDefault(u => u.Code == value);

                if (!ReferenceEquals(_invoicePositionSelectedUnit, unit))
                {
                    _invoicePositionSelectedUnit = unit;
                    OnPropertyChanged(nameof(InvoicePositionSelectedUnit));
                }
                UpdateToEnableSaveControl();
            }
        }
    }
    public sealed class UnitItem
    {
        public string Code { get; }
        public string TextKey { get; }
        public string DisplayText { get; }

        public UnitItem(string code, string textKey, string displayText)
        {
            Code = code;
            TextKey = textKey;
            DisplayText = displayText;
        }
    }

    public ObservableCollection<UnitItem> UnitsObservableCollection { get; } = new();

    private void LoadUintsList()
    {
        UnitsObservableCollection.Clear();
        UnitsObservableCollection.Add(new UnitItem("H87", "UnitPiece", _translatorUiProvider.Translate("UnitPiece")));
        UnitsObservableCollection.Add(new UnitItem("C62", "UnitPiece", _translatorUiProvider.Translate("UnitPiece")));
        UnitsObservableCollection.Add(new UnitItem("HUR", "UnitHour", _translatorUiProvider.Translate("UnitHour")));
        UnitsObservableCollection.Add(new UnitItem("KGM", "UnitKilogram", _translatorUiProvider.Translate("UnitKilogram")));
        UnitsObservableCollection.Add(new UnitItem("LTR", "UnitLitre", _translatorUiProvider.Translate("UnitLitre")));
        UnitsObservableCollection.Add(new UnitItem("MTR", "UnitMetre", _translatorUiProvider.Translate("UnitMetre")));
        UnitsObservableCollection.Add(new UnitItem("LM", "UnitLinearMetre", _translatorUiProvider.Translate("UnitLinearMetre")));
        UnitsObservableCollection.Add(new UnitItem("M2", "UnitSquareMetre", _translatorUiProvider.Translate("UnitSquareMetre")));
        UnitsObservableCollection.Add(new UnitItem("M3", "UnitCubicMetre", _translatorUiProvider.Translate("UnitCubicMetre")));
        UnitsObservableCollection.Add(new UnitItem("DAY", "UnitDay", _translatorUiProvider.Translate("UnitDay")));
        UnitsObservableCollection.Add(new UnitItem("MIN", "UnitMinute", _translatorUiProvider.Translate("UnitMinute")));
        UnitsObservableCollection.Add(new UnitItem("SEC", "UnitSecond", _translatorUiProvider.Translate("UnitSecond")));
        UnitsObservableCollection.Add(new UnitItem("HAR", "UnitHectare", _translatorUiProvider.Translate("UnitHectare")));
        UnitsObservableCollection.Add(new UnitItem("KMT", "UnitKilometre", _translatorUiProvider.Translate("UnitKilometre")));
        UnitsObservableCollection.Add(new UnitItem("LS", "UnitFlatRate", _translatorUiProvider.Translate("UnitFlatRate")));
        UnitsObservableCollection.Add(new UnitItem("NAR", "UnitCount", _translatorUiProvider.Translate("UnitCount")));
        UnitsObservableCollection.Add(new UnitItem("NPR", "UnitPair", _translatorUiProvider.Translate("UnitPair")));
        UnitsObservableCollection.Add(new UnitItem("SET", "UnitSet", _translatorUiProvider.Translate("UnitSet")));
        UnitsObservableCollection.Add(new UnitItem("TNE", "UnitTonne", _translatorUiProvider.Translate("UnitTonne")));
        UnitsObservableCollection.Add(new UnitItem("WEE", "UnitWeek", _translatorUiProvider.Translate("UnitWeek")));
        UnitsObservableCollection.Add(new UnitItem("P1", "UnitPercent", _translatorUiProvider.Translate("UnitPercent")));
        //disbled units,duplicates (H87, MTK, MTQ, KTM and XPP) are not allowed in combobox
    }

    private UnitItem? _invoicePositionSelectedUnit;
    public UnitItem? InvoicePositionSelectedUnit
    {
        get => _invoicePositionSelectedUnit;
        set
        {
            if (!SetField(ref _invoicePositionSelectedUnit, value))
                return;
            if (!string.Equals(InvoicePostionUnit, value?.Code ?? string.Empty, StringComparison.Ordinal))
                InvoicePostionUnit = value?.Code ?? string.Empty;
        }
    }

    //private void SyncSelectedUnitFromCode(string? code)
    //{
    //    var unit = string.IsNullOrWhiteSpace(code) ? null : UnitsObservableCollection.FirstOrDefault(unit => unit.Code == code);

    //    if (!ReferenceEquals(_invoicePositionSelectedUnit, unit))
    //    {
    //        _invoicePositionSelectedUnit = unit;
    //        OnPropertyChanged(nameof(InvoicePositionSelectedUnit));
    //    }
    //}
    #endregion                      

    public InvoicePositionDetailsFormViewModel(ICollectorCollection collectorCollection, ICommand submitInvPosDetailsCommand, ICommand closeInvPosDetailsCommand)
    {
        #region Get Services / Stores from CollectorCollection
        _logger = collectorCollection.GetService<ILoggerFactory>().CreateLogger<InvoicePositionDetailsFormViewModel>();
        _appOptions = collectorCollection.GetService<IAppOptions>();
        _snapShot = collectorCollection.GetService<ISnapShotService>();
        _globalPropsUiManage = collectorCollection.GetService<IGlobalPropsUiManage>();
        _selectedInvoicePositionStore = collectorCollection.GetService<ISelectedInvoicePositionStore>();
        _translatorUiProvider = collectorCollection.GetService<ITranslatorUiProvider>();
        #endregion

        #region Selected Invoice Position
        //var selectedInvPos = _selectedInvoicePositionStore?.SelectedInvoicePosition;
        if (_selectedInvoicePositionStore.SelectedInvoicePosition != null)
            _tmpSelectedInvPos = _snapShot.TryDeepCopy(_selectedInvoicePositionStore!.SelectedInvoicePosition);
        else
        {
            _tmpSelectedInvPos = new InvoicePositionDetailsDTO
            {
                InvoicePositionDescription = string.Empty,
                InvoicePositionQuantity = 0,
                InvoicePostionUnit = string.Empty,
                InvoicePositionUnitPrice = 0,
                InvoicePositionVatRate = 0,
                //InvoicePositionNetAmount = 0,
                //InvoicePositionGrossAmount = 0,
                InvoicePositionDiscountReason = string.Empty,
                InvoicePositionDiscountNetAmount = 0,
                //InvoicePositionNetAmountAfterDiscount = 0
            };
        }
        #endregion

        StatusMessage = new MessageViewModel();

        SubmitCommand = submitInvPosDetailsCommand;
        //SubmitCommand = new CanSaveInvoicePositionDetailsCommand(this, submitInvPosDetailsCommand, collectorCollection);
        CloseCommand = closeInvPosDetailsCommand;
        HideCommand = new HideSaveRequestMessageCommand(collectorCollection);

        CalculateTotalsAtInvoicePositionCommand = new CalculateTotalsAtInvoicePositionCommand(this);

        _globalPropsUiManage.IsEnableToSaveDataChanged += GlobalPropsUiManageOnIsEnableToSaveDataChanged;
        _globalPropsUiManage.IsRequiredFieldChanged += GlobalPropsUiManageOnIsRequiredFieldChanged;
        _globalPropsUiManage.HasUnsavedChangesChanged += GlobalPropsUiManageOnHasUnsavedChangesChanged;
        _globalPropsUiManage.IsSaveRequestMessageVisibleChanged += GlobalPropsUiManageOnIsSaveRequestMessageVisibleChanged;

        FillAllInvoicePositionDetailsFormLabels();
        FillAllInvoicePositionDetailsFormToolTips();
        FillAllInvoicePositionDetailsFormPlaceholders();
        FillAllInvoicePositionDetailsFormTags();

        VatList = _appOptions.Vats.VatList.ToList();
        VatListItemCollectionView = CollectionViewSource.GetDefaultView(VatList);
        VatListItemCollectionView.MoveCurrentToFirst();

        InvoicePositionSelectedVatCategory = VatCategoriesObservableCollection.First(x => x.Code == "S");
        LoadUintsList();
    }

    private void GlobalPropsUiManageOnIsEnableToSaveDataChanged() => OnPropertyChanged(nameof(IsEnableToSaveData));
    private void GlobalPropsUiManageOnIsRequiredFieldChanged() => OnPropertyChanged(nameof(IsRequiredField));
    private void GlobalPropsUiManageOnHasUnsavedChangesChanged() => OnPropertyChanged(nameof(HasUnsavedChanges));
    private void GlobalPropsUiManageOnIsSaveRequestMessageVisibleChanged() => OnPropertyChanged(nameof(IsSaveRequestMessageVisible));

    #region Labels
    public string LabelSaveRequestMessage { get; set; } = string.Empty;
    public string LabelInvoicePositionGroupBox { get; set; } = string.Empty;
    private void FillAllInvoicePositionDetailsFormLabels()
    {
        LabelSaveRequestMessage = "Would you like to save the changes?";
        LabelInvoicePositionGroupBox = "Invoice position";
    }

    #endregion

    #region ToolTips
    public string ToolTipYesButtonSaveRequestMessage { get; set; } = string.Empty;
    public string ToolTipNoButtonSaveRequestMessage { get; set; } = string.Empty;
    public string ToolTipReturnButtonSaveRequestMessage { get; set; } = string.Empty;

    public string ToolTipVatCategory_S { get; set; } = string.Empty;
    public string ToolTipVatCategory_Z { get; set; } = string.Empty;
    public string ToolTipVatCategory_E { get; set; } = string.Empty;
    public string ToolTipVatCategory_AE { get; set; } = string.Empty;
    public string ToolTipVatCategory_K { get; set; } = string.Empty;
    public string ToolTipVatCategory_G { get; set; } = string.Empty;

    public string ToolTipInvoicePositionNr { get; set; } = string.Empty;

    public string ToolTipInvoicePositionDescription { get; set; } = string.Empty;
    public string ToolTipInvoicePositionProductDescription { get; set; } = string.Empty;

    public string ToolTipInvoicePositionItemNr { get; set; } = string.Empty;
    public string ToolTipInvoicePositionEan { get; set; } = string.Empty;
    public string ToolTipInvoicePositionQuantity { get; set; } = string.Empty;
    public string ToolTipInvoicePostionUnit { get; set; } = string.Empty;
    public string ToolTipInvoicePositionUnitPrice { get; set; } = string.Empty;

    public string ToolTipInvoicePositionSelectedVat { get; set; } = string.Empty;
    public string ToolTipInvoicePositionVatRate { get; set; } = string.Empty;
    public string ToolTipInvoicePositionSelectedVatCategory { get; set; } = string.Empty;
    public string ToolTipInvoicePositionDiscountReason { get; set; } = string.Empty;
    public string ToolTipInvoicePositionDiscountNetAmount { get; set; } = string.Empty;
    public string ToolTipInvoicePositionNetAmount { get; set; } = string.Empty;
    public string ToolTipInvoicePositionGrossAmount { get; set; } = string.Empty;
    public string ToolTipInvoicePositionNetAmountAfterDiscount { get; set; } = string.Empty;
    public string ToolTipCalculateTotalsAtInvoicePositionCommand { get; set; } = string.Empty;

    public string ToolTipInvoicePositionOrderDate { get; set; } = string.Empty;
    public string ToolTipInvoicePositionOrderId { get; set; } = string.Empty;

    public string ToolTipInvoicePositionDeliveryNoteDate { get; set; } = string.Empty;
    public string ToolTipInvoicePositionDeliveryNoteId { get; set; } = string.Empty;
    public string ToolTipInvoicePositionDeliveryNoteLineId { get; set; } = string.Empty;

    public string ToolTipInvoicePositionRefDocId { get; set; } = string.Empty;
    public string ToolTipInvoicePositionRefDocType { get; set; } = string.Empty;
    public string ToolTipInvoicePositionRefDocRefType { get; set; } = string.Empty;


    public string ToolTipAdditionalInfosExpander { get; set; } = string.Empty;

    private void FillAllInvoicePositionDetailsFormToolTips()
    {
        ToolTipYesButtonSaveRequestMessage = "Yes (UI action)";
        ToolTipNoButtonSaveRequestMessage = "No (UI action)";
        ToolTipReturnButtonSaveRequestMessage = "Exit (UI action)";

        ToolTipVatCategory_S = "S - Standard rate (ram:ApplicableTradeTax/ram:CategoryCode)";
        ToolTipVatCategory_Z = "Z - Zero rated (ram:ApplicableTradeTax/ram:CategoryCode)";
        ToolTipVatCategory_E = "E - Exempt from VAT (ram:ApplicableTradeTax/ram:CategoryCode)";
        ToolTipVatCategory_AE = "AE - Reverse charge (ram:ApplicableTradeTax/ram:CategoryCode)";
        ToolTipVatCategory_K = "K - Intra-community supply (ram:ApplicableTradeTax/ram:CategoryCode)";
        ToolTipVatCategory_G = "G - Export outside EU (ram:ApplicableTradeTax/ram:CategoryCode)";

        ToolTipInvoicePositionNr = "Line/position number (ram:AssociatedDocumentLineDocument/ram:LineID)";

        ToolTipInvoicePositionDescription = "Item name (ram:SpecifiedTradeProduct/ram:Name)";
        ToolTipInvoicePositionProductDescription = "Item description (ram:SpecifiedTradeProduct/ram:Description)";

        ToolTipInvoicePositionItemNr = "Item number / Seller ID (ram:SpecifiedTradeProduct/ram:SellerAssignedID)";
        ToolTipInvoicePositionEan = "GTIN/EAN (ram:SpecifiedTradeProduct/ram:GlobalID)";
        ToolTipInvoicePositionQuantity = "Billed quantity (ram:SpecifiedLineTradeDelivery/ram:BilledQuantity)";
        ToolTipInvoicePostionUnit = "Unit code (ram:SpecifiedLineTradeDelivery/ram:BilledQuantity/@unitCode)";
        ToolTipInvoicePositionUnitPrice = "Net unit price (excl. VAT) (ram:SpecifiedLineTradeAgreement/ram:NetPriceProductTradePrice/ram:ChargeAmount)";

        ToolTipInvoicePositionSelectedVat = "Tax type (ram:ApplicableTradeTax/ram:TypeCode)";
        ToolTipInvoicePositionVatRate = "VAT rate % (ram:ApplicableTradeTax/ram:RateApplicablePercent)";
        ToolTipInvoicePositionSelectedVatCategory = "VAT category (ram:ApplicableTradeTax/ram:CategoryCode)";

        ToolTipInvoicePositionDiscountReason = "Discount reason (ram:SpecifiedTradeAllowanceCharge/ram:Reason)";
        ToolTipInvoicePositionDiscountNetAmount = "Discount amount (ram:SpecifiedTradeSettlementLineMonetarySummation/ram:TotalAllowanceChargeAmount)";

        ToolTipInvoicePositionNetAmount = "Line net total (ram:SpecifiedTradeSettlementLineMonetarySummation/ram:LineTotalAmount)";
        ToolTipInvoicePositionGrossAmount = "Gross total (not explicit; net + tax)";
        ToolTipInvoicePositionNetAmountAfterDiscount = "Net after discount (net - allowance)";

        ToolTipCalculateTotalsAtInvoicePositionCommand = "Recalculate amounts (UI action)";

        ToolTipInvoicePositionOrderDate = "Order date (ram:BuyerOrderReferencedDocument/ram:FormattedIssueDateTime/qdt:DateTimeString)";
        ToolTipInvoicePositionOrderId = "Order reference (ram:BuyerOrderReferencedDocument/ram:IssuerAssignedID)";

        ToolTipInvoicePositionDeliveryNoteDate = "Delivery note date (ram:DeliveryNoteReferencedDocument/ram:FormattedIssueDateTime/qdt:DateTimeString)";
        ToolTipInvoicePositionDeliveryNoteId = "Delivery note id (ram:DeliveryNoteReferencedDocument/ram:IssuerAssignedID)";
        ToolTipInvoicePositionDeliveryNoteLineId = "Delivery note line id (ram:DeliveryNoteReferencedDocument/ram:LineID)";

        ToolTipInvoicePositionRefDocId = "Referenced doc id (ram:AdditionalReferencedDocument/ram:IssuerAssignedID)";
        ToolTipInvoicePositionRefDocType = "Referenced doc type (ram:AdditionalReferencedDocument/ram:TypeCode)";
        ToolTipInvoicePositionRefDocRefType = "Reference type (ram:AdditionalReferencedDocument/ram:ReferenceTypeCode)";

        ToolTipAdditionalInfosExpander = "Reference documents for this invoice position (order, delivery note, additional references).";
    }

    #endregion

    #region Placeholder
    public string PlaceholderInvoicePositionDescription { get; set; } = string.Empty;
    public string PlaceholderInvoicePositionProductDescription { get; set; } = string.Empty;

    public string PlaceholderInvoicePositionItemNr { get; set; } = string.Empty;
    public string PlaceholderInvoicePositionEan { get; set; } = string.Empty;
    public string PlaceholderInvoicePositionQuantity { get; set; } = string.Empty;
    public string PlaceholderInvoicePostionUnit { get; set; } = string.Empty;
    public string PlaceholderInvoicePositionUnitPrice { get; set; } = string.Empty;
    public string PlaceholderInvoicePositionVatRate { get; set; } = string.Empty;

    public string PlaceholderInvoicePositionDiscountReason { get; set; } = string.Empty;
    public string PlaceholderInvoicePositionDiscountNetAmount { get; set; } = string.Empty;

    public string PlaceholderInvoicePositionNetAmount { get; set; } = string.Empty;
    public string PlaceholderInvoicePositionGrossAmount { get; set; } = string.Empty;
    public string PlaceholderInvoicePositionNetAmountAfterDiscount { get; set; } = string.Empty;

    public string PlaceholderInvoicePositionOrderId { get; set; } = string.Empty;
    public string PlaceholderInvoicePositionDeliveryNoteId { get; set; } = string.Empty;
    public string PlaceholderInvoicePositionDeliveryNoteLineId { get; set; } = string.Empty;

    public string PlaceholderInvoicePositionRefDocId { get; set; } = string.Empty;
    public string PlaceholderInvoicePositionRefDocType { get; set; } = string.Empty;
    public string PlaceholderInvoicePositionRefDocRefType { get; set; } = string.Empty;

    private void FillAllInvoicePositionDetailsFormPlaceholders()
    {
        // ===== Placeholders =====
        PlaceholderInvoicePositionDescription = "Description";
        PlaceholderInvoicePositionProductDescription = "Product Description";

        PlaceholderInvoicePositionItemNr = "Item Nr.";
        PlaceholderInvoicePositionEan = "EAN";
        PlaceholderInvoicePositionQuantity = "Quantity";
        PlaceholderInvoicePostionUnit = "Unit";
        PlaceholderInvoicePositionUnitPrice = "u/p [€]";
        PlaceholderInvoicePositionVatRate = "%";

        PlaceholderInvoicePositionDiscountReason = "Discount Reason";
        PlaceholderInvoicePositionDiscountNetAmount = "Disc. Net[€]";

        PlaceholderInvoicePositionNetAmount = "Total Net[€]";
        PlaceholderInvoicePositionGrossAmount = "Total Gross[€]";
        PlaceholderInvoicePositionNetAmountAfterDiscount = "Tot. Net Disc.[€]";

        // Expander
        PlaceholderInvoicePositionOrderId = "Order Id";
        PlaceholderInvoicePositionDeliveryNoteId = "Delivery Note Id";
        PlaceholderInvoicePositionDeliveryNoteLineId = "Delivery Note Line Id";

        PlaceholderInvoicePositionRefDocId = "Ref Doc Id";
        PlaceholderInvoicePositionRefDocType = "Ref Doc Type";
        PlaceholderInvoicePositionRefDocRefType = "Ref Doc Ref Type";
    }
    #endregion

    #region Datepicker Tags
    public string TagInvoicePositionOrderDate { get; set; } = string.Empty;
    public string TagInvoicePositionDeliveryNoteDate { get; set; } = string.Empty;

    private void FillAllInvoicePositionDetailsFormTags()
    {
        TagInvoicePositionOrderDate = "Order date";
        TagInvoicePositionDeliveryNoteDate = "Delivery date";
    }
    #endregion

    public override void Dispose()
    {
        base.Dispose();

        _globalPropsUiManage.IsEnableToSaveDataChanged -= GlobalPropsUiManageOnIsEnableToSaveDataChanged;
        _globalPropsUiManage.HasUnsavedChangesChanged -= GlobalPropsUiManageOnHasUnsavedChangesChanged;
        _globalPropsUiManage.IsSaveRequestMessageVisibleChanged -= GlobalPropsUiManageOnIsSaveRequestMessageVisibleChanged;
        _globalPropsUiManage.IsRequiredFieldChanged -= GlobalPropsUiManageOnIsRequiredFieldChanged;
    }
}
