using System.Windows.Input;
using tulo.CommonMVVM.Collector;
using tulo.CommonMVVM.Commands;
using tulo.CommonMVVM.ViewModels;
using tulo.eInvoice.eInvoiceApp.DTOs;
using tulo.eInvoice.eInvoiceApp.Services;

namespace tulo.eInvoice.eInvoiceApp.ViewModels.Invoices;

public class InvoicePositionCardItemViewModel : BaseViewModel
{
    #region Services / Stores filled via CollectorCollection
    private readonly IInvoicePositionLookupService _lookup;
    #endregion

    public InvoicePositionDetailsDTO InvoicePositionDetails { get; private set; }

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

    private bool _isDeleting;
    public bool IsDeleting
    {
        get => _isDeleting;
        set => SetField(ref _isDeleting, value);
    }

    #region invoice position Commands
    public ICommand OpenEditInvoicePositionViewCommand { get; }
    public ICommand OpenDeleteInvoicePositionViewCommand { get; }
    #endregion

    public string ToolTipDiscountInfosExpander { get; set; } = string.Empty;

    public InvoicePositionCardItemViewModel(InvoicePositionDetailsDTO invoicePositionDetails, ICollectorCollection collectorCollection)
    {
        #region Get Services / Stores from CollectorCollection
        _lookup = collectorCollection.GetService<IInvoicePositionLookupService>();
        #endregion
        InvoicePositionDetails = invoicePositionDetails;

        #region Invoice position Commands
        OpenEditInvoicePositionViewCommand = new OpenModalStackCommand(collectorCollection, () => new EditInvoicePositionViewModel(collectorCollection), typeof(EditInvoicePositionViewModel));
        OpenDeleteInvoicePositionViewCommand = new OpenModalStackCommand(collectorCollection, () => new DeleteInvoicePositionViewModel(collectorCollection), typeof(DeleteInvoicePositionViewModel));
        #endregion

        ToolTipDiscountInfosExpander = "Discount details (reason, net discount, net total after discount).";
    }

    public void Update(InvoicePositionDetailsDTO invoicePositionDetails)
    {
        InvoicePositionDetails = invoicePositionDetails;
        OnPropertyChanged(nameof(InvoicePositionDetails));
    }
}
