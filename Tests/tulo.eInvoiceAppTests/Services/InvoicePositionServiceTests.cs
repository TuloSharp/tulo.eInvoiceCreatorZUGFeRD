using tulo.eInvoiceApp.DTOs;
using tulo.eInvoiceApp.Services;
using tulo.eInvoiceApp.Stores.Invoices;

namespace tulo.eInvoiceAppTests.Services;

public class InvoicePositionServiceTests
{
    private static InvoicePositionDetailsDTO MakeDto(
        string description, decimal qty = 1m, decimal price = 100m) => new()
        {
            InvoicePositionDescription = description,
            InvoicePositionQuantity = qty,
            InvoicePositionUnitPrice = price,
            InvoicePositionVatRate = 19,
            InvoicePositionVatCategoryCode = "S",
            InvoicePostionUnit = "C62"
        };

    private static InvoicePositionDetailsDTO MakeGroupDto(string description) => new()
    {
        InvoicePositionDescription = description,
        LineStatusReasonCode = "GROUP"
    };

    private static InvoicePositionDetailsDTO MakeSubDto(
        string description, decimal qty = 1m, decimal price = 100m) => new()
        {
            InvoicePositionDescription = description,
            LineStatusReasonCode = "DETAIL",
            InvoicePositionQuantity = qty,
            InvoicePositionUnitPrice = price,
            InvoicePositionVatRate = 19,
            InvoicePositionVatCategoryCode = "S",
            InvoicePostionUnit = "C62"
        };

    private static InvoicePositionService CreateService() =>
        new(new InvoicePositionStore());

    #region LoadAllInvoicePositionsAsync

    [Fact(DisplayName = "LoadAllInvoicePositionsAsync: empty store returns successful result with empty list and sets IsLoaded to true")]
    public async Task LoadAllInvoicePositionsAsync_EmptyStore_ShouldReturnEmptyListAndSetIsLoaded()
    {
        var svc = CreateService();

        var result = await svc.LoadAllInvoicePositionsAsync();

        Assert.True(result.Success);
        Assert.Empty(result.Data);
        Assert.True(svc.IsLoaded);
        Assert.Equal(0, svc.TotalCount);
    }

    [Fact(DisplayName = "LoadAllInvoicePositionsAsync: returns all positions with correct InvoicePositionNr and TotalCount")]
    public async Task LoadAllInvoicePositionsAsync_WithItems_ShouldReturnNumberedList()
    {
        var svc = CreateService();
        await svc.AddInvoicePositionAsync(MakeDto("A"));
        await svc.AddInvoicePositionAsync(MakeDto("B"));

        var result = await svc.LoadAllInvoicePositionsAsync();

        Assert.True(result.Success);
        Assert.Equal(2, result.Data.Count);
        Assert.Equal("A", result.Data[0].InvoicePositionDescription);
        Assert.Equal(1, result.Data[0].InvoicePositionNr);
        Assert.Equal("B", result.Data[1].InvoicePositionDescription);
        Assert.Equal(2, result.Data[1].InvoicePositionNr);
        Assert.Equal(2, svc.TotalCount);
    }

    [Fact(DisplayName = "LoadAllInvoicePositionsAsync: TotalCount counts only top-level positions and excludes sub-positions")]
    public async Task LoadAllInvoicePositionsAsync_WithSubPositions_TotalCountShouldBeTopLevelOnly()
    {
        var svc = CreateService();
        var groupId = (await svc.AddInvoicePositionAsync(MakeGroupDto("Group A"))).Data;
        await svc.AddSubInvoicePositionAsync(groupId, MakeSubDto("Sub A1"));
        await svc.AddSubInvoicePositionAsync(groupId, MakeSubDto("Sub A2"));

        var result = await svc.LoadAllInvoicePositionsAsync();

        Assert.Equal(1, svc.TotalCount);        // only the GROUP is top-level
        Assert.Equal(3, result.Data.Count);     // Sub A1 + Sub A2 + Group A in list
    }

    [Fact(DisplayName = "LoadAllInvoicePositionsAsync: DETAIL sub-positions appear before their GROUP parent in the returned list")]
    public async Task LoadAllInvoicePositionsAsync_WithGroup_DetailShouldAppearBeforeGroup()
    {
        var svc = CreateService();
        var groupId = (await svc.AddInvoicePositionAsync(MakeGroupDto("Group A"))).Data;
        await svc.AddSubInvoicePositionAsync(groupId, MakeSubDto("Sub A1"));

        var result = await svc.LoadAllInvoicePositionsAsync();

        Assert.Equal("Sub A1", result.Data[0].InvoicePositionDescription);
        Assert.Equal("Group A", result.Data[1].InvoicePositionDescription);
    }

    [Fact(DisplayName = "LoadAllInvoicePositionsAsync: fires InvoicePositionsLoaded event with the complete list")]
    public async Task LoadAllInvoicePositionsAsync_ShouldFireInvoicePositionsLoadedEvent()
    {
        var svc = CreateService();
        await svc.AddInvoicePositionAsync(MakeDto("A"));

        List<InvoicePositionDetailsDTO>? firedList = null;
        svc.InvoicePositionsLoaded += list => firedList = list;

        await svc.LoadAllInvoicePositionsAsync();

        Assert.NotNull(firedList);
        Assert.Single(firedList);
    }

    #endregion

    #region AddInvoicePositionAsync

    [Fact(DisplayName = "AddInvoicePositionAsync: valid position is added successfully and IsCreated is set to true")]
    public async Task AddInvoicePositionAsync_ValidDto_ShouldSucceedAndSetIsCreated()
    {
        var svc = CreateService();

        var result = await svc.AddInvoicePositionAsync(MakeDto("A"));

        Assert.True(result.Success);
        Assert.True(svc.IsCreated);
        Assert.NotEqual(Guid.Empty, result.Data);
    }

    [Fact(DisplayName = "AddInvoicePositionAsync: missing description returns failed result and sets AreRequiredFieldsFilled to false")]
    public async Task AddInvoicePositionAsync_MissingDescription_ShouldFailValidation()
    {
        var svc = CreateService();

        var result = await svc.AddInvoicePositionAsync(MakeDto(description: ""));

        Assert.False(result.Success);
        Assert.False(svc.AreRequiredFieldsFilled);
        Assert.False(svc.IsCreated);
    }

    [Fact(DisplayName = "AddInvoicePositionAsync: zero quantity returns failed result and sets AreRequiredFieldsFilled to false")]
    public async Task AddInvoicePositionAsync_ZeroQuantity_ShouldFailValidation()
    {
        var svc = CreateService();

        var result = await svc.AddInvoicePositionAsync(MakeDto("A", qty: 0m));

        Assert.False(result.Success);
        Assert.False(svc.AreRequiredFieldsFilled);
    }

    [Fact(DisplayName = "AddInvoicePositionAsync: GROUP position with description only passes validation successfully")]
    public async Task AddInvoicePositionAsync_GroupWithDescriptionOnly_ShouldPassValidation()
    {
        var svc = CreateService();

        var result = await svc.AddInvoicePositionAsync(MakeGroupDto("Group A"));

        Assert.True(result.Success);
        Assert.True(svc.IsCreated);
    }

    [Fact(DisplayName = "AddInvoicePositionAsync: fires InvoicePositionCreated event with the new position DTO")]
    public async Task AddInvoicePositionAsync_ShouldFireInvoicePositionCreatedEvent()
    {
        var svc = CreateService();
        InvoicePositionDetailsDTO? firedDto = null;
        svc.InvoicePositionCreated += dto => firedDto = dto;

        await svc.AddInvoicePositionAsync(MakeDto("A"));

        Assert.NotNull(firedDto);
        Assert.Equal("A", firedDto.InvoicePositionDescription);
    }

    [Fact(DisplayName = "AddInvoicePositionAsync: fires InvoicePositionsLoaded event with updated list after add")]
    public async Task AddInvoicePositionAsync_ShouldFireInvoicePositionsLoadedAfterAdd()
    {
        var svc = CreateService();
        List<InvoicePositionDetailsDTO>? firedList = null;
        svc.InvoicePositionsLoaded += list => firedList = list;

        await svc.AddInvoicePositionAsync(MakeDto("A"));

        Assert.NotNull(firedList);
        Assert.Single(firedList);
    }

    #endregion

    #region AddSubPositionAsync

    [Fact(DisplayName = "AddSubPositionAsync: valid sub-position under an existing GROUP is added successfully")]
    public async Task AddSubPositionAsync_ValidSubPosition_ShouldSucceed()
    {
        var svc = CreateService();
        var groupId = (await svc.AddInvoicePositionAsync(MakeGroupDto("Group A"))).Data;

        var result = await svc.AddSubInvoicePositionAsync(groupId, MakeSubDto("Sub A1"));

        Assert.True(result.Success);
        Assert.True(svc.IsCreated);
    }

    [Fact(DisplayName = "AddSubPositionAsync: non-existing parent id returns a failed result")]
    public async Task AddSubPositionAsync_NonExistingParent_ShouldFail()
    {
        var svc = CreateService();

        var result = await svc.AddSubInvoicePositionAsync(Guid.NewGuid(), MakeSubDto("Sub"));

        Assert.False(result.Success);
        Assert.False(svc.IsCreated);
    }

    [Fact(DisplayName = "AddSubPositionAsync: missing description on sub-position sets AreRequiredFieldsFilled to false")]
    public async Task AddSubPositionAsync_MissingDescription_ShouldFailValidation()
    {
        var svc = CreateService();
        var groupId = (await svc.AddInvoicePositionAsync(MakeGroupDto("Group A"))).Data;

        var result = await svc.AddSubInvoicePositionAsync(groupId, MakeSubDto(description: ""));

        Assert.False(result.Success);
        Assert.False(svc.AreRequiredFieldsFilled);
    }

    [Fact(DisplayName = "AddSubPositionAsync: GROUP NetAmount is recalculated as the sum of all its DETAIL children after each add")]
    public async Task AddSubPositionAsync_GroupNetAmount_ShouldEqualSumOfChildren()
    {
        var svc = CreateService();
        var groupId = (await svc.AddInvoicePositionAsync(MakeGroupDto("Group A"))).Data;
        await svc.AddSubInvoicePositionAsync(groupId, MakeSubDto("Sub A1", qty: 2m, price: 100m)); // 200
        await svc.AddSubInvoicePositionAsync(groupId, MakeSubDto("Sub A2", qty: 3m, price: 50m));  // 150

        var list = (await svc.LoadAllInvoicePositionsAsync()).Data;
        var group = list.First(p => p.IsGroupPosition);

        Assert.Equal(350m, group.InvoicePositionNetAmount);
    }

    [Fact(DisplayName = "AddSubPositionAsync: fires InvoicePositionCreated event with a DTO that has IsSubPosition true and correct ParentPositionId")]
    public async Task AddSubPositionAsync_ShouldFireInvoicePositionCreatedWithCorrectMetadata()
    {
        var svc = CreateService();
        var groupId = (await svc.AddInvoicePositionAsync(MakeGroupDto("Group A"))).Data;

        InvoicePositionDetailsDTO? firedDto = null;
        svc.InvoicePositionCreated += dto => firedDto = dto;

        await svc.AddSubInvoicePositionAsync(groupId, MakeSubDto("Sub A1"));

        Assert.NotNull(firedDto);
        Assert.True(firedDto.IsSubPosition);
        Assert.Equal(groupId, firedDto.ParentPositionId);
    }

    #endregion

    #region UpdateInvoicePositionAsync

    [Fact(DisplayName = "UpdateInvoicePositionAsync: valid update changes description and sets IsUpdated to true")]
    public async Task UpdateInvoicePositionAsync_ValidDto_ShouldSucceedAndSetIsUpdated()
    {
        var svc = CreateService();
        var id = (await svc.AddInvoicePositionAsync(MakeDto("Alt"))).Data;

        var result = await svc.UpdateInvoicePositionAsync(id, MakeDto("Neu"));

        Assert.True(result.Success);
        Assert.True(svc.IsUpdated);
    }

    [Fact(DisplayName = "UpdateInvoicePositionAsync: description change is visible in subsequent LoadAll")]
    public async Task UpdateInvoicePositionAsync_DescriptionChange_ShouldBeReflectedInLoad()
    {
        var svc = CreateService();
        var id = (await svc.AddInvoicePositionAsync(MakeDto("Alt"))).Data;

        await svc.UpdateInvoicePositionAsync(id, MakeDto("Neu"));
        var list = (await svc.LoadAllInvoicePositionsAsync()).Data;

        Assert.Equal("Neu", list[0].InvoicePositionDescription);
    }

    [Fact(DisplayName = "UpdateInvoicePositionAsync: invalid DTO returns failed result and sets AreRequiredFieldsFilled to false")]
    public async Task UpdateInvoicePositionAsync_InvalidDto_ShouldFailValidation()
    {
        var svc = CreateService();
        var id = (await svc.AddInvoicePositionAsync(MakeDto("A"))).Data;

        var result = await svc.UpdateInvoicePositionAsync(id, MakeDto(description: "", qty: 0m));

        Assert.False(result.Success);
        Assert.False(svc.AreRequiredFieldsFilled);
        Assert.False(svc.IsUpdated);
    }

    [Fact(DisplayName = "UpdateInvoicePositionAsync: fires InvoicePositionUpdated event with the updated DTO")]
    public async Task UpdateInvoicePositionAsync_ShouldFireInvoicePositionUpdatedEvent()
    {
        var svc = CreateService();
        var id = (await svc.AddInvoicePositionAsync(MakeDto("Alt"))).Data;

        InvoicePositionDetailsDTO? firedDto = null;
        svc.InvoicePositionUpdated += dto => firedDto = dto;

        await svc.UpdateInvoicePositionAsync(id, MakeDto("Neu"));

        Assert.NotNull(firedDto);
        Assert.Equal("Neu", firedDto.InvoicePositionDescription);
    }

    #endregion

    #region DeleteInvoicePositionAsync

    [Fact(DisplayName = "DeleteInvoicePositionAsync: existing position is deleted successfully and IsDeleted is set to true")]
    public async Task DeleteInvoicePositionAsync_ExistingPosition_ShouldSucceedAndSetIsDeleted()
    {
        var svc = CreateService();
        var id = (await svc.AddInvoicePositionAsync(MakeDto("A"))).Data;

        var result = await svc.DeleteInvoicePositionAsync(id);

        Assert.True(result.Success);
        Assert.True(svc.IsDeleted);
    }

    [Fact(DisplayName = "DeleteInvoicePositionAsync: non-existing id returns a failed result")]
    public async Task DeleteInvoicePositionAsync_NonExistingId_ShouldFail()
    {
        var svc = CreateService();

        var result = await svc.DeleteInvoicePositionAsync(Guid.NewGuid());

        Assert.False(result.Success);
        Assert.False(svc.IsDeleted);
    }

    [Fact(DisplayName = "DeleteInvoicePositionAsync: fires InvoicePositionDeleted event with the correct deleted id")]
    public async Task DeleteInvoicePositionAsync_ShouldFireInvoicePositionDeletedEvent()
    {
        var svc = CreateService();
        var id = (await svc.AddInvoicePositionAsync(MakeDto("A"))).Data;

        Guid firedId = Guid.Empty;
        svc.InvoicePositionDeleted += deletedId => firedId = deletedId;

        await svc.DeleteInvoicePositionAsync(id);

        Assert.Equal(id, firedId);
    }

    [Fact(DisplayName = "DeleteInvoicePositionAsync: fires InvoicePositionsLoaded with updated list not containing the deleted position")]
    public async Task DeleteInvoicePositionAsync_ShouldFireInvoicePositionsLoadedWithUpdatedList()
    {
        var svc = CreateService();
        var idA = (await svc.AddInvoicePositionAsync(MakeDto("A"))).Data;
        await svc.AddInvoicePositionAsync(MakeDto("B"));

        List<InvoicePositionDetailsDTO>? firedList = null;
        svc.InvoicePositionsLoaded += list => firedList = list;

        await svc.DeleteInvoicePositionAsync(idA);

        Assert.NotNull(firedList);
        Assert.Single(firedList);
        Assert.DoesNotContain(firedList, p => p.InvoicePositionDescription == "A");
    }

    #endregion

    #region SetInvoicePositionNoAsync

    [Fact(DisplayName = "SetInvoicePositionNoAsync: moves position to new index and returns updated list with correct numbering")]
    public async Task SetInvoicePositionNoAsync_MovesPositionAndRenumbers()
    {
        var svc = CreateService();
        await svc.AddInvoicePositionAsync(MakeDto("A"));
        await svc.AddInvoicePositionAsync(MakeDto("B"));
        var idC = (await svc.AddInvoicePositionAsync(MakeDto("C"))).Data;

        var result = await svc.SetInvoicePositionNoAsync(idC, 1);

        Assert.True(result.Success);
        Assert.Equal("C", result.Data[0].InvoicePositionDescription);
        Assert.Equal(1, result.Data[0].InvoicePositionNr);
        Assert.Equal("A", result.Data[1].InvoicePositionDescription);
        Assert.Equal(2, result.Data[1].InvoicePositionNr);
        Assert.Equal("B", result.Data[2].InvoicePositionDescription);
        Assert.Equal(3, result.Data[2].InvoicePositionNr);
    }

    [Fact(DisplayName = "SetInvoicePositionNoAsync: non-existing id returns a failed result")]
    public async Task SetInvoicePositionNoAsync_NonExistingId_ShouldFail()
    {
        var svc = CreateService();

        var result = await svc.SetInvoicePositionNoAsync(Guid.NewGuid(), 1);

        Assert.False(result.Success);
    }

    [Fact(DisplayName = "SetInvoicePositionNoAsync: fires InvoicePositionsLoaded event with the reordered list")]
    public async Task SetInvoicePositionNoAsync_ShouldFireInvoicePositionsLoadedEvent()
    {
        var svc = CreateService();
        await svc.AddInvoicePositionAsync(MakeDto("A"));
        var idB = (await svc.AddInvoicePositionAsync(MakeDto("B"))).Data;

        List<InvoicePositionDetailsDTO>? firedList = null;
        svc.InvoicePositionsLoaded += list => firedList = list;

        await svc.SetInvoicePositionNoAsync(idB, 1);

        Assert.NotNull(firedList);
        Assert.Equal("B", firedList[0].InvoicePositionDescription);
    }

    #endregion

    #region SuggestNextPositionNo

    [Fact(DisplayName = "SuggestNextPositionNo: returns 1 when no positions exist")]
    public void SuggestNextPositionNo_EmptyService_ShouldReturn1()
    {
        var svc = CreateService();

        Assert.Equal(1, svc.SuggestNextPositionNo());
    }

    [Fact(DisplayName = "SuggestNextPositionNo: returns correct next number after adding multiple positions")]
    public async Task SuggestNextPositionNo_AfterAdds_ShouldReturnNextNumber()
    {
        var svc = CreateService();
        await svc.AddInvoicePositionAsync(MakeDto("A"));
        await svc.AddInvoicePositionAsync(MakeDto("B"));

        Assert.Equal(3, svc.SuggestNextPositionNo());
    }

    [Fact(DisplayName = "SuggestNextPositionNo: sub-positions are not counted in the next number suggestion")]
    public async Task SuggestNextPositionNo_WithSubPositions_ShouldNotCountSubPositions()
    {
        var svc = CreateService();
        var groupId = (await svc.AddInvoicePositionAsync(MakeGroupDto("Group A"))).Data;
        await svc.AddSubInvoicePositionAsync(groupId, MakeSubDto("Sub A1"));
        await svc.AddSubInvoicePositionAsync(groupId, MakeSubDto("Sub A2"));

        Assert.Equal(2, svc.SuggestNextPositionNo()); // only 1 top-level GROUP → next is 2
    }

    #endregion

    #region SuggestNextSubPositionNo

    [Fact(DisplayName = "SuggestNextSubPositionNo: returns 1 when GROUP has no sub-positions yet")]
    public async Task SuggestNextSubPositionNo_NoChildren_ShouldReturn1()
    {
        var svc = CreateService();
        var groupId = (await svc.AddInvoicePositionAsync(MakeGroupDto("Group A"))).Data;

        Assert.Equal(1, svc.SuggestNextSubPositionNo(groupId));
    }

    [Fact(DisplayName = "SuggestNextSubPositionNo: returns correct next number after one sub-position has been added")]
    public async Task SuggestNextSubPositionNo_OneChild_ShouldReturn2()
    {
        var svc = CreateService();
        var groupId = (await svc.AddInvoicePositionAsync(MakeGroupDto("Group A"))).Data;
        await svc.AddSubInvoicePositionAsync(groupId, MakeSubDto("Sub A1"));

        Assert.Equal(2, svc.SuggestNextSubPositionNo(groupId));
    }

    #endregion

    #region Berechnung und Rundung

    [Fact(DisplayName = "Berechnung: NetAmount = Quantity × UnitPrice rounded to 2 decimal places")]
    public async Task Recalculate_NetAmount_ShouldBeQuantityTimesPriceRounded()
    {
        var svc = CreateService();
        await svc.AddInvoicePositionAsync(MakeDto("A", qty: 3m, price: 33.333m)); // 3 × 33.333 = 99.999 → 100.00

        var list = (await svc.LoadAllInvoicePositionsAsync()).Data;

        Assert.Equal(100.00m, list[0].InvoicePositionNetAmount);
    }

    [Fact(DisplayName = "Berechnung: GrossAmount = NetAmountAfterDiscount × (1 + VatRate / 100)")]
    public async Task Recalculate_GrossAmount_ShouldApplyVatFactor()
    {
        var svc = CreateService();
        await svc.AddInvoicePositionAsync(MakeDto("A", qty: 1m, price: 100m)); // net = 100, 19% → gross = 119

        var list = (await svc.LoadAllInvoicePositionsAsync()).Data;

        Assert.Equal(119.00m, list[0].InvoicePositionGrossAmount);
    }

    [Fact(DisplayName = "Berechnung: NetAmountAfterDiscount = NetAmount - Discount and GrossAmount reflects the reduced net")]
    public async Task Recalculate_WithDiscount_ShouldReduceNetAndGross()
    {
        var svc = CreateService();
        var dto = MakeDto("A", qty: 1m, price: 100m);
        dto.InvoicePositionDiscountNetAmount = 10m; // net = 100 − 10 = 90 → gross = 90 × 1.19 = 107.10

        await svc.AddInvoicePositionAsync(dto);
        var list = (await svc.LoadAllInvoicePositionsAsync()).Data;

        Assert.Equal(90.00m, list[0].InvoicePositionNetAmountAfterDiscount);
        Assert.Equal(107.10m, list[0].InvoicePositionGrossAmount);
    }

    [Fact(DisplayName = "Berechnung: GROUP NetAmount and GrossAmount equal the sum of all DETAIL children amounts")]
    public async Task Recalculate_GroupAmounts_ShouldBeSumOfAllChildren()
    {
        var svc = CreateService();
        var groupId = (await svc.AddInvoicePositionAsync(MakeGroupDto("Group A"))).Data;
        await svc.AddSubInvoicePositionAsync(groupId, MakeSubDto("Sub A1", qty: 2m, price: 100m)); // net=200 gross=238
        await svc.AddSubInvoicePositionAsync(groupId, MakeSubDto("Sub A2", qty: 1m, price: 50m));  // net=50  gross=59.50

        var list = (await svc.LoadAllInvoicePositionsAsync()).Data;
        var group = list.First(p => p.IsGroupPosition);

        Assert.Equal(250.00m, group.InvoicePositionNetAmount);
        Assert.Equal(297.50m, group.InvoicePositionGrossAmount);
    }

    #endregion

    #region LineId-Generierung

    [Fact(DisplayName = "LineId-Generierung: single top-level position gets LineId '01'")]
    public async Task GenerateLineIds_SingleTopLevel_ShouldGetLineId01()
    {
        var svc = CreateService();
        await svc.AddInvoicePositionAsync(MakeDto("A"));

        var list = (await svc.LoadAllInvoicePositionsAsync()).Data;

        Assert.Equal("01", list[0].LineId);
    }

    [Fact(DisplayName = "LineId-Generierung: multiple top-level positions get sequential two-digit LineIds")]
    public async Task GenerateLineIds_MultipleTopLevel_ShouldGetSequentialLineIds()
    {
        var svc = CreateService();
        await svc.AddInvoicePositionAsync(MakeDto("A"));
        await svc.AddInvoicePositionAsync(MakeDto("B"));
        await svc.AddInvoicePositionAsync(MakeDto("C"));

        var list = (await svc.LoadAllInvoicePositionsAsync()).Data;

        Assert.Equal("01", list[0].LineId);
        Assert.Equal("02", list[1].LineId);
        Assert.Equal("03", list[2].LineId);
    }

    [Fact(DisplayName = "LineId-Generierung: first sub-position of GROUP '01' gets LineId '0101'")]
    public async Task GenerateLineIds_FirstSubPosition_ShouldGetLineId0101()
    {
        var svc = CreateService();
        var groupId = (await svc.AddInvoicePositionAsync(MakeGroupDto("Group A"))).Data;
        await svc.AddSubInvoicePositionAsync(groupId, MakeSubDto("Sub A1"));

        var list = (await svc.LoadAllInvoicePositionsAsync()).Data;
        var sub = list.First(p => p.IsSubPosition);

        Assert.Equal("0101", sub.LineId);
    }

    [Fact(DisplayName = "LineId-Generierung: sub-positions of two different GROUPs get LineIds correctly scoped to their parent")]
    public async Task GenerateLineIds_TwoGroups_SubPositionsShouldBeScopedToParent()
    {
        var svc = CreateService();
        var groupAId = (await svc.AddInvoicePositionAsync(MakeGroupDto("Group A"))).Data;
        var groupBId = (await svc.AddInvoicePositionAsync(MakeGroupDto("Group B"))).Data;
        await svc.AddSubInvoicePositionAsync(groupAId, MakeSubDto("Sub A1"));
        await svc.AddSubInvoicePositionAsync(groupAId, MakeSubDto("Sub A2"));
        await svc.AddSubInvoicePositionAsync(groupBId, MakeSubDto("Sub B1"));

        var list = (await svc.LoadAllInvoicePositionsAsync()).Data;
        var subA1 = list.First(p => p.InvoicePositionDescription == "Sub A1");
        var subA2 = list.First(p => p.InvoicePositionDescription == "Sub A2");
        var subB1 = list.First(p => p.InvoicePositionDescription == "Sub B1");

        Assert.Equal("0101", subA1.LineId);
        Assert.Equal("0102", subA2.LineId);
        Assert.Equal("0201", subB1.LineId);
    }

    #endregion

    #region Statusflags und Events

    [Fact(DisplayName = "Statusflags: all mutation flags are false before any operation is performed")]
    public void Statusflags_BeforeAnyOperation_AllFlagsShouldBeFalse()
    {
        var svc = CreateService();

        Assert.False(svc.IsLoaded);
        Assert.False(svc.IsCreated);
        Assert.False(svc.IsUpdated);
        Assert.False(svc.IsDeleted);
        Assert.False(svc.AreRequiredFieldsFilled);
    }

    [Fact(DisplayName = "Statusflags: IsUpdated from a previous update is reset to false when a new add operation begins")]
    public async Task Statusflags_IsUpdated_IsResetOnNextOperation()
    {
        var svc = CreateService();
        var id = (await svc.AddInvoicePositionAsync(MakeDto("A"))).Data;

        await svc.UpdateInvoicePositionAsync(id, MakeDto("B"));
        Assert.True(svc.IsUpdated);

        await svc.AddInvoicePositionAsync(MakeDto("C"));

        Assert.False(svc.IsUpdated);  // reset by new operation ✅
        Assert.True(svc.IsCreated);   // set by the new add ✅
    }

    [Fact(DisplayName = "Statusflags: AreRequiredFieldsFilled is true after valid add and false after invalid add")]
    public async Task Statusflags_AreRequiredFieldsFilled_ReflectsValidation()
    {
        var svc = CreateService();

        await svc.AddInvoicePositionAsync(MakeDto("A"));
        Assert.True(svc.AreRequiredFieldsFilled);

        await svc.AddInvoicePositionAsync(MakeDto(description: ""));
        Assert.False(svc.AreRequiredFieldsFilled);
    }

    [Fact(DisplayName = "Statusflags: TotalCount is updated correctly after each add and delete")]
    public async Task Statusflags_TotalCount_IsUpdatedAfterAddAndDelete()
    {
        var svc = CreateService();

        await svc.AddInvoicePositionAsync(MakeDto("A"));
        await svc.LoadAllInvoicePositionsAsync();
        Assert.Equal(1, svc.TotalCount);

        var idB = (await svc.AddInvoicePositionAsync(MakeDto("B"))).Data;
        await svc.LoadAllInvoicePositionsAsync();
        Assert.Equal(2, svc.TotalCount);

        await svc.DeleteInvoicePositionAsync(idB);
        await svc.LoadAllInvoicePositionsAsync();
        Assert.Equal(1, svc.TotalCount);
    }

    [Fact(DisplayName = "Statusflags: StatusMessage is non-empty after a successful add operation")]
    public async Task Statusflags_StatusMessage_IsSetAfterSuccessfulAdd()
    {
        var svc = CreateService();

        await svc.AddInvoicePositionAsync(MakeDto("A"));

        Assert.False(string.IsNullOrWhiteSpace(svc.StatusMessage));
    }

    #endregion
}

