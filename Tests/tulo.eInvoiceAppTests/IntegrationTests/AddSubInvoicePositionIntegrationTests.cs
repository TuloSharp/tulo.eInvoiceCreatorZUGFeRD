using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using tulo.CommonMVVM.Collector;
using tulo.CommonMVVM.GlobalProperties;
using tulo.CommonMVVM.Stores;
using tulo.CoreLib.Interfaces.SnapShots;
using tulo.CoreLib.Services;
using tulo.CoreLib.Translators;
using tulo.eInvoiceCreatorZUGFeRD.DTOs;
using tulo.eInvoiceCreatorZUGFeRD.Options;
using tulo.eInvoiceCreatorZUGFeRD.Services;
using tulo.eInvoiceCreatorZUGFeRD.Stores.Invoices;
using tulo.eInvoiceCreatorZUGFeRD.ViewModels.Invoices;
using tulo.eInvoiceCreatorZUGFeRDTests.TestInfrastructure;

namespace tulo.eInvoiceCreatorZUGFeRDTests.IntegrationTests;

public class AddSubInvoicePositionIntegrationTests : IDisposable
{
    private readonly WpfTestContext _wpf = new();
    private readonly ICollectorCollection _collectorCollection;
    private readonly InvoicePositionStore _store;
    private readonly SelectedInvoicePositionStore _selectionStore;

    public AddSubInvoicePositionIntegrationTests()
    {
        _store = new InvoicePositionStore();
        var invoicePositionService = new InvoicePositionService(_store);
        _selectionStore = new SelectedInvoicePositionStore(invoicePositionService);

        var testTranslations = new Dictionary<string, string>
        {
            ["LabelCreateInvoicePosition"] = "Create Invoice Position",
            ["ToolTipReturn"] = "Return",
            ["ToolTipSave"] = "Save",
            ["LabelSaveRequestMessage"] = "Save changes?",
            ["LabelInvoicePositionGroupBox"] = "Invoice Position"
        };

        _collectorCollection = new CollectorCollection();
        _collectorCollection.AddService<IInvoicePositionStore>(_store);
        _collectorCollection.AddService<ISelectedInvoicePositionStore>(_selectionStore);
        _collectorCollection.AddService<IGlobalPropsUiManage>(new GlobalPropsUiManage());
        _collectorCollection.AddService<ITranslatorUiProvider>(new TranslatorUiProvider(testTranslations));
        _collectorCollection.AddService<IInvoicePositionService>(invoicePositionService);
        _collectorCollection.AddService<ILoggerFactory>(NullLoggerFactory.Instance);
        _collectorCollection.AddService<IOptions<AppOptions>>(Options.Create(new AppOptions()));
        _collectorCollection.AddService<ISnapShotService>(new SnapShotService());
        _collectorCollection.AddService<IInvoicePositionLookupService>(new InvoicePositionLookupService(_collectorCollection));
        _collectorCollection.AddService<IModalStackNavigationStore>(new ModalStackNavigationStore());
    }

    // Creates a VM pre-filled with all required fields and parent context set
    private AddInvoicePositionViewModel MakeFilledSubVm(Guid parentId, string description = "Sub Item")
    {
        return _wpf.Invoke(() =>
        {
            _selectionStore.SelectedParentPositionId = parentId;
            var v = new AddInvoicePositionViewModel(_collectorCollection);
            v.InvoicePositionDetailsFormViewModel.InvoicePositionDescription = description;
            v.InvoicePositionDetailsFormViewModel.InvoicePositionQuantity = 1m;
            v.InvoicePositionDetailsFormViewModel.InvoicePositionUnitPrice = 100m;
            v.InvoicePositionDetailsFormViewModel.InvoicePositionVatRate = 19;
            v.InvoicePositionDetailsFormViewModel.InvoicePositionVatCategoryCode = "S";
            v.InvoicePositionDetailsFormViewModel.InvoicePostionUnit = "H87";
            return v;
        });
    }

    // Adds a GROUP position directly to the store and returns its Id
    private async Task<Guid> AddGroupToStoreAsync(string description = "Group Header")
    {
        var result = await _store.AddAsync(new InvoicePositionDetailsDTO
        {
            InvoicePositionDescription = description,
            LineStatusReasonCode = "GROUP",
            InvoicePositionQuantity = 1m,
            InvoicePositionUnitPrice = 100m
        });
        return result.Data;
    }

    #region 1. Save to store

    [Fact(DisplayName = "Integration: AddSub command saves sub-position to store")]
    public async Task AddSubCommand_SavesSubPosition_ToStore()
    {
        var groupId = await AddGroupToStoreAsync();
        var vm = MakeFilledSubVm(groupId, "Sub Item A");

        _wpf.Invoke(() => vm.AddInvoicePositionDetailsCommand.Execute(null));

        var result = await _store.GetAllAsync();
        var sub = result.Data.FirstOrDefault(p => p.IsSubPosition);

        Assert.NotNull(sub);
        Assert.Equal("Sub Item A", sub.InvoicePositionDescription);
    }

    [Fact(DisplayName = "Integration: AddSub command sets LineStatusReasonCode to DETAIL")]
    public async Task AddSubCommand_SetsLineStatusReasonCode_ToDetail()
    {
        var groupId = await AddGroupToStoreAsync();
        var vm = MakeFilledSubVm(groupId);

        _wpf.Invoke(() => vm.AddInvoicePositionDetailsCommand.Execute(null));

        var result = await _store.GetAllAsync();
        var sub = result.Data.First(p => p.IsSubPosition);

        Assert.Equal("DETAIL", sub.LineStatusReasonCode);
    }

    [Fact(DisplayName = "Integration: AddSub command links sub-position to correct parent via ParentPositionId")]
    public async Task AddSubCommand_LinksSubPosition_ToCorrectParent()
    {
        var groupId = await AddGroupToStoreAsync();
        var vm = MakeFilledSubVm(groupId);

        _wpf.Invoke(() => vm.AddInvoicePositionDetailsCommand.Execute(null));

        var result = await _store.GetAllAsync();
        var sub = result.Data.First(p => p.IsSubPosition);

        Assert.Equal(groupId, sub.ParentPositionId);
    }

    [Fact(DisplayName = "Integration: AddSub command correctly maps UnitPrice to sub-position in store")]
    public async Task AddSubCommand_MapsUnitPrice_ToSubPosition()
    {
        var groupId = await AddGroupToStoreAsync();
        var vm = MakeFilledSubVm(groupId);
        vm.InvoicePositionDetailsFormViewModel.InvoicePositionUnitPrice = 75m;

        _wpf.Invoke(() => vm.AddInvoicePositionDetailsCommand.Execute(null));

        var result = await _store.GetAllAsync();
        var sub = result.Data.First(p => p.IsSubPosition);

        Assert.Equal(75m, sub.InvoicePositionUnitPrice);
    }

    [Fact(DisplayName = "Integration: AddSub command correctly maps Quantity to sub-position in store")]
    public async Task AddSubCommand_MapsQuantity_ToSubPosition()
    {
        var groupId = await AddGroupToStoreAsync();
        var vm = MakeFilledSubVm(groupId);
        vm.InvoicePositionDetailsFormViewModel.InvoicePositionQuantity = 5m;

        _wpf.Invoke(() => vm.AddInvoicePositionDetailsCommand.Execute(null));

        var result = await _store.GetAllAsync();
        var sub = result.Data.First(p => p.IsSubPosition);

        Assert.Equal(5m, sub.InvoicePositionQuantity);
    }

    [Fact(DisplayName = "Integration: AddSub command correctly maps VatRate to sub-position in store")]
    public async Task AddSubCommand_MapsVatRate_ToSubPosition()
    {
        var groupId = await AddGroupToStoreAsync();
        var vm = MakeFilledSubVm(groupId);
        vm.InvoicePositionDetailsFormViewModel.InvoicePositionVatRate = 7;

        _wpf.Invoke(() => vm.AddInvoicePositionDetailsCommand.Execute(null));

        var result = await _store.GetAllAsync();
        var sub = result.Data.First(p => p.IsSubPosition);

        Assert.Equal(7, sub.InvoicePositionVatRate);
    }

    #endregion

    #region 2. Multiple sub-positions

    [Fact(DisplayName = "Integration: multiple sub-positions under same GROUP all land in store")]
    public async Task AddSubCommand_MultipleSubPositions_AllLandInStore()
    {
        var groupId = await AddGroupToStoreAsync();

        for (var i = 0; i < 3; i++)
        {
            var index = i;
            var vm = MakeFilledSubVm(groupId, $"Sub Item {index}");
            _wpf.Invoke(() => vm.AddInvoicePositionDetailsCommand.Execute(null));
        }

        var result = await _store.GetAllAsync();
        var subPositions = result.Data.Where(p => p.IsSubPosition).ToList();

        Assert.Equal(3, subPositions.Count);
    }

    [Fact(DisplayName = "Integration: multiple sub-positions all have correct ParentPositionId")]
    public async Task AddSubCommand_MultipleSubPositions_AllHaveCorrectParentId()
    {
        var groupId = await AddGroupToStoreAsync();

        for (var i = 0; i < 3; i++)
        {
            var index = i;
            var vm = MakeFilledSubVm(groupId, $"Sub Item {index}");
            _wpf.Invoke(() => vm.AddInvoicePositionDetailsCommand.Execute(null));
        }

        var result = await _store.GetAllAsync();
        var subPositions = result.Data.Where(p => p.IsSubPosition).ToList();

        Assert.All(subPositions, p => Assert.Equal(groupId, p.ParentPositionId));
    }

    [Fact(DisplayName = "Integration: sub-positions under different GROUPs are correctly separated in store")]
    public async Task AddSubCommand_SubPositionsUnderDifferentGroups_CorrectlySeparated()
    {
        var groupId1 = await AddGroupToStoreAsync("Group 1");
        var groupId2 = await AddGroupToStoreAsync("Group 2");

        var vm1 = MakeFilledSubVm(groupId1, "Sub of Group 1");
        _wpf.Invoke(() => vm1.AddInvoicePositionDetailsCommand.Execute(null));

        var vm2 = MakeFilledSubVm(groupId2, "Sub of Group 2");
        _wpf.Invoke(() => vm2.AddInvoicePositionDetailsCommand.Execute(null));

        var result = await _store.GetAllAsync();
        var subsOfGroup1 = result.Data.Where(p => p.ParentPositionId == groupId1).ToList();
        var subsOfGroup2 = result.Data.Where(p => p.ParentPositionId == groupId2).ToList();

        Assert.Single(subsOfGroup1);
        Assert.Single(subsOfGroup2);
        Assert.Equal("Sub of Group 1", subsOfGroup1[0].InvoicePositionDescription);
        Assert.Equal("Sub of Group 2", subsOfGroup2[0].InvoicePositionDescription);
    }

    #endregion

    #region 3. Store state after add

    [Fact(DisplayName = "Integration: GROUP count stays the same after sub-position is added")]
    public async Task AddSubCommand_GroupCount_StaysTheSame()
    {
        var groupId = await AddGroupToStoreAsync();
        var vm = MakeFilledSubVm(groupId);

        _wpf.Invoke(() => vm.AddInvoicePositionDetailsCommand.Execute(null));

        var result = await _store.GetAllAsync();
        Assert.Single(result.Data.Where(p => p.IsGroupPosition));
    }

    [Fact(DisplayName = "Integration: SelectedParentPositionId is reset to null after Dispose")]
    public async Task AddSubCommand_SelectedParentPositionId_ResetAfterDispose()
    {
        var groupId = await AddGroupToStoreAsync();
        var vm = MakeFilledSubVm(groupId);

        _wpf.Invoke(() => vm.AddInvoicePositionDetailsCommand.Execute(null));
        _wpf.Invoke(() => vm.Dispose());

        Assert.Null(_selectionStore.SelectedParentPositionId);
    }

    [Fact(DisplayName = "Integration: IsSubPosition is true on saved sub-position")]
    public async Task AddSubCommand_SavedPosition_IsSubPositionTrue()
    {
        var groupId = await AddGroupToStoreAsync();
        var vm = MakeFilledSubVm(groupId);

        _wpf.Invoke(() => vm.AddInvoicePositionDetailsCommand.Execute(null));

        var result = await _store.GetAllAsync();
        var sub = result.Data.First(p => p.IsSubPosition);

        Assert.True(sub.IsSubPosition);
        Assert.False(sub.IsGroupPosition);
        Assert.False(sub.IsStandalonePosition);
    }

    #endregion

    public void Dispose() => _wpf.Dispose();
}

