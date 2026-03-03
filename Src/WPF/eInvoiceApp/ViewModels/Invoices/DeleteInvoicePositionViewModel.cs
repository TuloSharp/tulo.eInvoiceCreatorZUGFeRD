using System.Windows.Input;
using tulo.CommonMVVM.Collector;
using tulo.CommonMVVM.Commands;
using tulo.CommonMVVM.GlobalProperties;
using tulo.CommonMVVM.ViewModels;
using tulo.eInvoice.eInvoiceApp.Commands.Invoices;
using tulo.eInvoice.eInvoiceApp.Stores.Invoices;
using tulo.ResourcesWpfLib.Commands;
using tulo.ResourcesWpfLib.Viewmodels;

namespace tulo.eInvoice.eInvoiceApp.ViewModels.Invoices;

public class DeleteInvoicePositionViewModel : BaseViewModel
{
    #region Services / Stores filled via CollectorCollection
    private readonly ISelectedInvoicePositionStore _selectedInvoicePositionStore;
    private readonly IGlobalPropsUiManage _globalPropsUiManage;
    #endregion

    public InvoicePositionDetailsFormViewModel InvoicePositionDetailsFormViewModel { get; }

    #region Manage Enable Buttons in UI
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

    private bool _invalidStateAtInputField;
    public bool InvalidStateAtInputField
    {
        get => _invalidStateAtInputField;
        set => SetField(ref _invalidStateAtInputField, value);
    }

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
        set
        {

            if (!SetField(ref _isAltShortcutKeyPressed, value)) return;
            if (_globalPropsUiManage.IsAltShortcutKeyPressed != value)
                _globalPropsUiManage.IsAltShortcutKeyPressed = value;
        }
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

    public static string SelectedViewModel => nameof(AddInvoicePositionViewModel);
    public string CurrentViewModelName => SelectedViewModel;
    #endregion

    #region Request Message
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
    #endregion

    #region Invoice Position Commands 
    public ICommand DeleteInvoicePositionDetailsCommand { get; }
    public ICommand CloseDeleteInvoicePositionDetailsCommand { get; }
    #endregion

    #region Common Commands 
    public ICommand CloseSpinnerMessageCommand { get; }
    public ICommand OpenSpinnerMessageCommand { get; }
    public ICommand IsAltShortcutKeyReleasedCommand { get; }
    public ICommand IsAltShortcutKeyPressedCommand { get; }
    #endregion

    public DeleteInvoicePositionViewModel(ICollectorCollection collectorCollection)
    {
        #region Get Services / Stores from CollectorCollection
        _selectedInvoicePositionStore = collectorCollection.GetService<ISelectedInvoicePositionStore>();
        _globalPropsUiManage = collectorCollection.GetService<IGlobalPropsUiManage>();
        #endregion

        StatusMessageViewModel = new MessageViewModel();

        #region Invoice Position Commands 
        DeleteInvoicePositionDetailsCommand = new DeleteInvoicePositionDetailsCommand(this, collectorCollection);
        CloseDeleteInvoicePositionDetailsCommand = new CloseModalStackCommand(collectorCollection, typeof(DeleteInvoicePositionViewModel), false);
        #endregion

        var invPos = _selectedInvoicePositionStore.SelectedInvoicePosition;

        InvoicePositionDetailsFormViewModel = new InvoicePositionDetailsFormViewModel(collectorCollection, DeleteInvoicePositionDetailsCommand, CloseDeleteInvoicePositionDetailsCommand)
        {
            Id = invPos.Id,
            InvoicePositionNr = invPos.InvoicePositionNr,
            InvoicePositionDescription = invPos.InvoicePositionDescription,
            InvoicePositionProductDescription = invPos.InvoicePositionProductDescription,
            InvoicePositionItemNr = invPos.InvoicePositionItemNr,
            InvoicePositionEan = invPos.InvoicePositionEan,

            InvoicePositionQuantity = invPos.InvoicePositionQuantity,
            InvoicePostionUnit = invPos.InvoicePostionUnit,

            InvoicePositionUnitPrice = invPos.InvoicePositionUnitPrice,
            InvoicePositionVatRate = invPos.InvoicePositionVatRate,

            InvoicePositionNetAmount = invPos.InvoicePositionNetAmount,
            InvoicePositionGrossAmount = invPos.InvoicePositionGrossAmount,

            InvoicePositionDiscountReason = invPos.InvoicePositionDiscountReason,
            InvoicePositionDiscountNetAmount = invPos.InvoicePositionDiscountNetAmount,
            InvoicePositionNetAmountAfterDiscount = invPos.InvoicePositionNetAmountAfterDiscount,

            InvoicePositionOrderDate = invPos.InvoicePositionOrderDate,
            InvoicePositionOrderId = invPos.InvoicePositionOrderId,

            InvoicePositionDeliveryNoteId = invPos.InvoicePositionDeliveryNoteId,
            InvoicePositionDeliveryNoteLineId = invPos.InvoicePositionDeliveryNoteLineId,
            InvoicePositionDeliveryNoteDate = invPos.InvoicePositionDeliveryNoteDate,

            InvoicePositionRefDocId = invPos.InvoicePositionRefDocId,
            InvoicePositionRefDocType = invPos.InvoicePositionRefDocType,
            InvoicePositionRefDocRefType = invPos.InvoicePositionRefDocRefType,

            //InvoicePositionSelectedVatCategory = invPos.InvoicePositionSelectedVatCategory,

            //IsEnableAdditionalButton = false,
            IsEnable4ShowButtons = true,
            IsEnableToSaveData = IsEnableToSaveData,
            IsRequiredField = IsRequiredField,
            IsSaveRequestMessageVisible = IsSaveRequestMessageVisible,
            HasUnsavedChanges = HasUnsavedChanges,
        };

        _globalPropsUiManage.IsEnableToSaveDataChanged += GlobalPropsUiManageOnIsEnableToSaveDataChanged;
        _globalPropsUiManage.IsRequiredFieldChanged += GlobalPropsUiManageOnIsRequiredFieldChanged;
        IsRequiredField = false;
        _globalPropsUiManage.HasUnsavedChangesChanged += GlobalPropsUiManageOnHasUnsavedChangesChanged;
        HasUnsavedChanges = false;
        _globalPropsUiManage.IsSaveRequestMessageVisibleChanged += GlobalPropsUiManageOnIsSaveRequestMessageVisibleChanged;
        IsRequiredField = false;
        _globalPropsUiManage.IsAltShortcutKeyPressedChanged += GlobalPropsUiManageOnIsAltShortcutKeyPressedChanged;
        IsAltShortcutKeyPressed = false;

        #region Common Commands 
        OpenSpinnerMessageCommand = new OpenModalStackCommand(collectorCollection, () => new SpinnerMessageViewModel(), typeof(SpinnerMessageViewModel));
        CloseSpinnerMessageCommand = new CloseModalStackCommand(collectorCollection, typeof(SpinnerMessageViewModel));
        IsAltShortcutKeyReleasedCommand = new IsAltShortcutKeyReleasedCommand(collectorCollection);
        IsAltShortcutKeyPressedCommand = new IsAltShortcutKeyPressedCommand(collectorCollection);
        #endregion

        FillAllDeleteInvoicePositionLabels();
        FillAllDeleteInvoicePositionToolTips();
    }

    #region Labels
    public string LabelDeleteInvoicePosition { get; set; } = string.Empty;

    private void FillAllDeleteInvoicePositionLabels()
    {
        LabelDeleteInvoicePosition = "Delete invoice position";
    }
    #endregion

    #region ToolTips
    public string ToolTipReturn { get; set; } = string.Empty;
    public string ToolTipSave { get; set; } = string.Empty;

    private void FillAllDeleteInvoicePositionToolTips()
    {
        ToolTipReturn = "Return";
        ToolTipSave = "Confirm";
    }
    #endregion

    private void GlobalPropsUiManageOnIsEnableToSaveDataChanged() => OnPropertyChanged(nameof(IsEnableToSaveData));
    private void GlobalPropsUiManageOnIsRequiredFieldChanged() => OnPropertyChanged(nameof(IsRequiredField));
    private void GlobalPropsUiManageOnHasUnsavedChangesChanged() => OnPropertyChanged(nameof(HasUnsavedChanges));
    private void GlobalPropsUiManageOnIsSaveRequestMessageVisibleChanged() => OnPropertyChanged(nameof(IsSaveRequestMessageVisible));
    private void GlobalPropsUiManageOnIsAltShortcutKeyPressedChanged()
    {
        IsAltShortcutKeyPressed = _globalPropsUiManage.IsAltShortcutKeyPressed;
    }

    public override void Dispose()
    {
        base.Dispose();

        _globalPropsUiManage.IsEnableToSaveDataChanged -= GlobalPropsUiManageOnIsEnableToSaveDataChanged;
        _globalPropsUiManage.HasUnsavedChangesChanged -= GlobalPropsUiManageOnHasUnsavedChangesChanged;
        _globalPropsUiManage.IsSaveRequestMessageVisibleChanged -= GlobalPropsUiManageOnIsSaveRequestMessageVisibleChanged;
        _globalPropsUiManage.IsRequiredFieldChanged -= GlobalPropsUiManageOnIsRequiredFieldChanged;
        _globalPropsUiManage.IsAltShortcutKeyPressedChanged -= GlobalPropsUiManageOnIsAltShortcutKeyPressedChanged;
    }
}
