using tulo.CoreLib.Components.ResultPattern;
using tulo.eInvoiceApp.DTOs;

namespace tulo.eInvoiceApp.Stores.Invoices;

public sealed class InvoicePositionStore : IInvoicePositionStore
{
    private readonly Dictionary<Guid, InvoicePositionDetailsDTO> _items = new();
    private readonly List<Guid> _order = new();
    private readonly object _lock = new();

    // ── Suggestions ──────────────────────────────────────────────────────────

    public int SuggestNextPositionNo()
    {
        lock (_lock)
            // Only count top-level positions (standalone + GROUP), not sub-positions
            return _items.Values.Count(dto => !dto.IsSubPosition) + 1;
    }

    public int SuggestNextSubPositionNo(Guid parentId)
    {
        lock (_lock)
            return _items.Values.Count(dto => dto.ParentPositionId == parentId) + 1;
    }

    // ── Add top-level position ────────────────────────────────────────────────

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
                // Insert at the desired position among top-level positions only
                var topLevelIds = _order
                    .Where(oid => _items.TryGetValue(oid, out var d) && !d.IsSubPosition)
                    .ToList();

                var targetIndex = Math.Clamp(desiredPositionNo.Value - 1, 0, topLevelIds.Count);

                if (targetIndex >= topLevelIds.Count)
                {
                    _order.Add(id);
                }
                else
                {
                    // Insert before the first sub-position block of the target top-level position
                    var targetId = topLevelIds[targetIndex];
                    var insertAt = _order.IndexOf(targetId);

                    // If target is a GROUP, its children come before it — insert before the first child
                    if (_items[targetId].IsGroupPosition)
                    {
                        var firstChildIndex = _order
                            .Select((oid, i) => (oid, i))
                            .FirstOrDefault(x => _items.TryGetValue(x.oid, out var d)
                                                 && d.ParentPositionId == targetId);
                        if (firstChildIndex.oid != Guid.Empty)
                            insertAt = firstChildIndex.i;
                    }

                    _order.Insert(insertAt, id);
                }
            }
            else
            {
                _order.Add(id);
            }
        }

        return Task.FromResult(OperationResult<Guid>.Ok(id));
    }

    // ── Add sub-position (DETAIL under a GROUP) ───────────────────────────────

    public Task<OperationResult<Guid>> AddSubPositionAsync(Guid parentId, InvoicePositionDetailsDTO dto)
    {
        lock (_lock)
        {
            if (!_items.TryGetValue(parentId, out var parent))
                return Task.FromResult(OperationResult<Guid>.Fail("Parent position not found."));

            if (!parent.IsGroupPosition)
                return Task.FromResult(OperationResult<Guid>.Fail("Parent is not a GROUP position."));

            var id = Guid.NewGuid();
            var cloned = Clone(dto);
            cloned.Id = id;
            cloned.ParentPositionId = parentId;
            cloned.LineStatusReasonCode = "DETAIL";

            if (!_items.TryAdd(id, cloned))
                return Task.FromResult(OperationResult<Guid>.Fail("Failed to add sub-position."));

            // Sub-positions always appear immediately before their GROUP in _order
            // So insert just before the GROUP position
            var parentIndex = _order.IndexOf(parentId);
            _order.Insert(parentIndex, id);

            return Task.FromResult(OperationResult<Guid>.Ok(id));
        }
    }

    // ── Update ───────────────────────────────────────────────────────────────

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

    // ── Delete ───────────────────────────────────────────────────────────────

    public Task<OperationResult<Guid>> DeleteAsync(Guid id)
    {
        lock (_lock)
        {
            if (!_items.TryGetValue(id, out var dto))
                return Task.FromResult(OperationResult<Guid>.Fail("Invoice position not found."));

            // GROUP deleted → cascade delete all its DETAIL children
            if (dto.IsGroupPosition)
            {
                var childIds = _items
                    .Where(kvp => kvp.Value.ParentPositionId == id)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var childId in childIds)
                {
                    _items.Remove(childId);
                    _order.Remove(childId);
                }
            }

            _items.Remove(id);
            _order.Remove(id);
        }

        return Task.FromResult(OperationResult<Guid>.Ok(id));
    }

    // ── Get all ──────────────────────────────────────────────────────────────

    public Task<OperationResult<List<InvoicePositionDetailsDTO>>> GetAllAsync()
    {
        lock (_lock)
            return Task.FromResult(
                OperationResult<List<InvoicePositionDetailsDTO>>.Ok(BuildOrderedList()));
    }

    // ── Reorder top-level position ────────────────────────────────────────────

    public Task<OperationResult<List<InvoicePositionDetailsDTO>>> SetPositionNoAsync(Guid id, int newPositionNo)
    {
        if (newPositionNo < 1)
            newPositionNo = 1;

        lock (_lock)
        {
            if (!_items.TryGetValue(id, out var dto))
                return Task.FromResult(
                    OperationResult<List<InvoicePositionDetailsDTO>>.Fail("Invoice position not found."));

            // Sub-positions cannot be reordered independently — they follow their GROUP
            if (dto.IsSubPosition)
                return Task.FromResult(
                    OperationResult<List<InvoicePositionDetailsDTO>>.Fail(
                        "Sub-positions cannot be reordered independently."));

            // Collect the full block: children first (if GROUP), then the position itself
            var block = new List<Guid>();

            if (dto.IsGroupPosition)
            {
                var childIds = _order
                    .Where(oid => _items.TryGetValue(oid, out var d) && d.ParentPositionId == id)
                    .ToList();
                block.AddRange(childIds);
            }

            block.Add(id);

            // Remove the whole block from _order
            foreach (var bid in block)
                _order.Remove(bid);

            // Find insertion point among remaining top-level positions
            var topLevelIds = _order
                .Where(oid => _items.TryGetValue(oid, out var d) && !d.IsSubPosition)
                .ToList();

            var targetIndex = Math.Clamp(newPositionNo - 1, 0, topLevelIds.Count);

            int insertAt;
            if (targetIndex >= topLevelIds.Count)
            {
                insertAt = _order.Count;
            }
            else
            {
                var targetId = topLevelIds[targetIndex];
                insertAt = _order.IndexOf(targetId);

                // If target is a GROUP, insert before its first child
                if (_items[targetId].IsGroupPosition)
                {
                    var firstChild = _order
                        .Select((oid, i) => (oid, i))
                        .FirstOrDefault(x => _items.TryGetValue(x.oid, out var d)
                                             && d.ParentPositionId == targetId);
                    if (firstChild.oid != Guid.Empty)
                        insertAt = firstChild.i;
                }
            }

            // Re-insert the whole block at the target position
            for (var i = 0; i < block.Count; i++)
                _order.Insert(insertAt + i, block[i]);

            return Task.FromResult(
                OperationResult<List<InvoicePositionDetailsDTO>>.Ok(BuildOrderedList()));
        }
    }

    // ── Internal ─────────────────────────────────────────────────────────────

    private List<InvoicePositionDetailsDTO> BuildOrderedList()
    {
        var list = new List<InvoicePositionDetailsDTO>(_order.Count);
        var topLevelNr = 0;

        foreach (var id in _order)
        {
            if (!_items.TryGetValue(id, out var dto))
                continue;

            var cloned = Clone(dto);
            // Sub-positions get 0 — only top-level positions (standalone + GROUP) are numbered
            cloned.InvoicePositionNr = cloned.IsSubPosition ? 0 : ++topLevelNr;
            list.Add(cloned);
        }

        return list;
    }

    private static InvoicePositionDetailsDTO Clone(InvoicePositionDetailsDTO s) => new()
    {
        Id = s.Id,
        InvoicePositionNr = s.InvoicePositionNr,
        ParentPositionId = s.ParentPositionId,
        LineStatusReasonCode = s.LineStatusReasonCode,
        LineId = s.LineId,
        InvoicePositionDescription = s.InvoicePositionDescription,
        InvoicePositionProductDescription = s.InvoicePositionProductDescription,
        InvoicePositionItemNr = s.InvoicePositionItemNr,
        InvoicePositionEan = s.InvoicePositionEan,
        InvoicePositionQuantity = s.InvoicePositionQuantity,
        InvoicePostionUnit = s.InvoicePostionUnit,
        InvoicePositionUnitPrice = s.InvoicePositionUnitPrice,
        InvoicePositionVatRate = s.InvoicePositionVatRate,
        InvoicePositionVatCategoryCode = s.InvoicePositionVatCategoryCode,
        InvoicePositionNetAmount = s.InvoicePositionNetAmount,
        InvoicePositionGrossAmount = s.InvoicePositionGrossAmount,
        InvoicePositionDiscountReason = s.InvoicePositionDiscountReason,
        InvoicePositionDiscountNetAmount = s.InvoicePositionDiscountNetAmount,
        InvoicePositionNetAmountAfterDiscount = s.InvoicePositionNetAmountAfterDiscount,
        InvoicePositionOrderDate = s.InvoicePositionOrderDate,
        InvoicePositionOrderId = s.InvoicePositionOrderId,
        InvoicePositionDeliveryNoteDate = s.InvoicePositionDeliveryNoteDate,
        InvoicePositionDeliveryNoteId = s.InvoicePositionDeliveryNoteId,
        InvoicePositionDeliveryNoteLineId = s.InvoicePositionDeliveryNoteLineId,
        InvoicePositionRefDocId = s.InvoicePositionRefDocId,
        InvoicePositionRefDocType = s.InvoicePositionRefDocType,
        InvoicePositionRefDocRefType = s.InvoicePositionRefDocRefType,
        InvoicePositionSelectedVatCategory = s.InvoicePositionSelectedVatCategory
    };
}

