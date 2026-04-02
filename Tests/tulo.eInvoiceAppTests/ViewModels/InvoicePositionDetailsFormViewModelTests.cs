using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Windows.Input;
using tulo.CommonMVVM.Collector;
using tulo.CommonMVVM.GlobalProperties;
using tulo.CoreLib.Interfaces.SnapShots;
using tulo.CoreLib.Services;
using tulo.CoreLib.Translators;
using tulo.eInvoice.eInvoiceApp.DTOs;
using tulo.eInvoice.eInvoiceApp.Options;
using tulo.eInvoice.eInvoiceApp.Services;
using tulo.eInvoice.eInvoiceApp.Stores.Invoices;
using tulo.eInvoice.eInvoiceApp.ViewModels.Invoices;
using tulo.eInvoiceAppTests.Fakes;
using tulo.eInvoiceAppTests.TestInfrastructure;

namespace tulo.eInvoiceAppTests.ViewModels;

public class InvoicePositionDetailsFormViewModelTests : IDisposable
{
    private readonly WpfTestContext _wpf = new();
    private readonly ICollectorCollection _collectorCollection;
    private readonly GlobalPropsUiManage _globalProps;
    private readonly FakeSelectedInvoicePositionStore _selectionStore;

    public InvoicePositionDetailsFormViewModelTests()
    {
        _globalProps = new GlobalPropsUiManage();
        _selectionStore = new FakeSelectedInvoicePositionStore();
        _collectorCollection = new CollectorCollection();

        var testTranslations = new Dictionary<string, string>
        {
            ["LabelSaveRequestMessage"] = "Save changes?",
            ["LabelInvoicePositionGroupBox"] = "Invoice Position"
        };

        var translator = new TranslatorUiProvider(testTranslations);
        var appOptions = Options.Create(new AppOptions
        {
            Vats = new VatsOptions
            {
                VatList = new List<int> { 0, 7, 19 }
            }
        });

      

        _collectorCollection.AddService<ISelectedInvoicePositionStore>(_selectionStore);
        _collectorCollection.AddService<IGlobalPropsUiManage>(_globalProps);
        _collectorCollection.AddService<ITranslatorUiProvider>(translator);
        _collectorCollection.AddService<ILoggerFactory>(NullLoggerFactory.Instance);
        _collectorCollection.AddService<IOptions<AppOptions>>(appOptions);
        _collectorCollection.AddService<ISnapShotService>(new SnapShotService());
        _collectorCollection.AddService<IInvoicePositionLookupService>(new InvoicePositionLookupService(_collectorCollection));
    }

    private InvoicePositionDetailsFormViewModel CreateVm() =>
        _wpf.Invoke(() => new InvoicePositionDetailsFormViewModel(
            _collectorCollection,
            new DummyCommand(),
            new DummyCommand()
        ));

    #region 1. Constructor / Initial State
    [Fact(DisplayName = "Constructor: default VAT category is 'S' after construction")]
    public void Constructor_DefaultVatCategory_IsS()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            Assert.NotNull(vm.InvoicePositionSelectedVatCategory);
            Assert.Equal("S", vm.InvoicePositionSelectedVatCategory!.Code);
        });
    }

    [Fact(DisplayName = "Constructor: VatCategoriesObservableCollection has exactly 6 entries")]
    public void Constructor_VatCategoriesCollection_HasSixEntries()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            Assert.Equal(6, vm.VatCategoriesObservableCollection.Count);
        });
    }

    [Fact(DisplayName = "Constructor: VatCategoriesObservableCollection contains all expected codes")]
    public void Constructor_VatCategoriesCollection_ContainsAllExpectedCodes()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();
            var codes = vm.VatCategoriesObservableCollection.Select(x => x.Code).ToList();

            Assert.Contains("S", codes);
            Assert.Contains("Z", codes);
            Assert.Contains("E", codes);
            Assert.Contains("AE", codes);
            Assert.Contains("K", codes);
            Assert.Contains("G", codes);
        });
    }

    [Fact(DisplayName = "Constructor: UnitsObservableCollection is filled with units")]
    public void Constructor_UnitsCollection_IsFilled()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            Assert.NotEmpty(vm.UnitsObservableCollection);
        });
    }

    [Theory(DisplayName = "Constructor: UnitsObservableCollection contains expected unit codes")]
    [InlineData("H87")]
    [InlineData("HUR")]
    [InlineData("KGM")]
    [InlineData("LTR")]
    [InlineData("MTR")]
    [InlineData("DAY")]
    public void Constructor_UnitsCollection_ContainsExpectedCodes(string code)
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            Assert.Contains(vm.UnitsObservableCollection, u => u.Code == code);
        });
    }

    [Fact(DisplayName = "Constructor: VatList is not empty")]
    public void Constructor_VatList_IsNotEmpty()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            Assert.NotEmpty(vm.VatList);
        });
    }

    [Fact(DisplayName = "Constructor: IsEnableToSaveData starts as false")]
    public void Constructor_IsEnableToSaveData_StartsAsFalse()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            Assert.False(vm.IsEnableToSaveData);
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
    #endregion

    #region 2. VatCategoryCode <-> SelectedVatCategory Sync
    [Theory(DisplayName = "InvoicePositionVatCategoryCode: setting a known code syncs SelectedVatCategory")]
    [InlineData("S")]
    [InlineData("Z")]
    [InlineData("E")]
    [InlineData("AE")]
    [InlineData("K")]
    [InlineData("G")]
    public void VatCategoryCode_KnownCode_SyncsSelectedVatCategory(string code)
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            vm.InvoicePositionVatCategoryCode = code;

            Assert.NotNull(vm.InvoicePositionSelectedVatCategory);
            Assert.Equal(code, vm.InvoicePositionSelectedVatCategory!.Code);
        });
    }

    [Fact(DisplayName = "InvoicePositionVatCategoryCode: setting an unknown code clears SelectedVatCategory")]
    public void VatCategoryCode_UnknownCode_ClearsSelectedVatCategory()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            vm.InvoicePositionVatCategoryCode = "UNKNOWN";

            Assert.Null(vm.InvoicePositionSelectedVatCategory);
        });
    }

    [Fact(DisplayName = "InvoicePositionSelectedVatCategory: setting an item syncs VatCategoryCode")]
    public void SelectedVatCategory_Set_SyncsVatCategoryCode()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();
            var item = vm.VatCategoriesObservableCollection.First(x => x.Code == "Z");

            vm.InvoicePositionSelectedVatCategory = item;

            Assert.Equal("Z", vm.InvoicePositionVatCategoryCode);
        });
    }

    [Fact(DisplayName = "InvoicePositionSelectedVatCategory: changing category fires PropertyChanged for SelectedVatCategoryTooltip")]
    public void SelectedVatCategory_Change_FiresPropertyChangedForTooltip()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();
            var changed = new List<string?>();
            vm.PropertyChanged += (_, args) => changed.Add(args.PropertyName);

            vm.InvoicePositionVatCategoryCode = "Z";

            Assert.Contains(nameof(vm.SelectedVatCategoryTooltip), changed);
        });
    }

    [Fact(DisplayName = "InvoicePositionSelectedVatCategory: changing category fires PropertyChanged for SelectedVatCategoryText")]
    public void SelectedVatCategory_Change_FiresPropertyChangedForText()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();
            var changed = new List<string?>();
            vm.PropertyChanged += (_, args) => changed.Add(args.PropertyName);

            // Use the SelectedVatCategory setter directly – this one fires SelectedVatCategoryText
            var item = vm.VatCategoriesObservableCollection.First(x => x.Code == "Z");
            vm.InvoicePositionSelectedVatCategory = item;

            Assert.Contains(nameof(vm.SelectedVatCategoryText), changed);
        });
    }
    #endregion

    #region 3. Unit <-> SelectedUnit Sync
    [Theory(DisplayName = "InvoicePostionUnit: setting a known code syncs InvoicePositionSelectedUnit")]
    [InlineData("H87")]
    [InlineData("HUR")]
    [InlineData("KGM")]
    public void Unit_KnownCode_SyncsSelectedUnit(string code)
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            vm.InvoicePostionUnit = code;

            Assert.NotNull(vm.InvoicePositionSelectedUnit);
            Assert.Equal(code, vm.InvoicePositionSelectedUnit!.Code);
        });
    }

    [Fact(DisplayName = "InvoicePostionUnit: setting an unknown code clears SelectedUnit")]
    public void Unit_UnknownCode_ClearsSelectedUnit()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            vm.InvoicePostionUnit = "UNKNOWN";

            Assert.Null(vm.InvoicePositionSelectedUnit);
        });
    }

    [Fact(DisplayName = "InvoicePostionUnit: empty string clears SelectedUnit")]
    public void Unit_EmptyString_ClearsSelectedUnit()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();
            vm.InvoicePostionUnit = "HUR";

            vm.InvoicePostionUnit = string.Empty;

            Assert.Null(vm.InvoicePositionSelectedUnit);
        });
    }

    [Fact(DisplayName = "InvoicePositionSelectedUnit: setting an item syncs InvoicePostionUnit")]
    public void SelectedUnit_Set_SyncsUnitCode()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();
            var item = vm.UnitsObservableCollection.First(u => u.Code == "KGM");

            vm.InvoicePositionSelectedUnit = item;

            Assert.Equal("KGM", vm.InvoicePostionUnit);
        });
    }
    #endregion

    #region 4. SelectedVat -> VatRate Sync
    [Theory(DisplayName = "InvoicePositionSelectedVat: setting a value syncs InvoicePositionVatRate")]
    [InlineData(0)]
    [InlineData(7)]
    [InlineData(19)]
    public void SelectedVat_Set_SyncsVatRate(int vat)
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            vm.InvoicePositionSelectedVat = vat;

            Assert.Equal(vat, vm.InvoicePositionVatRate);
        });
    }
    #endregion

    #region 5. UpdateToEnableSaveControl – Herzstück
    [Fact(DisplayName = "UpdateToEnableSaveControl: changing Description enables save and sets HasUnsavedChanges")]
    public void Description_Changed_EnablesSaveAndSetsUnsavedChanges()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            vm.InvoicePositionDescription = "New description";

            Assert.True(vm.HasUnsavedChanges);
            Assert.True(vm.IsEnableToSaveData);
        });
    }

    [Fact(DisplayName = "UpdateToEnableSaveControl: changing Quantity enables save and sets HasUnsavedChanges")]
    public void Quantity_Changed_EnablesSave()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            vm.InvoicePositionQuantity = 5m;

            Assert.True(vm.HasUnsavedChanges);
            Assert.True(vm.IsEnableToSaveData);
        });
    }

    [Fact(DisplayName = "UpdateToEnableSaveControl: changing Unit enables save and sets HasUnsavedChanges")]
    public void Unit_Changed_EnablesSave()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            vm.InvoicePostionUnit = "HUR";

            Assert.True(vm.HasUnsavedChanges);
            Assert.True(vm.IsEnableToSaveData);
        });
    }

    [Fact(DisplayName = "UpdateToEnableSaveControl: changing UnitPrice enables save and sets HasUnsavedChanges")]
    public void UnitPrice_Changed_EnablesSave()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            vm.InvoicePositionUnitPrice = 99.99m;

            Assert.True(vm.HasUnsavedChanges);
            Assert.True(vm.IsEnableToSaveData);
        });
    }

    [Fact(DisplayName = "UpdateToEnableSaveControl: changing VatRate enables save and sets HasUnsavedChanges")]
    public void VatRate_Changed_EnablesSave()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            vm.InvoicePositionVatRate = 19;

            Assert.True(vm.HasUnsavedChanges);
            Assert.True(vm.IsEnableToSaveData);
        });
    }

    [Fact(DisplayName = "UpdateToEnableSaveControl: changing GrossAmount enables save and sets HasUnsavedChanges")]
    public void GrossAmount_Changed_EnablesSave()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            vm.InvoicePositionGrossAmount = 119m;

            Assert.True(vm.HasUnsavedChanges);
            Assert.True(vm.IsEnableToSaveData);
        });
    }

    [Fact(DisplayName = "UpdateToEnableSaveControl: changing DiscountReason enables save and sets HasUnsavedChanges")]
    public void DiscountReason_Changed_EnablesSave()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            vm.InvoicePositionDiscountReason = "Rabatt";

            Assert.True(vm.HasUnsavedChanges);
            Assert.True(vm.IsEnableToSaveData);
        });
    }

    [Fact(DisplayName = "UpdateToEnableSaveControl: changing DiscountNetAmount enables save and sets HasUnsavedChanges")]
    public void DiscountNetAmount_Changed_EnablesSave()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            vm.InvoicePositionDiscountNetAmount = 10m;

            Assert.True(vm.HasUnsavedChanges);
            Assert.True(vm.IsEnableToSaveData);
        });
    }

    [Fact(DisplayName = "UpdateToEnableSaveControl: setting same value as snapshot keeps HasUnsavedChanges false")]
    public void SameValueAsSnapshot_KeepsUnsavedChangesFalse()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            // Snapshot has Description = "" so setting to same value = no change
            vm.InvoicePositionDescription = string.Empty;

            Assert.False(vm.HasUnsavedChanges);
            Assert.False(vm.IsEnableToSaveData);
        });
    }

    [Fact(DisplayName = "UpdateToEnableSaveControl: changing then reverting Description disables save again")]
    public void Description_ChangedThenReverted_DisablesSave()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            vm.InvoicePositionDescription = "changed";
            Assert.True(vm.HasUnsavedChanges);

            vm.InvoicePositionDescription = string.Empty;

            Assert.False(vm.HasUnsavedChanges);
            Assert.False(vm.IsEnableToSaveData);
        });
    }

    [Fact(DisplayName = "UpdateToEnableSaveControl: edit mode with snapshot – change triggers HasUnsavedChanges")]
    public void EditMode_WithSnapshot_ChangeTriggersUnsavedChanges()
    {
        _wpf.Invoke(() =>
        {
            // Simulate edit mode: store has a selected position
            _selectionStore.SelectedInvoicePosition = new InvoicePositionDetailsDTO
            {
                Id = Guid.NewGuid(),
                InvoicePositionNr = 1,
                InvoicePositionDescription = "Original",
                InvoicePositionQuantity = 1m,
                InvoicePostionUnit = "H87",
                InvoicePositionUnitPrice = 10m,
                InvoicePositionVatRate = 19,
                InvoicePositionGrossAmount = 11.9m,
                InvoicePositionDiscountReason = string.Empty,
                InvoicePositionDiscountNetAmount = 0m
            };

            var vm = CreateVm();
            Assert.False(vm.HasUnsavedChanges);

            vm.InvoicePositionDescription = "Modified";

            Assert.True(vm.HasUnsavedChanges);
            Assert.True(vm.IsEnableToSaveData);
        });
    }

    [Fact(DisplayName = "UpdateToEnableSaveControl: edit mode – reverting all fields to snapshot disables save")]
    public void EditMode_RevertAllFieldsToSnapshot_DisablesSave()
    {
        _wpf.Invoke(() =>
        {
            var original = new InvoicePositionDetailsDTO
            {
                Id = Guid.NewGuid(),
                InvoicePositionNr = 1,
                InvoicePositionDescription = "Original",
                InvoicePositionQuantity = 1m,
                InvoicePostionUnit = "H87",
                InvoicePositionUnitPrice = 10m,
                InvoicePositionVatRate = 19,
                InvoicePositionGrossAmount = 11.9m,
                InvoicePositionDiscountReason = string.Empty,
                InvoicePositionDiscountNetAmount = 0m
            };
            _selectionStore.SelectedInvoicePosition = original;

            var vm = CreateVm();

            // Manually set all fields to match snapshot (VM does not auto-populate in edit mode)
            vm.InvoicePositionDescription = "Original";
            vm.InvoicePositionQuantity = 1m;
            vm.InvoicePostionUnit = "H87";
            vm.InvoicePositionUnitPrice = 10m;
            vm.InvoicePositionVatRate = 19;
            vm.InvoicePositionGrossAmount = 11.9m;
            vm.InvoicePositionDiscountReason = string.Empty;
            vm.InvoicePositionDiscountNetAmount = 0m;

            // All fields match snapshot → no unsaved changes
            Assert.False(vm.HasUnsavedChanges);

            // Change one field
            vm.InvoicePositionDescription = "Modified";
            Assert.True(vm.HasUnsavedChanges);

            // Revert that field → all fields match again
            vm.InvoicePositionDescription = "Original";
            Assert.False(vm.HasUnsavedChanges);
            Assert.False(vm.IsEnableToSaveData);
        });
    }
    #endregion

    #region 6. GlobalPropsUiManage Sync
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
    #endregion

    #region 7. GlobalPropsUiManage Events -> PropertyChanged
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
    #endregion

    #region 8. Dispose
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

    #region 9. Labels
    [Fact(DisplayName = "Labels: LabelSaveRequestMessage is filled from translator on construction")]
    public void Labels_LabelSaveRequestMessage_FilledFromTranslator()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            Assert.Equal("Save changes?", vm.LabelSaveRequestMessage);
        });
    }

    [Fact(DisplayName = "Labels: LabelInvoicePositionGroupBox is filled from translator on construction")]
    public void Labels_LabelInvoicePositionGroupBox_FilledFromTranslator()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            Assert.Equal("Invoice Position", vm.LabelInvoicePositionGroupBox);
        });
    }
    #endregion

    #region Utilities
    public void Dispose() => _wpf.Dispose();

    /// <summary>
    /// Minimal no-op ICommand for passing as submit/close command to the VM constructor.
    /// </summary>
    private sealed class DummyCommand : ICommand
    {
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) { }
        public event EventHandler? CanExecuteChanged;
    }
    #endregion
}
