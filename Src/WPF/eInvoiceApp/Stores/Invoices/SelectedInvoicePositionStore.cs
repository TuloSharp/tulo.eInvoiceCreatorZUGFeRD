using tulo.eInvoice.eInvoiceApp.DTOs;
using tulo.eInvoice.eInvoiceApp.Services;

namespace tulo.eInvoice.eInvoiceApp.Stores.Invoices;
public class SelectedInvoicePositionStore : ISelectedInvoicePositionStore
{
    private readonly IInvoicePositionService _invoicePositionService;

    private Guid? _selectedInvoicePositionId;
    public Guid? SelectedInvoicePositionId
    {
        get => _selectedInvoicePositionId;
        set
        {
            if (_selectedInvoicePositionId == value) return;
            _selectedInvoicePositionId = value;
            SelectedInvoicePositionChanged?.Invoke();
        }
    }

    private InvoicePositionDetailsDTO? _selectedInvoicePosition;
    public InvoicePositionDetailsDTO SelectedInvoicePosition
    {
        get => _selectedInvoicePosition!;
        set
        {
            if (_selectedInvoicePosition == value) return;
            _selectedInvoicePosition = value;
            SelectedInvoicePositionChanged?.Invoke();
        }
    }

    // Set by InvoicePositionCardItemViewModel before opening the Add modal.
    // If not null → AddInvoicePositionViewModel will call AddSubPositionAsync under this parent.
    // Always reset to null after the Add operation completes (success or cancel).
    public Guid? SelectedParentPositionId { get; set; }

    public SelectedInvoicePositionStore(IInvoicePositionService invociePositionService)
    {
        _invoicePositionService = invociePositionService;

        _invoicePositionService.InvoicePositionUpdated += OnPositionUpdated;
        _invoicePositionService.InvoicePositionCreated += OnPositionCreated;
        _invoicePositionService.InvoicePositionDeleted += OnPositionDeleted;
    }

    private void OnPositionCreated(InvoicePositionDetailsDTO invoicePosition)
    {
        SelectedInvoicePositionId = invoicePosition.Id;
        SelectedInvoicePosition = invoicePosition;
    }

    private void OnPositionUpdated(InvoicePositionDetailsDTO invoicePosition)
    {
        SelectedInvoicePositionId = invoicePosition.Id;
        SelectedInvoicePosition = invoicePosition;
    }

    private void OnPositionDeleted(Guid id)
    {
        if (SelectedInvoicePositionId == id)
        {
            SelectedInvoicePositionId = null;
            SelectedInvoicePosition = null!;
        }
    }

    public event Action? SelectedInvoicePositionChanged;
}