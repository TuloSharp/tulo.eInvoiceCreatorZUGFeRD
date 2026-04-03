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

    private static InvoicePositionDetailsDTO MakeGroupDto(string description) => new()
    {
        InvoicePositionDescription = description,
        LineStatusReasonCode = "GROUP"
    };

    private static InvoicePositionDetailsDTO MakeSubPositionDto(string description) => new()
    {
        InvoicePositionDescription = description,
        LineStatusReasonCode = "DETAIL",
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
        Assert.Equal("A", all[0].InvoicePositionDescription);
        Assert.Equal(1, all[0].InvoicePositionNr);
        Assert.Equal("B", all[1].InvoicePositionDescription);
        Assert.Equal(2, all[1].InvoicePositionNr);
        Assert.Equal("C", all[2].InvoicePositionDescription);
        Assert.Equal(3, all[2].InvoicePositionNr);
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

    [Fact(DisplayName = "AddAsync: inserting at a position occupied by a GROUP inserts before that GROUP's first sub-position")]
    public async Task AddAsync_DesiredPositionOccupiedByGroup_ShouldInsertBeforeGroupBlock()
    {
        var store = new InvoicePositionStore();
        var groupId = (await store.AddAsync(MakeGroupDto("Group A"))).Data;
        await store.AddSubPositionAsync(groupId, MakeSubPositionDto("Sub A1"));

        await store.AddAsync(MakeDto("Standalone"), desiredPositionNo: 1);

        var all = (await store.GetAllAsync()).Data;

        Assert.Equal("Standalone", all[0].InvoicePositionDescription);
        Assert.Equal(1, all[0].InvoicePositionNr);
        Assert.Equal("Sub A1", all[1].InvoicePositionDescription);
        Assert.Equal("Group A", all[2].InvoicePositionDescription);
        Assert.Equal(2, all[2].InvoicePositionNr);
    }

    #endregion

    #region AddSubPositionAsync

    [Fact(DisplayName = "AddSubPositionAsync: sub-position is inserted immediately before its GROUP parent in the ordered list")]
    public async Task AddSubPositionAsync_SingleSubPosition_ShouldBeInsertedBeforeGroup()
    {
        var store = new InvoicePositionStore();
        var groupId = (await store.AddAsync(MakeGroupDto("Group A"))).Data;

        var result = await store.AddSubPositionAsync(groupId, MakeSubPositionDto("Sub A1"));

        Assert.True(result.Success);

        var all = (await store.GetAllAsync()).Data;
        Assert.Equal(2, all.Count);
        Assert.Equal("Sub A1", all[0].InvoicePositionDescription);
        Assert.Equal("Group A", all[1].InvoicePositionDescription);
    }

    [Fact(DisplayName = "AddSubPositionAsync: multiple sub-positions are inserted before GROUP in insertion order")]
    public async Task AddSubPositionAsync_MultipleSubPositions_ShouldMaintainOrderBeforeGroup()
    {
        var store = new InvoicePositionStore();
        var groupId = (await store.AddAsync(MakeGroupDto("Group A"))).Data;

        await store.AddSubPositionAsync(groupId, MakeSubPositionDto("Sub A1"));
        await store.AddSubPositionAsync(groupId, MakeSubPositionDto("Sub A2"));
        await store.AddSubPositionAsync(groupId, MakeSubPositionDto("Sub A3"));

        var all = (await store.GetAllAsync()).Data;

        Assert.Equal(4, all.Count);
        Assert.Equal("Sub A1", all[0].InvoicePositionDescription);
        Assert.Equal("Sub A2", all[1].InvoicePositionDescription);
        Assert.Equal("Sub A3", all[2].InvoicePositionDescription);
        Assert.Equal("Group A", all[3].InvoicePositionDescription);
    }

    [Fact(DisplayName = "AddSubPositionAsync: sub-position has LineStatusReasonCode DETAIL and correct ParentPositionId")]
    public async Task AddSubPositionAsync_SubPosition_ShouldHaveCorrectMetadata()
    {
        var store = new InvoicePositionStore();
        var groupId = (await store.AddAsync(MakeGroupDto("Group A"))).Data;

        var subId = (await store.AddSubPositionAsync(groupId, MakeSubPositionDto("Sub A1"))).Data;

        var all = (await store.GetAllAsync()).Data;
        var sub = all.First(p => p.Id == subId);

        Assert.True(sub.IsSubPosition);
        Assert.Equal("DETAIL", sub.LineStatusReasonCode);
        Assert.Equal(groupId, sub.ParentPositionId);
    }

    [Fact(DisplayName = "AddSubPositionAsync: two GROUP positions each keep their sub-positions in separate blocks")]
    public async Task AddSubPositionAsync_TwoGroups_ShouldKeepBlocksSeparated()
    {
        var store = new InvoicePositionStore();
        var groupAId = (await store.AddAsync(MakeGroupDto("Group A"))).Data;
        var groupBId = (await store.AddAsync(MakeGroupDto("Group B"))).Data;

        await store.AddSubPositionAsync(groupAId, MakeSubPositionDto("Sub A1"));
        await store.AddSubPositionAsync(groupBId, MakeSubPositionDto("Sub B1"));

        var all = (await store.GetAllAsync()).Data;

        // Expected order: Sub A1 → Group A → Sub B1 → Group B
        Assert.Equal(4, all.Count);
        Assert.Equal("Sub A1", all[0].InvoicePositionDescription);
        Assert.Equal("Group A", all[1].InvoicePositionDescription);
        Assert.Equal("Sub B1", all[2].InvoicePositionDescription);
        Assert.Equal("Group B", all[3].InvoicePositionDescription);
    }

    [Fact(DisplayName = "AddSubPositionAsync: non-existing parent id returns a failed result")]
    public async Task AddSubPositionAsync_NonExistingParent_ShouldFail()
    {
        var store = new InvoicePositionStore();

        var result = await store.AddSubPositionAsync(Guid.NewGuid(), MakeSubPositionDto("Sub"));

        Assert.False(result.Success);
    }

    [Fact(DisplayName = "AddSubPositionAsync: parent that is not a GROUP position returns a failed result")]
    public async Task AddSubPositionAsync_ParentIsNotGroup_ShouldFail()
    {
        var store = new InvoicePositionStore();
        var standaloneId = (await store.AddAsync(MakeDto("Standalone"))).Data;

        var result = await store.AddSubPositionAsync(standaloneId, MakeSubPositionDto("Sub"));

        Assert.False(result.Success);
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

    [Fact(DisplayName = "SuggestNextPositionNo: decreases by one after a top-level item is deleted")]
    public async Task SuggestNextPositionNo_AfterDelete_ShouldDecrease()
    {
        var store = new InvoicePositionStore();
        await store.AddAsync(MakeDto("A"));
        var r = await store.AddAsync(MakeDto("B"));
        await store.DeleteAsync(r.Data);

        Assert.Equal(2, store.SuggestNextPositionNo());
    }

    [Fact(DisplayName = "SuggestNextPositionNo: sub-positions are not counted — only top-level positions are considered")]
    public async Task SuggestNextPositionNo_WithSubPositions_ShouldOnlyCountTopLevel()
    {
        var store = new InvoicePositionStore();
        var groupId = (await store.AddAsync(MakeGroupDto("Group A"))).Data;
        await store.AddSubPositionAsync(groupId, MakeSubPositionDto("Sub A1"));
        await store.AddSubPositionAsync(groupId, MakeSubPositionDto("Sub A2"));

        Assert.Equal(2, store.SuggestNextPositionNo());
    }

    #endregion

    #region SuggestNextSubPositionNo

    [Fact(DisplayName = "SuggestNextSubPositionNo: returns 1 when the GROUP has no sub-positions yet")]
    public async Task SuggestNextSubPositionNo_NoChildren_ShouldReturn1()
    {
        var store = new InvoicePositionStore();
        var groupId = (await store.AddAsync(MakeGroupDto("Group A"))).Data;

        Assert.Equal(1, store.SuggestNextSubPositionNo(groupId));
    }

    [Fact(DisplayName = "SuggestNextSubPositionNo: returns 2 after one sub-position has been added to the GROUP")]
    public async Task SuggestNextSubPositionNo_OneChild_ShouldReturn2()
    {
        var store = new InvoicePositionStore();
        var groupId = (await store.AddAsync(MakeGroupDto("Group A"))).Data;
        await store.AddSubPositionAsync(groupId, MakeSubPositionDto("Sub A1"));

        Assert.Equal(2, store.SuggestNextSubPositionNo(groupId));
    }

    [Fact(DisplayName = "SuggestNextSubPositionNo: counts sub-positions per parent independently across multiple GROUP positions")]
    public async Task SuggestNextSubPositionNo_MultipleGroups_ShouldCountIndependently()
    {
        var store = new InvoicePositionStore();
        var groupAId = (await store.AddAsync(MakeGroupDto("Group A"))).Data;
        var groupBId = (await store.AddAsync(MakeGroupDto("Group B"))).Data;

        await store.AddSubPositionAsync(groupAId, MakeSubPositionDto("Sub A1"));
        await store.AddSubPositionAsync(groupAId, MakeSubPositionDto("Sub A2"));
        await store.AddSubPositionAsync(groupBId, MakeSubPositionDto("Sub B1"));

        Assert.Equal(3, store.SuggestNextSubPositionNo(groupAId));
        Assert.Equal(2, store.SuggestNextSubPositionNo(groupBId));
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

    [Fact(DisplayName = "DeleteAsync: deleting a GROUP position cascade-deletes all its DETAIL sub-positions")]
    public async Task DeleteAsync_GroupPosition_ShouldCascadeDeleteAllChildren()
    {
        var store = new InvoicePositionStore();
        var groupId = (await store.AddAsync(MakeGroupDto("Group A"))).Data;
        await store.AddSubPositionAsync(groupId, MakeSubPositionDto("Sub A1"));
        await store.AddSubPositionAsync(groupId, MakeSubPositionDto("Sub A2"));

        var result = await store.DeleteAsync(groupId);

        Assert.True(result.Success);
        Assert.Empty((await store.GetAllAsync()).Data);
    }

    [Fact(DisplayName = "DeleteAsync: deleting a DETAIL sub-position leaves the GROUP and remaining siblings intact")]
    public async Task DeleteAsync_SubPosition_ShouldNotAffectGroupOrSiblings()
    {
        var store = new InvoicePositionStore();
        var groupId = (await store.AddAsync(MakeGroupDto("Group A"))).Data;
        var sub1Id = (await store.AddSubPositionAsync(groupId, MakeSubPositionDto("Sub A1"))).Data;
        await store.AddSubPositionAsync(groupId, MakeSubPositionDto("Sub A2"));

        await store.DeleteAsync(sub1Id);

        var all = (await store.GetAllAsync()).Data;
        Assert.Equal(2, all.Count);
        Assert.Contains(all, p => p.InvoicePositionDescription == "Sub A2");
        Assert.Contains(all, p => p.InvoicePositionDescription == "Group A");
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

    [Fact(DisplayName = "SetPositionNoAsync: moving a GROUP position also moves all its DETAIL sub-positions as a block")]
    public async Task SetPositionNoAsync_GroupPosition_ShouldMoveEntireBlockWithChildren()
    {
        var store = new InvoicePositionStore();
        var groupAId = (await store.AddAsync(MakeGroupDto("Group A"))).Data;
        await store.AddSubPositionAsync(groupAId, MakeSubPositionDto("Sub A1"));
        await store.AddAsync(MakeDto("Standalone B"));

        // Move Group A from position 1 to position 2
        var result = await store.SetPositionNoAsync(groupAId, 2);

        Assert.True(result.Success);
        var all = result.Data;

        // Expected order: Standalone B → Sub A1 → Group A
        Assert.Equal("Standalone B", all[0].InvoicePositionDescription);
        Assert.Equal(1, all[0].InvoicePositionNr);
        Assert.Equal("Sub A1", all[1].InvoicePositionDescription);
        Assert.Equal(0, all[1].InvoicePositionNr);
        Assert.Equal("Group A", all[2].InvoicePositionDescription);
        Assert.Equal(2, all[2].InvoicePositionNr);
    }

    [Fact(DisplayName = "SetPositionNoAsync: attempting to reorder a DETAIL sub-position independently returns a failed result")]
    public async Task SetPositionNoAsync_SubPosition_ShouldFail()
    {
        var store = new InvoicePositionStore();
        var groupId = (await store.AddAsync(MakeGroupDto("Group A"))).Data;
        var subId = (await store.AddSubPositionAsync(groupId, MakeSubPositionDto("Sub A1"))).Data;

        var result = await store.SetPositionNoAsync(subId, 1);

        Assert.False(result.Success);
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

    [Fact(DisplayName = "GetAllAsync: DETAIL sub-positions have InvoicePositionNr 0, only top-level positions are numbered")]
    public async Task GetAllAsync_SubPositions_ShouldHavePositionNrZero()
    {
        var store = new InvoicePositionStore();
        var groupId = (await store.AddAsync(MakeGroupDto("Group A"))).Data;
        await store.AddSubPositionAsync(groupId, MakeSubPositionDto("Sub A1"));

        var all = (await store.GetAllAsync()).Data;
        var sub = all.First(p => p.IsSubPosition);
        var group = all.First(p => p.IsGroupPosition);

        Assert.Equal(0, sub.InvoicePositionNr);  // sub-positions are not numbered ✅
        Assert.Equal(1, group.InvoicePositionNr); // GROUP counts as top-level ✅
    }

    #endregion
}
