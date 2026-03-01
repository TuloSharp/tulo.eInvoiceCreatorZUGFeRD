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

    public SelectedInvoicePositionStore(IInvoicePositionService invociePositionStore)
    {
        _invoicePositionService = invociePositionStore;

        _invoicePositionService.InvoicePositionUpdated += OnPositionUpdated;
        _invoicePositionService.InvoicePositionCreated += OnPositionCreated;
        _invoicePositionService.InvoicePositionDeleted += OnPositionDeleted;
    }

    private void OnPositionCreated(Guid id, InvoicePositionDetailsDTO invoicePosition)
    {
        _selectedInvoicePositionId = id;
        SelectedInvoicePosition = invoicePosition;
    }

    private void OnPositionUpdated(Guid id, InvoicePositionDetailsDTO invoicePosition)
    {
        if (_selectedInvoicePositionId == id)
            SelectedInvoicePosition = invoicePosition;
    }

    private void OnPositionDeleted(Guid id)
    {
        if (_selectedInvoicePositionId == id)
        {
            _selectedInvoicePositionId = null;
            SelectedInvoicePosition = null!;
        }
    }

    public void SetSelected(Guid id, InvoicePositionDetailsDTO invoicePosition)
    {
        _selectedInvoicePositionId = id;
        SelectedInvoicePosition = invoicePosition;
    }

    public event Action? SelectedInvoicePositionChanged;
}