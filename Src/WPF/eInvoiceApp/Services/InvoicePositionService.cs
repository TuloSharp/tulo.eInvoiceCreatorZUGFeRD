using tulo.CoreLib.Components.ResultPattern;
using tulo.eInvoice.eInvoiceApp.DTOs;
using tulo.eInvoice.eInvoiceApp.Stores.Invoices;

namespace tulo.eInvoice.eInvoiceApp.Services;

public sealed class InvoicePositionService(IInvoicePositionStore invoicePositionStore) : IInvoicePositionService
{
    private readonly IInvoicePositionStore _invoicePositionStore = invoicePositionStore;

    public bool IsLoaded { get; private set; }
    public bool IsCreated { get; private set; }
    public bool IsUpdated { get; private set; }
    public bool IsDeleted { get; private set; }
    public bool AreRequiredFieldsFilled { get; private set; }
    public string StatusMessage { get; private set; } = string.Empty;

    // Only counts top-level positions (standalone + GROUP)
    public int TotalCount { get; private set; }

    public event Action<List<InvoicePositionDetailsDTO>>? InvoicePositionsLoaded;
    public event Action<InvoicePositionDetailsDTO>? InvoicePositionCreated;
    public event Action<InvoicePositionDetailsDTO>? InvoicePositionUpdated;
    public event Action<Guid>? InvoicePositionDeleted;

    #region Suggestions
    public int SuggestNextPositionNo() => _invoicePositionStore.SuggestNextPositionNo();

    public int SuggestNextSubPositionNo(Guid parentId) => _invoicePositionStore.SuggestNextSubPositionNo(parentId);
    #endregion

    #region Load
    public async Task<OperationResult<List<InvoicePositionDetailsDTO>>> LoadAllInvoicePositionsAsync()
    {
        ResetLoadFlags();

        var result = await _invoicePositionStore.GetAllAsync();
        if (!result.Success)
        {
            StatusMessage = result.Message;
            return OperationResult<List<InvoicePositionDetailsDTO>>.Fail(StatusMessage);
        }

        var list = BuildNumberedCalculatedList(result.Data ?? []);
        IsLoaded = true;
        StatusMessage = $"Loaded {TotalCount} invoice positions.";

        InvoicePositionsLoaded?.Invoke(list);
        return OperationResult<List<InvoicePositionDetailsDTO>>.Ok(list, StatusMessage);
    }
    #endregion

    #region Add top-level position
    public async Task<OperationResult<Guid>> AddInvoicePositionAsync(
        InvoicePositionDetailsDTO invPos, int? desiredPositionNo = null)
    {
        ResetMutationFlags();

        AreRequiredFieldsFilled = Validate(invPos);
        if (!AreRequiredFieldsFilled)
            return OperationResult<Guid>.Fail(string.Empty);

        Recalculate(invPos);

        var result = await _invoicePositionStore.AddAsync(invPos, desiredPositionNo);
        if (!result.Success)
        {
            StatusMessage = result.Message;
            return OperationResult<Guid>.Fail(StatusMessage);
        }

        IsCreated = true;
        StatusMessage = "Invoice position created successfully.";

        invPos.Id = result.Data;
        InvoicePositionCreated?.Invoke(invPos);

        await FireUpdatedListAsync();

        return OperationResult<Guid>.Ok(result.Data, StatusMessage);
    }
    #endregion

    #region Add sub-position (DETAIL under a GROUP)
    public async Task<OperationResult<Guid>> AddSubInvoicePositionAsync(Guid parentId, InvoicePositionDetailsDTO subPos)
    {
        ResetMutationFlags();

        subPos.LineStatusReasonCode = "DETAIL";
        subPos.ParentPositionId = parentId;

        AreRequiredFieldsFilled = Validate(subPos);
        if (!AreRequiredFieldsFilled)
            return OperationResult<Guid>.Fail(string.Empty);

        Recalculate(subPos);

        var result = await _invoicePositionStore.AddSubPositionAsync(parentId, subPos);
        if (!result.Success)
        {
            StatusMessage = result.Message;
            return OperationResult<Guid>.Fail(StatusMessage);
        }

        IsCreated = true;
        StatusMessage = "Sub-position created successfully.";

        subPos.Id = result.Data;
        InvoicePositionCreated?.Invoke(subPos);

        await FireUpdatedListAsync();

        return OperationResult<Guid>.Ok(result.Data, StatusMessage);
    }
    #endregion

    #region Update
    public async Task<OperationResult<Guid>> UpdateInvoicePositionAsync(
        Guid id, InvoicePositionDetailsDTO invPos)
    {
        ResetMutationFlags();

        AreRequiredFieldsFilled = Validate(invPos);
        if (!AreRequiredFieldsFilled)
            return OperationResult<Guid>.Fail(string.Empty);

        Recalculate(invPos);

        var result = await _invoicePositionStore.UpdateAsync(id, invPos);
        if (!result.Success)
        {
            StatusMessage = result.Message;
            return OperationResult<Guid>.Fail(StatusMessage);
        }

        IsUpdated = true;
        StatusMessage = "Invoice position updated successfully.";

        invPos.Id = id;
        InvoicePositionUpdated?.Invoke(invPos);

        return OperationResult<Guid>.Ok(id, StatusMessage);
    }
    #endregion

    #region Delete
    public async Task<OperationResult<Guid>> DeleteInvoicePositionAsync(Guid id)
    {
        ResetMutationFlags();

        var result = await _invoicePositionStore.DeleteAsync(id);
        if (!result.Success)
        {
            StatusMessage = result.Message;
            return OperationResult<Guid>.Fail(StatusMessage);
        }

        IsDeleted = true;
        StatusMessage = "Invoice position deleted successfully.";

        // Fire deleted first so the UI removes the card (+ children are removed
        // automatically via OnInvoicePositionsLoaded smart update)
        InvoicePositionDeleted?.Invoke(id);

        await FireUpdatedListAsync();

        return OperationResult<Guid>.Ok(id, StatusMessage);
    }
    #endregion

    #region Reorder
    public async Task<OperationResult<List<InvoicePositionDetailsDTO>>> SetInvoicePositionNoAsync(
        Guid id, int newPositionNo)
    {
        ResetMutationFlags();

        var result = await _invoicePositionStore.SetPositionNoAsync(id, newPositionNo);
        if (!result.Success)
        {
            StatusMessage = result.Message;
            return OperationResult<List<InvoicePositionDetailsDTO>>.Fail(result.Message);
        }

        IsUpdated = true;
        StatusMessage = "Position order updated.";

        var list = BuildNumberedCalculatedList(result.Data ?? []);
        InvoicePositionsLoaded?.Invoke(list);

        return OperationResult<List<InvoicePositionDetailsDTO>>.Ok(list, StatusMessage);
    }
    #endregion

    #region Utilities
    // ── Fire full updated list ────────────────────────────────────────────────

    private async Task FireUpdatedListAsync()
    {
        var allResult = await _invoicePositionStore.GetAllAsync();
        if (!allResult.Success) return;

        var list = BuildNumberedCalculatedList(allResult.Data ?? []);
        InvoicePositionsLoaded?.Invoke(list);
    }

    // ── Build numbered + calculated list ─────────────────────────────────────

    private List<InvoicePositionDetailsDTO> BuildNumberedCalculatedList(
        List<InvoicePositionDetailsDTO> raw)
    {
        // Pass 1: recalculate non-GROUP positions (standalone + DETAIL)
        foreach (var pos in raw.Where(p => !p.IsGroupPosition))
            Recalculate(pos);

        // Pass 2: GROUP positions → amounts = sum of their DETAIL children
        foreach (var group in raw.Where(p => p.IsGroupPosition))
            RecalculateGroup(group, raw);

        // Pass 3: assign InvoicePositionNr to top-level only (standalone + GROUP)
        //         sub-positions get InvoicePositionNr = 0
        var topLevelNr = 0;
        foreach (var pos in raw)
        {
            if (pos.IsSubPosition)
            {
                pos.InvoicePositionNr = 0;
                continue;
            }
            pos.InvoicePositionNr = ++topLevelNr;
        }

        // Pass 4: generate LineIds for all positions
        GenerateLineIds(raw);

        // TotalCount = number of top-level positions only
        TotalCount = raw.Count(p => !p.IsSubPosition);

        return raw;
    }

    // GROUP total = sum of all its DETAIL children
    private static void RecalculateGroup(
        InvoicePositionDetailsDTO group, List<InvoicePositionDetailsDTO> all)
    {
        var children = all.Where(p => p.ParentPositionId == group.Id).ToList();

        group.InvoicePositionNetAmount =
            children.Sum(c => c.InvoicePositionNetAmount);

        group.InvoicePositionDiscountNetAmount =
            children.Sum(c => c.InvoicePositionDiscountNetAmount);

        group.InvoicePositionNetAmountAfterDiscount =
            children.Sum(c => c.InvoicePositionNetAmountAfterDiscount
                               ?? c.InvoicePositionNetAmount);

        group.InvoicePositionGrossAmount =
            children.Sum(c => c.InvoicePositionGrossAmount);
    }

    // "01", "02" for top-level — "0101", "0102", "0201" for sub-positions
    private static void GenerateLineIds(List<InvoicePositionDetailsDTO> positions)
    {
        // Pass A: top-level positions in list order
        var topLevelNr = 0;
        foreach (var pos in positions.Where(p => !p.IsSubPosition))
            pos.LineId = (++topLevelNr).ToString("D2");

        // Pass B: sub-positions in list order, sequenced per parent
        var subNrByParent = new Dictionary<Guid, int>();
        foreach (var pos in positions.Where(p => p.IsSubPosition))
        {
            if (!pos.ParentPositionId.HasValue) continue;

            var parentId = pos.ParentPositionId.Value;
            var parent = positions.FirstOrDefault(p => p.Id == parentId);
            if (parent == null || string.IsNullOrEmpty(parent.LineId)) continue;

            subNrByParent.TryAdd(parentId, 0);
            pos.LineId = $"{parent.LineId}{++subNrByParent[parentId]:D2}";
        }
    }

    // ── Validation ───────────────────────────────────────────────────────────

    private static bool Validate(InvoicePositionDetailsDTO invPos)
    {
        // All positions need a description
        if (string.IsNullOrWhiteSpace(invPos.InvoicePositionDescription))
            return false;

        // GROUP positions have no own quantity/price — amounts come from children
        if (invPos.IsGroupPosition)
            return true;

        // DETAIL and standalone positions need quantity (≠ 0 because negative = allowance)
        if (invPos.InvoicePositionQuantity == 0) return false;
        if (invPos.InvoicePositionUnitPrice < 0) return false;
        if (invPos.InvoicePositionVatRate < 0) return false;

        return true;
    }

    // ── Recalculate amounts ───────────────────────────────────────────────────

    private static void Recalculate(InvoicePositionDetailsDTO invPos)
    {
        // GROUP amounts are set by RecalculateGroup — do not override here
        if (invPos.IsGroupPosition) return;

        invPos.InvoicePositionNetAmount =
            Math.Round(invPos.InvoicePositionQuantity * invPos.InvoicePositionUnitPrice,
                       2, MidpointRounding.AwayFromZero);

        var netAfterDiscount = invPos.InvoicePositionNetAmount
                               - invPos.InvoicePositionDiscountNetAmount;
        if (netAfterDiscount < 0) netAfterDiscount = 0;
        invPos.InvoicePositionNetAmountAfterDiscount = netAfterDiscount;

        var vatFactor = 1m + invPos.InvoicePositionVatRate / 100m;
        invPos.InvoicePositionGrossAmount =
            Math.Round((decimal)invPos.InvoicePositionNetAmountAfterDiscount * vatFactor,
                       2, MidpointRounding.AwayFromZero);
    }

    // ── Flag helpers ─────────────────────────────────────────────────────────

    private void ResetMutationFlags()
    {
        IsCreated = false;
        IsUpdated = false;
        IsDeleted = false;
        StatusMessage = string.Empty;
    }

    private void ResetLoadFlags()
    {
        IsLoaded = false;
        StatusMessage = string.Empty;
    } 
    #endregion
}
