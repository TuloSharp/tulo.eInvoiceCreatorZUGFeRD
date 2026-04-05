using System.Windows.Input;
using tulo.CommonMVVM.Collector;
using tulo.CommonMVVM.Commands;
using tulo.CommonMVVM.ViewModels;
using tulo.CoreLib.Translators;
using tulo.eInvoiceApp.DTOs;
using tulo.eInvoiceApp.Services;

namespace tulo.eInvoiceApp.ViewModels.Invoices;

public class InvoicePositionCardItemViewModel : BaseViewModel
{
    #region Services / Stores filled via CollectorCollection
    private readonly IInvoicePositionLookupService _lookup;
    private readonly ICollectorCollection _collectorCollection;
    private readonly ITranslatorUiProvider _translatorUiProvider;
    #endregion

    public InvoicePositionDetailsDTO InvoicePositionDetails { get; private set; }

    #region Pass-through properties – base position fields
    public Guid InvoicePositionId => InvoicePositionDetails.Id;
    public int InvoicePositionNr => InvoicePositionDetails.InvoicePositionNr;
    public string InvoicePositionDescription => InvoicePositionDetails.InvoicePositionDescription;
    public string InvoicePositionProductDescription => InvoicePositionDetails.InvoicePositionProductDescription;
    public string InvoicePositionItemNr => InvoicePositionDetails.InvoicePositionItemNr;
    public string InvoicePositionEan => InvoicePositionDetails.InvoicePositionEan;

    public decimal InvoicePositionQuantity => InvoicePositionDetails.InvoicePositionQuantity;
    public string InvoicePositionUnit => InvoicePositionDetails.InvoicePostionUnit;

    public string InvoicePositionUnitText => _lookup.GetUnitText(InvoicePositionUnit);

    public decimal InvoicePositionUnitPrice => InvoicePositionDetails.InvoicePositionUnitPrice;

    public int InvoicePositionVatRate => InvoicePositionDetails.InvoicePositionVatRate;
    public string InvoicePositionVatCategoryCode => InvoicePositionDetails.InvoicePositionVatCategoryCode;

    public decimal InvoicePositionNetAmount => InvoicePositionDetails.InvoicePositionNetAmount;
    public decimal InvoicePositionGrossAmount => InvoicePositionDetails.InvoicePositionGrossAmount;

    public string InvoicePositionDiscountReason => InvoicePositionDetails.InvoicePositionDiscountReason;
    public decimal InvoicePositionDiscountNetAmount => InvoicePositionDetails.InvoicePositionDiscountNetAmount;
    public decimal InvoicePositionNetAmountAfterDiscount => (decimal)InvoicePositionDetails.InvoicePositionNetAmountAfterDiscount!;

    public string InvoicePositionVatCategoryText => _lookup.GetVatCategoryText(InvoicePositionVatCategoryCode);
    public string InvoicePositionVatCategoryTooltip => _lookup.GetVatCategoryTooltip(InvoicePositionVatCategoryCode);
    #endregion

    #region Pass-through properties – line hierarchy fields
    // Hierarchical line identifier: "01" for top-level, "0101" for first sub-position of group 01, etc.
    public string LineId => InvoicePositionDetails.LineId;

    // "GROUP"  → parent/summary line (excluded from header tax totals)
    // "DETAIL" → sub-position line  (linked to a GROUP via ParentPositionId)
    // ""       → normal standalone position
    public string LineStatusReasonCode => InvoicePositionDetails.LineStatusReasonCode;

    // Id of the parent GROUP position — null for top-level and standalone positions
    public Guid? ParentPositionId => InvoicePositionDetails.ParentPositionId;

    // Convenience flags for XAML bindings (show/hide UI sections)
    public bool IsGroupPosition => InvoicePositionDetails.IsGroupPosition;
    public bool IsSubPosition => InvoicePositionDetails.IsSubPosition;
    public bool IsStandalonePosition => InvoicePositionDetails.IsStandalonePosition;
    #endregion

    #region State
    private bool _isDeleting;
    public bool IsDeleting
    {
        get => _isDeleting;
        set => SetField(ref _isDeleting, value);
    }
    #endregion

    #region Commands
    public ICommand OpenEditInvoicePositionViewCommand { get; }
    public ICommand OpenDeleteInvoicePositionViewCommand { get; }

    // Only enabled in XAML when IsGroupPosition == true
    public ICommand OpenAddSubInvociePostionViewCommand { get; }
    #endregion

    #region Tooltips
    public string ToolTipDiscountInfosExpander { get; set; } = string.Empty;
    #endregion

    public InvoicePositionCardItemViewModel(InvoicePositionDetailsDTO invoicePositionDetails, ICollectorCollection collectorCollection)
    {
        #region Get Services / Stores from CollectorCollection
        _collectorCollection = collectorCollection;
        _lookup = collectorCollection.GetService<IInvoicePositionLookupService>();
        _translatorUiProvider = collectorCollection.GetService<ITranslatorUiProvider>();
        #endregion

        InvoicePositionDetails = invoicePositionDetails;

        #region Commands
        OpenEditInvoicePositionViewCommand = new OpenModalStackCommand(collectorCollection, () => new EditInvoicePositionViewModel(collectorCollection), typeof(EditInvoicePositionViewModel));
        OpenDeleteInvoicePositionViewCommand = new OpenModalStackCommand(collectorCollection, () => new DeleteInvoicePositionViewModel(collectorCollection), typeof(DeleteInvoicePositionViewModel));
        OpenAddSubInvociePostionViewCommand = new OpenModalStackCommand(collectorCollection, () => new AddInvoicePositionViewModel(collectorCollection), typeof(AddInvoicePositionViewModel));
        #endregion

        ToolTipDiscountInfosExpander = _translatorUiProvider.Translate("ToolTipDiscountInfosExpander");
    }

    // Updates the card with fresh DTO data and notifies all bindings at once
    public void Update(InvoicePositionDetailsDTO invoicePositionDetails)
    {
        InvoicePositionDetails = invoicePositionDetails;
        OnPropertyChanged(string.Empty);
    }
}
