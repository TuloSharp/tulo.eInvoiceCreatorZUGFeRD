using System.Collections.Concurrent;
using tulo.CoreLib.Components.ResultPattern;
using tulo.eInvoice.eInvoiceApp.DTOs;

namespace tulo.eInvoice.eInvoiceApp.Stores.Invoices;

public sealed class InvoicePositionStore : IInvoicePositionStore
{
    private readonly Dictionary<Guid, InvoicePositionDetailsDTO> _items = new();
    private readonly List<Guid> _order = new();
    private readonly object _lock = new();

    public int SuggestNextPositionNo()
    {
        lock (_lock)
            return _order.Count + 1;
    }

    public Task<OperationResult<Guid>> AddAsync(InvoicePositionDetailsDTO dto, int? desiredPositionNo = null)
    {
        var id = Guid.NewGuid();
        var cloned = Clone(dto);
        cloned.Id = id;

        lock (_lock)
        {
            if (!_items.TryAdd(id, cloned))
                return Task.FromResult(OperationResult<Guid>.Fail("Failed to add invoice position."));

            if (desiredPositionNo.HasValue)
            {
                var targetIndex = Math.Clamp(desiredPositionNo.Value - 1, 0, _order.Count);
                _order.Insert(targetIndex, id);
            }
            else
            {
                _order.Add(id);
            }
        }

        return Task.FromResult(OperationResult<Guid>.Ok(id));
    }

    public Task<OperationResult<Guid>> UpdateAsync(Guid id, InvoicePositionDetailsDTO dto)
    {
        lock (_lock)
        {
            if (!_order.Contains(id))
                return Task.FromResult(OperationResult<Guid>.Fail("Invoice position not found."));

            var cloned = Clone(dto);
            cloned.Id = id;
            _items[id] = cloned;
        }

        return Task.FromResult(OperationResult<Guid>.Ok(id));
    }

    public Task<OperationResult<Guid>> DeleteAsync(Guid id)
    {
        lock (_lock)
        {
            if (!_items.Remove(id))
                return Task.FromResult(OperationResult<Guid>.Fail("Invoice position not found or could not be deleted."));

            _order.Remove(id);
        }

        return Task.FromResult(OperationResult<Guid>.Ok(id));
    }

    public Task<OperationResult<List<InvoicePositionDetailsDTO>>> GetAllAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(OperationResult<List<InvoicePositionDetailsDTO>>.Ok(BuildOrderedList()));
        }
    }

    public Task<OperationResult<List<InvoicePositionDetailsDTO>>> SetPositionNoAsync(Guid id, int newPositionNo)
    {
        if (newPositionNo < 1)
            newPositionNo = 1;

        lock (_lock)
        {
            var idx = _order.IndexOf(id);
            if (idx < 0)
                return Task.FromResult(OperationResult<List<InvoicePositionDetailsDTO>>.Fail("Invoice position not found."));

            _order.RemoveAt(idx);
            var targetIndex = Math.Clamp(newPositionNo - 1, 0, _order.Count);
            _order.Insert(targetIndex, id);

            return Task.FromResult(OperationResult<List<InvoicePositionDetailsDTO>>.Ok(BuildOrderedList()));
        }
    }

    private List<InvoicePositionDetailsDTO> BuildOrderedList()
    {
        var list = new List<InvoicePositionDetailsDTO>(_order.Count);
        for (var i = 0; i < _order.Count; i++)
        {
            if (!_items.TryGetValue(_order[i], out var dto))
                continue;

            var cloned = Clone(dto);
            cloned.InvoicePositionNr = i + 1; 
            list.Add(cloned);
        }
        return list;
    }

    private static InvoicePositionDetailsDTO Clone(InvoicePositionDetailsDTO invPos) => new()
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
        InvoicePositionVatCategoryCode = invPos.InvoicePositionVatCategoryCode,
        InvoicePositionNetAmount = invPos.InvoicePositionNetAmount,
        InvoicePositionGrossAmount = invPos.InvoicePositionGrossAmount,
        InvoicePositionDiscountReason = invPos.InvoicePositionDiscountReason,
        InvoicePositionDiscountNetAmount = invPos.InvoicePositionDiscountNetAmount,
        InvoicePositionNetAmountAfterDiscount = invPos.InvoicePositionNetAmountAfterDiscount,
        InvoicePositionOrderDate = invPos.InvoicePositionOrderDate,
        InvoicePositionOrderId = invPos.InvoicePositionOrderId,
        InvoicePositionDeliveryNoteDate = invPos.InvoicePositionDeliveryNoteDate,
        InvoicePositionDeliveryNoteId = invPos.InvoicePositionDeliveryNoteId,
        InvoicePositionDeliveryNoteLineId = invPos.InvoicePositionDeliveryNoteLineId,
        InvoicePositionRefDocId = invPos.InvoicePositionRefDocId,
        InvoicePositionRefDocType = invPos.InvoicePositionRefDocType,
        InvoicePositionRefDocRefType = invPos.InvoicePositionRefDocRefType,
        InvoicePositionSelectedVatCategory = invPos.InvoicePositionSelectedVatCategory
    };
}
