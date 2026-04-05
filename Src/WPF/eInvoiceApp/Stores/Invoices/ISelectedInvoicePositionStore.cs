using tulo.eInvoiceApp.DTOs;

namespace tulo.eInvoiceApp.Stores.Invoices;

public interface ISelectedInvoicePositionStore
{
    /// <summary>
    /// The Id of the currently selected invoice position.
    /// </summary>
    Guid? SelectedInvoicePositionId { get; set; }

    /// <summary>
    /// The full DTO of the currently selected invoice position.
    /// </summary>
    InvoicePositionDetailsDTO SelectedInvoicePosition { get; set; }

    /// <summary>
    /// Transient signal set by <see cref="InvoicePositionCardItemViewModel"/> 
    /// before opening the Add modal via <c>OpenAddSubInvoicePositionViewCommand</c>.
    /// When not null, <c>AddInvoicePositionViewModel</c> treats the new position
    /// as a DETAIL sub-position under this parent and calls <c>AddSubPositionAsync</c>.
    /// Always reset to null after the add operation completes — whether saved or cancelled.
    /// </summary>
    Guid? SelectedParentPositionId { get; set; }

    /// <summary>
    /// Raised whenever the selected invoice position changes.
    /// </summary>
    event Action SelectedInvoicePositionChanged;
}
