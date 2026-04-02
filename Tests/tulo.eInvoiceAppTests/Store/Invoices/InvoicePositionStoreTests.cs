using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tulo.eInvoice.eInvoiceApp.DTOs;
using tulo.eInvoice.eInvoiceApp.Stores.Invoices;

namespace tulo.eInvoiceAppTests.Store.Invoices;

public class InvoicePositionStoreTests
{
    private static InvoicePositionDetailsDTO MakeDto(string description) => new()
    {
        InvoicePositionDescription = description,
        InvoicePositionQuantity = 1,
        InvoicePositionUnitPrice = 100m,
        InvoicePositionNetAmount = 100m,
        InvoicePositionGrossAmount = 119m,
        InvoicePositionVatRate = 19
    };

    #region AddAsync
    [Fact(DisplayName = "AddAsync: single item is added successfully and gets position number 1")]
    public async Task AddAsync_SingleItem_ShouldSucceedAndHavePositionNo1()
    {
        var store = new InvoicePositionStore();

        var result = await store.AddAsync(MakeDto("A"));

        Assert.True(result.Success);

        var all = (await store.GetAllAsync()).Data;
        Assert.Single(all);
        Assert.Equal(1, all[0].InvoicePositionNr);
        Assert.Equal("A", all[0].InvoicePositionDescription);
    }

    [Fact(DisplayName = "AddAsync: multiple items maintain insertion order with correct position numbers")]
    public async Task AddAsync_MultipleItems_ShouldMaintainInsertionOrder()
    {
        var store = new InvoicePositionStore();

        await store.AddAsync(MakeDto("A"));
        await store.AddAsync(MakeDto("B"));
        await store.AddAsync(MakeDto("C"));

        var all = (await store.GetAllAsync()).Data;

        Assert.Equal(3, all.Count);
        Assert.Equal(1, all[0].InvoicePositionNr);
        Assert.Equal("A", all[0].InvoicePositionDescription);
        Assert.Equal(2, all[1].InvoicePositionNr);
        Assert.Equal("B", all[1].InvoicePositionDescription);
        Assert.Equal(3, all[2].InvoicePositionNr);
        Assert.Equal("C", all[2].InvoicePositionDescription);
    }

    [Fact(DisplayName = "AddAsync: item with desired position number is inserted at the correct index")]
    public async Task AddAsync_WithDesiredPositionNo_ShouldInsertAtCorrectPosition()
    {
        var store = new InvoicePositionStore();

        await store.AddAsync(MakeDto("A"));
        await store.AddAsync(MakeDto("C"));
        await store.AddAsync(MakeDto("B"), desiredPositionNo: 2);

        var all = (await store.GetAllAsync()).Data;

        Assert.Equal(3, all.Count);
        Assert.Equal("A", all[0].InvoicePositionDescription);
        Assert.Equal(1, all[0].InvoicePositionNr);
        Assert.Equal("B", all[1].InvoicePositionDescription);
        Assert.Equal(2, all[1].InvoicePositionNr);
        Assert.Equal("C", all[2].InvoicePositionDescription);
        Assert.Equal(3, all[2].InvoicePositionNr);
    }

    [Fact(DisplayName = "AddAsync: out-of-range desired position number is clamped to the end of the list")]
    public async Task AddAsync_WithOutOfRangeDesiredPosition_ShouldClampToEnd()
    {
        var store = new InvoicePositionStore();
        await store.AddAsync(MakeDto("A"));

        await store.AddAsync(MakeDto("B"), desiredPositionNo: 999);

        var all = (await store.GetAllAsync()).Data;
        Assert.Equal("B", all[1].InvoicePositionDescription);
        Assert.Equal(2, all[1].InvoicePositionNr);
    }
    #endregion

    #region SuggestNextPositionNo

    [Fact(DisplayName = "SuggestNextPositionNo: returns 1 when store is empty")]
    public void SuggestNextPositionNo_EmptyStore_ShouldReturn1()
    {
        var store = new InvoicePositionStore();

        Assert.Equal(1, store.SuggestNextPositionNo());
    }

    [Fact(DisplayName = "SuggestNextPositionNo: returns 3 after two items have been added")]
    public async Task SuggestNextPositionNo_AfterTwoAdds_ShouldReturn3()
    {
        var store = new InvoicePositionStore();
        await store.AddAsync(MakeDto("A"));
        await store.AddAsync(MakeDto("B"));

        Assert.Equal(3, store.SuggestNextPositionNo());
    }

    [Fact(DisplayName = "SuggestNextPositionNo: decreases by one after an item is deleted")]
    public async Task SuggestNextPositionNo_AfterDelete_ShouldDecrease()
    {
        var store = new InvoicePositionStore();
        await store.AddAsync(MakeDto("A"));
        var r = await store.AddAsync(MakeDto("B"));
        await store.DeleteAsync(r.Data);

        Assert.Equal(2, store.SuggestNextPositionNo());
    }
    #endregion

    #region UpdateAsync

    [Fact(DisplayName = "UpdateAsync: existing item is updated and new description is returned in GetAllAsync")]
    public async Task UpdateAsync_ExistingItem_ShouldUpdateDescription()
    {
        var store = new InvoicePositionStore();
        var id = (await store.AddAsync(MakeDto("Alt"))).Data;

        var result = await store.UpdateAsync(id, MakeDto("Neu"));

        Assert.True(result.Success);
        var all = (await store.GetAllAsync()).Data;
        Assert.Equal("Neu", all[0].InvoicePositionDescription);
    }

    [Fact(DisplayName = "UpdateAsync: non-existing id returns a failed result")]
    public async Task UpdateAsync_NonExistingItem_ShouldFail()
    {
        var store = new InvoicePositionStore();

        var result = await store.UpdateAsync(Guid.NewGuid(), MakeDto("X"));

        Assert.False(result.Success);
    }

    [Fact(DisplayName = "UpdateAsync: update on a deleted item fails and does not create a ghost entry")]
    public async Task UpdateAsync_AfterDelete_ShouldFail_NoGhostEntry()
    {
        var store = new InvoicePositionStore();
        var id = (await store.AddAsync(MakeDto("A"))).Data;

        await store.DeleteAsync(id);
        var result = await store.UpdateAsync(id, MakeDto("Ghost"));

        Assert.False(result.Success);
        Assert.Empty((await store.GetAllAsync()).Data);
    }
    #endregion

    #region DeleteAsync
    [Fact(DisplayName = "DeleteAsync: existing item is removed successfully and store is empty afterwards")]
    public async Task DeleteAsync_ExistingItem_ShouldSucceed()
    {
        var store = new InvoicePositionStore();
        var id = (await store.AddAsync(MakeDto("A"))).Data;

        var result = await store.DeleteAsync(id);

        Assert.True(result.Success);
        Assert.Empty((await store.GetAllAsync()).Data);
    }

    [Fact(DisplayName = "DeleteAsync: non-existing id returns a failed result")]
    public async Task DeleteAsync_NonExistingItem_ShouldFail()
    {
        var store = new InvoicePositionStore();

        var result = await store.DeleteAsync(Guid.NewGuid());

        Assert.False(result.Success);
    }
    #endregion

    #region Positionsnummer-Renummerierung nach Delete

    [Fact(DisplayName = "DeleteAsync: deleting the middle position renumbers the following item from 3 to 2")]
    public async Task DeleteAsync_MiddlePosition_ShouldRenumberRemaining()
    {
        var store = new InvoicePositionStore();

        await store.AddAsync(MakeDto("A"));
        var idB = (await store.AddAsync(MakeDto("B"))).Data;
        await store.AddAsync(MakeDto("C"));

        // ── Verify initial state: A=1, B=2, C=3 ────────────────────────────────
        var before = (await store.GetAllAsync()).Data;

        Assert.Equal(3, before.Count);
        Assert.Equal("A", before[0].InvoicePositionDescription);
        Assert.Equal(1, before[0].InvoicePositionNr);
        Assert.Equal("B", before[1].InvoicePositionDescription);
        Assert.Equal(2, before[1].InvoicePositionNr);
        Assert.Equal("C", before[2].InvoicePositionDescription);
        Assert.Equal(3, before[2].InvoicePositionNr);

        // ── Delete position 2 (B) ───────────────────────────────────────────────
        await store.DeleteAsync(idB);

        // ── Verify renumbering after deletion: A=1, C=2 ─────────────────────────
        var after = (await store.GetAllAsync()).Data;

        Assert.Equal(2, after.Count);
        Assert.Equal("A", after[0].InvoicePositionDescription);
        Assert.Equal(1, after[0].InvoicePositionNr);
        Assert.Equal("C", after[1].InvoicePositionDescription);
        Assert.Equal(2, after[1].InvoicePositionNr); // was 3, now 2 ✅
    }

    [Fact(DisplayName = "DeleteAsync: deleting the first position renumbers all remaining items starting from 1")]
    public async Task DeleteAsync_FirstPosition_ShouldRenumberRemaining()
    {
        var store = new InvoicePositionStore();

        var idA = (await store.AddAsync(MakeDto("A"))).Data;
        await store.AddAsync(MakeDto("B"));
        await store.AddAsync(MakeDto("C"));

        // ── Verify initial state: A=1, B=2, C=3 ────────────────────────────────
        var before = (await store.GetAllAsync()).Data;

        Assert.Equal(3, before.Count);
        Assert.Equal("A", before[0].InvoicePositionDescription);
        Assert.Equal(1, before[0].InvoicePositionNr);
        Assert.Equal("B", before[1].InvoicePositionDescription);
        Assert.Equal(2, before[1].InvoicePositionNr);
        Assert.Equal("C", before[2].InvoicePositionDescription);
        Assert.Equal(3, before[2].InvoicePositionNr);

        // ── Delete position 1 (A) ───────────────────────────────────────────────
        await store.DeleteAsync(idA);

        // ── Verify renumbering after deletion: B=1, C=2 ─────────────────────────
        var after = (await store.GetAllAsync()).Data;

        Assert.Equal(2, after.Count);
        Assert.Equal("B", after[0].InvoicePositionDescription);
        Assert.Equal(1, after[0].InvoicePositionNr); // was 2, now 1 ✅
        Assert.Equal("C", after[1].InvoicePositionDescription);
        Assert.Equal(2, after[1].InvoicePositionNr); // was 3, now 2 ✅
    }

    [Fact(DisplayName = "DeleteAsync: deleting the last position does not affect the position numbers of remaining items")]
    public async Task DeleteAsync_LastPosition_ShouldNotAffectOthers()
    {
        var store = new InvoicePositionStore();

        await store.AddAsync(MakeDto("A"));
        await store.AddAsync(MakeDto("B"));
        var idC = (await store.AddAsync(MakeDto("C"))).Data;

        // ── Verify initial state: A=1, B=2, C=3 ────────────────────────────────
        var before = (await store.GetAllAsync()).Data;

        Assert.Equal(3, before.Count);
        Assert.Equal("A", before[0].InvoicePositionDescription);
        Assert.Equal(1, before[0].InvoicePositionNr);
        Assert.Equal("B", before[1].InvoicePositionDescription);
        Assert.Equal(2, before[1].InvoicePositionNr);
        Assert.Equal("C", before[2].InvoicePositionDescription);
        Assert.Equal(3, before[2].InvoicePositionNr);

        // ── Delete position 3 (C) ───────────────────────────────────────────────
        await store.DeleteAsync(idC);

        // ── Verify remaining items are unaffected: A=1, B=2 ─────────────────────
        var after = (await store.GetAllAsync()).Data;

        Assert.Equal(2, after.Count);
        Assert.Equal("A", after[0].InvoicePositionDescription);
        Assert.Equal(1, after[0].InvoicePositionNr); // unchanged ✅
        Assert.Equal("B", after[1].InvoicePositionDescription);
        Assert.Equal(2, after[1].InvoicePositionNr); // unchanged ✅
    }
    #endregion

    #region SetPositionNoAsync

    [Fact(DisplayName = "SetPositionNoAsync: moving an item to a new position renumbers all items correctly")]
    public async Task SetPositionNoAsync_MovesItemAndRenumbersAll()
    {
        var store = new InvoicePositionStore();

        await store.AddAsync(MakeDto("A"));
        await store.AddAsync(MakeDto("B"));
        var idC = (await store.AddAsync(MakeDto("C"))).Data;

        var result = await store.SetPositionNoAsync(idC, 1);

        Assert.True(result.Success);
        var all = result.Data;

        Assert.Equal("C", all[0].InvoicePositionDescription);
        Assert.Equal(1, all[0].InvoicePositionNr);
        Assert.Equal("A", all[1].InvoicePositionDescription);
        Assert.Equal(2, all[1].InvoicePositionNr);
        Assert.Equal("B", all[2].InvoicePositionDescription);
        Assert.Equal(3, all[2].InvoicePositionNr);
    }

    [Fact(DisplayName = "SetPositionNoAsync: non-existing id returns a failed result")]
    public async Task SetPositionNoAsync_NonExistingItem_ShouldFail()
    {
        var store = new InvoicePositionStore();

        var result = await store.SetPositionNoAsync(Guid.NewGuid(), 1);

        Assert.False(result.Success);
    }

    [Fact(DisplayName = "SetPositionNoAsync: out-of-range position number is clamped to the last position")]
    public async Task SetPositionNoAsync_OutOfRangePosition_ShouldClampToEnd()
    {
        var store = new InvoicePositionStore();

        var idA = (await store.AddAsync(MakeDto("A"))).Data;
        await store.AddAsync(MakeDto("B"));

        var result = await store.SetPositionNoAsync(idA, 999);

        Assert.True(result.Success);
        var all = result.Data;
        Assert.Equal("B", all[0].InvoicePositionDescription);
        Assert.Equal("A", all[1].InvoicePositionDescription);
        Assert.Equal(2, all[1].InvoicePositionNr);
    }
    #endregion

    #region GetAllAsync

    [Fact(DisplayName = "GetAllAsync: empty store returns a successful result with an empty list")]
    public async Task GetAllAsync_EmptyStore_ShouldReturnEmptyList()
    {
        var store = new InvoicePositionStore();

        var result = await store.GetAllAsync();

        Assert.True(result.Success);
        Assert.Empty(result.Data);
    }

    [Fact(DisplayName = "GetAllAsync: InvoicePositionNr is always derived from actual order and ignores the stored DTO value")]
    public async Task GetAllAsync_PositionNr_AlwaysReflectsActualOrder()
    {
        var store = new InvoicePositionStore();

        var dto = MakeDto("A");
        dto.InvoicePositionNr = 42; // absichtlich falscher Wert
        await store.AddAsync(dto);

        var all = (await store.GetAllAsync()).Data;

        Assert.Equal(1, all[0].InvoicePositionNr);
    }
    #endregion
}


