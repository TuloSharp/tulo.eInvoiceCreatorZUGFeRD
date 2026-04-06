using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using tulo.CommonMVVM.Collector;
using tulo.CommonMVVM.GlobalProperties;
using tulo.CoreLib.Interfaces.SnapShots;
using tulo.CoreLib.Services;
using tulo.CoreLib.Translators;
using tulo.eInvoiceCreatorZUGFeRD.Commands.Invoices;
using tulo.eInvoiceCreatorZUGFeRD.Options;
using tulo.eInvoiceCreatorZUGFeRD.Services;
using tulo.eInvoiceCreatorZUGFeRD.Stores.Invoices;
using tulo.eInvoiceCreatorZUGFeRD.ViewModels.Invoices;
using tulo.eInvoiceCreatorZUGFeRDTests.Fakes;
using tulo.eInvoiceCreatorZUGFeRDTests.TestInfrastructure;

namespace tulo.eInvoiceCreatorZUGFeRDTests.ViewModels;

public class AddInvoicePositionViewModelTests : IDisposable
{
    private readonly WpfTestContext _wpf = new();
    private readonly ICollectorCollection _collectorCollection;
    private readonly GlobalPropsUiManage _globalProps;
    private readonly SelectedInvoicePositionStore _selectionStore;
    private readonly FakeInvoicePositionService _invoiceService;

    public AddInvoicePositionViewModelTests()
    {
        _globalProps = new GlobalPropsUiManage();
        _invoiceService = new FakeInvoicePositionService();
        _selectionStore = new SelectedInvoicePositionStore(_invoiceService);
        _collectorCollection = new CollectorCollection();

        var testTranslations = new Dictionary<string, string>
        {
            ["LabelCreateInvoicePosition"] = "Create Invoice Position",
            ["ToolTipReturn"] = "Return",
            ["ToolTipSave"] = "Save",
            ["LabelSaveRequestMessage"] = "Save changes?",
            ["LabelInvoicePositionGroupBox"] = "Invoice Position"
        };

        var translator = new TranslatorUiProvider(testTranslations);
        var appOptions = Options.Create(new AppOptions());

        _collectorCollection.AddService<ISelectedInvoicePositionStore>(_selectionStore);
        _collectorCollection.AddService<IGlobalPropsUiManage>(_globalProps);
        _collectorCollection.AddService<ITranslatorUiProvider>(translator);
        _collectorCollection.AddService<IInvoicePositionService>(_invoiceService);
        _collectorCollection.AddService<ILoggerFactory>(NullLoggerFactory.Instance);
        _collectorCollection.AddService<IOptions<AppOptions>>(appOptions);
        _collectorCollection.AddService<ISnapShotService>(new SnapShotService());     
        _collectorCollection.AddService<IInvoicePositionLookupService>(new InvoicePositionLookupService(_collectorCollection));
    }

    private AddInvoicePositionViewModel CreateVm() =>
        _wpf.Invoke(() => new AddInvoicePositionViewModel(_collectorCollection));

    #region 1. Translations / Labels
    [Fact(DisplayName = "Labels: LabelCreateInvoicePosition is filled from translator on construction")]
    public void Labels_LabelCreateInvoicePosition_FilledFromTranslator()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            Assert.Equal("Create Invoice Position", vm.LabelCreateInvoicePosition);
        });
    }

    [Fact(DisplayName = "ToolTips: ToolTipReturn is filled from translator on construction")]
    public void ToolTips_ToolTipReturn_FilledFromTranslator()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            Assert.Equal("Return", vm.ToolTipReturn);
        });
    }

    [Fact(DisplayName = "ToolTips: ToolTipSave is filled from translator on construction")]
    public void ToolTips_ToolTipSave_FilledFromTranslator()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            Assert.Equal("Save", vm.ToolTipSave);
        });
    }
    #endregion

    #region 2. Constructor Defaults
    [Fact(DisplayName = "Constructor: IsRequiredField starts as false")]
    public void Constructor_IsRequiredField_StartsAsFalse()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            Assert.False(vm.IsRequiredField);
        });
    }

    [Fact(DisplayName = "Constructor: HasUnsavedChanges starts as false")]
    public void Constructor_HasUnsavedChanges_StartsAsFalse()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            Assert.False(vm.HasUnsavedChanges);
        });
    }

    [Fact(DisplayName = "Constructor: IsAltShortcutKeyPressed starts as false")]
    public void Constructor_IsAltShortcutKeyPressed_StartsAsFalse()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            Assert.False(vm.IsAltShortcutKeyPressed);
        });
    }

    [Fact(DisplayName = "Constructor: InvoicePositionDetailsFormViewModel is not null")]
    public void Constructor_InvoicePositionDetailsFormViewModel_IsNotNull()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            Assert.NotNull(vm.InvoicePositionDetailsFormViewModel);
        });
    }
    #endregion

    #region 3. InvoicePositionDetailsFormViewModel Initial State
    [Fact(DisplayName = "InvoicePositionDetailsFormViewModel: IsEnable4ShowButtons is true after construction")]
    public void FormViewModel_IsEnable4ShowButtons_TrueAfterConstruction()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            Assert.True(vm.InvoicePositionDetailsFormViewModel.IsEnable4ShowButtons);
        });
    }

    [Fact(DisplayName = "InvoicePositionDetailsFormViewModel: IsRequiredField matches VM on construction")]
    public void FormViewModel_IsRequiredField_MatchesVmOnConstruction()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            Assert.Equal(vm.IsRequiredField, vm.InvoicePositionDetailsFormViewModel.IsRequiredField);
        });
    }

    [Fact(DisplayName = "InvoicePositionDetailsFormViewModel: HasUnsavedChanges matches VM on construction")]
    public void FormViewModel_HasUnsavedChanges_MatchesVmOnConstruction()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            Assert.Equal(vm.HasUnsavedChanges, vm.InvoicePositionDetailsFormViewModel.HasUnsavedChanges);
        });
    }
    #endregion

    #region 4. Property <-> GlobalPropsUiManage Sync
    [Fact(DisplayName = "IsEnableToSaveData: setting true updates GlobalPropsUiManage")]
    public void IsEnableToSaveData_SetTrue_UpdatesGlobalProps()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            vm.IsEnableToSaveData = true;

            Assert.True(_globalProps.IsEnableToSaveData);
        });
    }

    [Fact(DisplayName = "IsEnableToSaveData: setting false updates GlobalPropsUiManage")]
    public void IsEnableToSaveData_SetFalse_UpdatesGlobalProps()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();
            vm.IsEnableToSaveData = true;

            vm.IsEnableToSaveData = false;

            Assert.False(_globalProps.IsEnableToSaveData);
        });
    }

    [Fact(DisplayName = "IsRequiredField: setting true updates GlobalPropsUiManage")]
    public void IsRequiredField_SetTrue_UpdatesGlobalProps()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            vm.IsRequiredField = true;

            Assert.True(_globalProps.IsRequiredField);
        });
    }

    [Fact(DisplayName = "HasUnsavedChanges: setting true updates GlobalPropsUiManage")]
    public void HasUnsavedChanges_SetTrue_UpdatesGlobalProps()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            vm.HasUnsavedChanges = true;

            Assert.True(_globalProps.HasUnsavedChanges);
        });
    }

    [Fact(DisplayName = "IsSaveRequestMessageVisible: setting true updates GlobalPropsUiManage")]
    public void IsSaveRequestMessageVisible_SetTrue_UpdatesGlobalProps()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            vm.IsSaveRequestMessageVisible = true;

            Assert.True(_globalProps.IsSaveRequestMessageVisible);
        });
    }

    [Fact(DisplayName = "IsAltShortcutKeyPressed: setting true updates GlobalPropsUiManage")]
    public void IsAltShortcutKeyPressed_SetTrue_UpdatesGlobalProps()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            vm.IsAltShortcutKeyPressed = true;

            Assert.True(_globalProps.IsAltShortcutKeyPressed);
        });
    }
    #endregion

    #region 5. GlobalPropsUiManage Events -> VM PropertyChanged
    [Fact(DisplayName = "IsEnableToSaveDataChanged event: VM fires PropertyChanged for IsEnableToSaveData")]
    public void IsEnableToSaveDataChanged_Event_FiresPropertyChanged()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();
            var changed = new List<string?>();
            vm.PropertyChanged += (_, args) => changed.Add(args.PropertyName);

            _globalProps.IsEnableToSaveData = true;

            Assert.Contains(nameof(vm.IsEnableToSaveData), changed);
        });
    }

    [Fact(DisplayName = "IsRequiredFieldChanged event: VM fires PropertyChanged for IsRequiredField")]
    public void IsRequiredFieldChanged_Event_FiresPropertyChanged()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();
            var changed = new List<string?>();
            vm.PropertyChanged += (_, args) => changed.Add(args.PropertyName);

            _globalProps.IsRequiredField = true;

            Assert.Contains(nameof(vm.IsRequiredField), changed);
        });
    }

    [Fact(DisplayName = "HasUnsavedChangesChanged event: VM fires PropertyChanged for HasUnsavedChanges")]
    public void HasUnsavedChangesChanged_Event_FiresPropertyChanged()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();
            var changed = new List<string?>();
            vm.PropertyChanged += (_, args) => changed.Add(args.PropertyName);

            _globalProps.HasUnsavedChanges = true;

            Assert.Contains(nameof(vm.HasUnsavedChanges), changed);
        });
    }

    [Fact(DisplayName = "IsSaveRequestMessageVisibleChanged event: VM fires PropertyChanged for IsSaveRequestMessageVisible")]
    public void IsSaveRequestMessageVisibleChanged_Event_FiresPropertyChanged()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();
            var changed = new List<string?>();
            vm.PropertyChanged += (_, args) => changed.Add(args.PropertyName);

            _globalProps.IsSaveRequestMessageVisible = true;

            Assert.Contains(nameof(vm.IsSaveRequestMessageVisible), changed);
        });
    }

    [Fact(DisplayName = "IsAltShortcutKeyPressedChanged event: VM updates IsAltShortcutKeyPressed from GlobalProps")]
    public void IsAltShortcutKeyPressedChanged_Event_UpdatesVmProperty()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();
            Assert.False(vm.IsAltShortcutKeyPressed);

            _globalProps.IsAltShortcutKeyPressed = true;

            Assert.True(vm.IsAltShortcutKeyPressed);
        });
    }
    #endregion

    #region 6. Dispose
    [Fact(DisplayName = "Dispose: after dispose, GlobalPropsUiManage events no longer affect VM")]
    public void Dispose_EventsNoLongerAffectVm()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();
            var changed = new List<string?>();
            vm.PropertyChanged += (_, args) => changed.Add(args.PropertyName);

            vm.Dispose();
            changed.Clear();

            // Fire events after dispose – VM should not react
            _globalProps.IsEnableToSaveData = true;
            _globalProps.IsRequiredField = true;
            _globalProps.HasUnsavedChanges = true;
            _globalProps.IsSaveRequestMessageVisible = true;

            Assert.DoesNotContain(nameof(vm.IsEnableToSaveData), changed);
            Assert.DoesNotContain(nameof(vm.IsRequiredField), changed);
            Assert.DoesNotContain(nameof(vm.HasUnsavedChanges), changed);
            Assert.DoesNotContain(nameof(vm.IsSaveRequestMessageVisible), changed);
        });
    }
    #endregion

    #region 7. Sub-Position Context
    [Fact(DisplayName = "IsSubPosition: false when SelectedParentPositionId is null (normal position context)")]
    public void IsSubPosition_False_WhenSelectedParentPositionIdIsNull()
    {
        _wpf.Invoke(() =>
        {
            _selectionStore.SelectedParentPositionId = null;
            var vm = CreateVm();

            Assert.False(vm.IsSubPosition);
        });
    }

    [Fact(DisplayName = "IsSubPosition: true when SelectedParentPositionId is set before construction")]
    public void IsSubPosition_True_WhenSelectedParentPositionIdIsSet()
    {
        _wpf.Invoke(() =>
        {
            _selectionStore.SelectedParentPositionId = Guid.NewGuid();
            var vm = CreateVm();

            Assert.True(vm.IsSubPosition);
        });
    }

    [Fact(DisplayName = "ParentPositionId: null when SelectedParentPositionId is not set")]
    public void ParentPositionId_Null_WhenStoreHasNoParent()
    {
        _wpf.Invoke(() =>
        {
            _selectionStore.SelectedParentPositionId = null;
            var vm = CreateVm();

            Assert.Null(vm.ParentPositionId);
        });
    }

    [Fact(DisplayName = "ParentPositionId: matches store value when SelectedParentPositionId is set")]
    public void ParentPositionId_MatchesStoreValue_WhenSet()
    {
        _wpf.Invoke(() =>
        {
            var parentId = Guid.NewGuid();
            _selectionStore.SelectedParentPositionId = parentId;
            var vm = CreateVm();

            Assert.Equal(parentId, vm.ParentPositionId);
        });
    }

    [Fact(DisplayName = "AddInvoicePositionDetailsCommand: is AddInvoicePositionDetailsCommand type when IsSubPosition is false")]
    public void Command_IsAddInvoicePositionDetailsCommand_WhenNotSubPosition()
    {
        _wpf.Invoke(() =>
        {
            _selectionStore.SelectedParentPositionId = null;
            var vm = CreateVm();

            Assert.IsType<AddInvoicePositionDetailsCommand>(vm.AddInvoicePositionDetailsCommand);
        });
    }

    [Fact(DisplayName = "AddInvoicePositionDetailsCommand: is AddSubInvoicePositionDetailsCommand type when IsSubPosition is true")]
    public void Command_IsAddSubInvoicePositionDetailsCommand_WhenIsSubPosition()
    {
        _wpf.Invoke(() =>
        {
            _selectionStore.SelectedParentPositionId = Guid.NewGuid();
            var vm = CreateVm();

            Assert.IsType<AddSubInvoicePositionDetailsCommand>(vm.AddInvoicePositionDetailsCommand);
        });
    }

    [Fact(DisplayName = "Dispose: resets SelectedParentPositionId in store to null")]
    public void Dispose_ResetsSelectedParentPositionIdToNull()
    {
        _wpf.Invoke(() =>
        {
            _selectionStore.SelectedParentPositionId = Guid.NewGuid();
            var vm = CreateVm();

            vm.Dispose();

            Assert.Null(_selectionStore.SelectedParentPositionId);
        });
    }
    #endregion


    #region Utilities
    public void Dispose() => _wpf.Dispose();
    #endregion
}
