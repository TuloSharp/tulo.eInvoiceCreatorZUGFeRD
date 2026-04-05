using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using tulo.CommonMVVM.Collector;
using tulo.CommonMVVM.GlobalProperties;
using tulo.CommonMVVM.Stores;
using tulo.CoreLib.Interfaces.SnapShots;
using tulo.CoreLib.Services;
using tulo.CoreLib.Translators;
using tulo.eInvoiceApp.DTOs;
using tulo.eInvoiceApp.Options;
using tulo.eInvoiceApp.Services;
using tulo.eInvoiceApp.Stores.Invoices;
using tulo.eInvoiceApp.ViewModels.Invoices;
using tulo.eInvoiceAppTests.TestInfrastructure;

namespace tulo.eInvoiceAppTests.IntegrationTests;

public class AddInvoicePositionIntegrationTests : IDisposable
{
    private readonly WpfTestContext _wpf = new();
    private readonly ICollectorCollection _collectorCollection;
    private readonly InvoicePositionStore _store;
    private readonly SelectedInvoicePositionStore _selectionStore;

    public AddInvoicePositionIntegrationTests()
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

    // Creates a VM with all required fields filled — avoids service validation failure
    private AddInvoicePositionViewModel MakeFilledVm(string description = "Test Item", Guid? parentId = null)
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

    #region 1. Add normal position

    [Fact(DisplayName = "Integration: Add command saves position to store")]
    public async Task AddCommand_SavesPosition_ToStore()
    {
        var vm = MakeFilledVm("Consulting");
        vm.InvoicePositionDetailsFormViewModel.InvoicePositionQuantity = 2m;
        vm.InvoicePositionDetailsFormViewModel.InvoicePositionUnitPrice = 100m;

        _wpf.Invoke(() => vm.AddInvoicePositionDetailsCommand.Execute(null));

        var result = await _store.GetAllAsync();
        Assert.Single(result.Data);
        Assert.Equal("Consulting", result.Data[0].InvoicePositionDescription);
        Assert.Equal(2m, result.Data[0].InvoicePositionQuantity);
        Assert.Equal(100m, result.Data[0].InvoicePositionUnitPrice);
    }

    [Fact(DisplayName = "Integration: Add command assigns IsStandalonePosition when no parent")]
    public async Task AddCommand_AssignsStandalonePosition_WhenNoParent()
    {
        var vm = MakeFilledVm("Standalone");

        _wpf.Invoke(() => vm.AddInvoicePositionDetailsCommand.Execute(null));

        var result = await _store.GetAllAsync();
        Assert.True(result.Data[0].IsStandalonePosition);
    }

    [Fact(DisplayName = "Integration: multiple adds result in correct count in store")]
    public async Task AddCommand_MultipleAdds_CorrectCountInStore()
    {
        for (var i = 0; i < 3; i++)
        {
            var index = i; // capture loop variable
            var vm = MakeFilledVm($"Item {index}");
            _wpf.Invoke(() => vm.AddInvoicePositionDetailsCommand.Execute(null));
        }

        var result = await _store.GetAllAsync();
        Assert.Equal(3, result.Data.Count);
    }

    #endregion

    #region 2. Add sub-position

    [Fact(DisplayName = "Integration: AddSub command saves DETAIL sub-position under GROUP in store")]
    public async Task AddSubCommand_SavesSubPosition_UnderGroupInStore()
    {
        var groupId = await AddGroupToStoreAsync();
        var vm = MakeFilledVm("Sub Item", parentId: groupId);
        vm.InvoicePositionDetailsFormViewModel.InvoicePositionUnitPrice = 50m;

        _wpf.Invoke(() => vm.AddInvoicePositionDetailsCommand.Execute(null));

        var result = await _store.GetAllAsync();
        var subPosition = result.Data.FirstOrDefault(p => p.IsSubPosition);

        Assert.NotNull(subPosition);
        Assert.Equal("Sub Item", subPosition.InvoicePositionDescription);
        Assert.Equal(groupId, subPosition.ParentPositionId);
        Assert.Equal("DETAIL", subPosition.LineStatusReasonCode);
        Assert.Equal(50m, subPosition.InvoicePositionUnitPrice);
    }

    [Fact(DisplayName = "Integration: AddSub command resets SelectedParentPositionId after save")]
    public async Task AddSubCommand_ResetsSelectedParentPositionId_AfterSave()
    {
        var groupId = await AddGroupToStoreAsync();
        var vm = MakeFilledVm("Sub Item", parentId: groupId);

        _wpf.Invoke(() => vm.AddInvoicePositionDetailsCommand.Execute(null));
        _wpf.Invoke(() => vm.Dispose());

        Assert.Null(_selectionStore.SelectedParentPositionId);
    }

    [Fact(DisplayName = "Integration: store contains GROUP and DETAIL after sub-position add")]
    public async Task AddSubCommand_StoreContainsGroupAndDetail_AfterSubPositionAdd()
    {
        var groupId = await AddGroupToStoreAsync();
        var vm = MakeFilledVm("Sub Item", parentId: groupId);

        _wpf.Invoke(() => vm.AddInvoicePositionDetailsCommand.Execute(null));

        var result = await _store.GetAllAsync();
        Assert.Single(result.Data.Where(p => p.IsGroupPosition));
        Assert.Single(result.Data.Where(p => p.IsSubPosition));
    }

    [Fact(DisplayName = "Integration: multiple sub-positions under same GROUP all appear in store")]
    public async Task AddSubCommand_MultipleSubPositions_AllAppearInStore()
    {
        var groupId = await AddGroupToStoreAsync();

        for (var i = 0; i < 3; i++)
        {
            var index = i;
            var vm = MakeFilledVm($"Sub Item {index}", parentId: groupId);
            _wpf.Invoke(() => vm.AddInvoicePositionDetailsCommand.Execute(null));
        }

        var result = await _store.GetAllAsync();
        var subPositions = result.Data.Where(p => p.IsSubPosition).ToList();

        Assert.Equal(3, subPositions.Count);
        Assert.All(subPositions, p => Assert.Equal(groupId, p.ParentPositionId));
    }

    #endregion

    public void Dispose() => _wpf.Dispose();
}


