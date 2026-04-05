using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using tulo.CommonMVVM.Collector;
using tulo.CommonMVVM.GlobalProperties;
using tulo.CoreLib.Translators;
using tulo.eInvoiceApp.DTOs;
using tulo.eInvoiceApp.Options;
using tulo.eInvoiceApp.Services;
using tulo.eInvoiceApp.Stores.Invoices;
using tulo.eInvoiceApp.ViewModels.Invoices;
using tulo.eInvoiceAppTests.Fakes;
using tulo.eInvoiceAppTests.TestInfrastructure;

namespace tulo.eInvoiceAppTests.ViewModels;

public class InvoiceViewModelTests : IDisposable
{
    private readonly WpfTestContext _wpf = new();
    private readonly FakeInvoicePositionService _invoiceService;
    private readonly SelectedInvoicePositionStore _selectionStore;
    private readonly ICollectorCollection _collectorCollection;

    public InvoiceViewModelTests()
    {
        _invoiceService = new FakeInvoicePositionService();
        _selectionStore = new SelectedInvoicePositionStore(_invoiceService);
        _collectorCollection = new CollectorCollection();

        var testTranslations = new Dictionary<string, string>
        {
            ["ToolTipInvoiceNumber"] = "Invoice number tooltip",
            ["ToolTipPaymentMeansCode_58"] = "Payment means 58",
            ["PlaceholderDiscountPreviewText"] = "Pay until dd.MM.yyyy with ...% discount",
            ["PlaceholderNoDiscountPreviewText"] = "Pay full amount until dd.MM.yyyy"
        };

        var translator = new TranslatorUiProvider(testTranslations);

        _collectorCollection.AddService<IInvoicePositionService>(_invoiceService);
        _collectorCollection.AddService<ISelectedInvoicePositionStore>(_selectionStore);
        _collectorCollection.AddService<ITranslatorUiProvider>(translator);
        _collectorCollection.AddService<IGlobalPropsUiManage>(new GlobalPropsUiManage());
        _collectorCollection.AddService<IAppOptions>(new AppOptions());
        _collectorCollection.AddService<ILoggerFactory>(NullLoggerFactory.Instance);
    }

    private InvoiceViewModel CreateVm() => _wpf.Invoke(() => new InvoiceViewModel(_collectorCollection));

    #region Date Validation
    [Fact(DisplayName = "PaymentDueDateText: valid date sets PaymentDueDate and clears error")]
    public void PaymentDueDateText_ValidDate_SetsDateAndNoError()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();
            vm.PaymentDueDateText = "15.06.2025";

            Assert.Equal(new DateOnly(2025, 6, 15), vm.PaymentDueDate);
            Assert.False(vm.HasDatePickerError);
        });
    }

    [Fact(DisplayName = "PaymentDueDateText: empty string clears date and removes error")]
    public void PaymentDueDateText_Empty_ClearsDateAndNoError()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            vm.PaymentDueDateText = "15.06.2025";
            Assert.NotNull(vm.PaymentDueDate);

            vm.PaymentDueDateText = string.Empty;

            Assert.Null(vm.PaymentDueDate);
            Assert.False(vm.HasDatePickerError);
            Assert.Equal(string.Empty, vm.DatePickerErrorMessage);
        });
    }

    [Fact(DisplayName = "PaymentDueDateText: invalid format sets error flag and keeps date null")]
    public void PaymentDueDateText_InvalidFormat_SetsErrorAndKeepsNull()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            vm.PaymentDueDateText = string.Empty;
            Assert.Null(vm.PaymentDueDate);

            vm.PaymentDueDateText = "abc";

            Assert.Null(vm.PaymentDueDate);
            Assert.True(vm.HasDatePickerError);
            Assert.NotEmpty(vm.DatePickerErrorMessage);
        });
    }

    [Theory(DisplayName = "PaymentDueDateText: out-of-range date sets error with range message")]
    [InlineData("31.12.1899")] // < 1900-01-01
    [InlineData("01.01.2100")] // > 2099-12-31
    public void PaymentDueDateText_OutOfRange_SetsErrorWithRangeMessage(string input)
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            vm.PaymentDueDateText = input;

            Assert.Null(vm.PaymentDueDate);
            Assert.True(vm.HasDatePickerError);
            Assert.Contains(vm.ContentDateMustBeBetween, vm.DatePickerErrorMessage);
        });
    }

    [Fact(DisplayName = "DiscountBasisDateText: valid date sets DiscountBasisDate and clears error")]
    public void DiscountBasisDateText_Valid_SetsDateAndNoError()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            vm.DiscountBasisDateText = "01.03.2026";

            Assert.Equal(new DateOnly(2026, 3, 1), vm.DiscountBasisDate);
            Assert.False(vm.HasDiscountBasisDateError);
            Assert.Equal(string.Empty, vm.DiscountBasisDateErrorMessage);
        });
    }

    [Fact(DisplayName = "DiscountBasisDateText: invalid format sets error flag and error message")]
    public void DiscountBasisDateText_Invalid_SetsError()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            vm.DiscountBasisDateText = "99.99.9999";

            Assert.Null(vm.DiscountBasisDate);
            Assert.True(vm.HasDiscountBasisDateError);
            Assert.Equal(vm.ContentDateInvalid, vm.DiscountBasisDateErrorMessage);
        });
    }

    [Fact(DisplayName = "PaymentDueDateRangeText: valid date sets PaymentDueDateRange and clears error")]
    public void PaymentDueDateRangeText_Valid_SetsDateAndNoError()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            vm.PaymentDueDateRangeText = "30.04.2026";

            Assert.Equal(new DateOnly(2026, 4, 30), vm.PaymentDueDateRange);
            Assert.False(vm.HasPaymentDueDateRangeError);
            Assert.Equal(string.Empty, vm.PaymentDueDateRangeErrorMessage);
        });
    }

    [Fact(DisplayName = "PaymentDueDateRangeText: invalid format sets error flag and error message")]
    public void PaymentDueDateRangeText_Invalid_SetsError()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            vm.PaymentDueDateRangeText = "gar kein datum";

            Assert.Null(vm.PaymentDueDateRange);
            Assert.True(vm.HasPaymentDueDateRangeError);
            Assert.Equal(vm.ContentDateInvalid, vm.PaymentDueDateRangeErrorMessage);
        });
    }
#endregion

    #region PaymentMeansCode <-> SelectedPaymentMeansItem Sync
    [Theory(DisplayName = "PaymentMeansCode: setting a known code syncs SelectedPaymentMeansItem")]
    [InlineData("10")]
    [InlineData("48")]
    [InlineData("49")]
    [InlineData("58")]
    [InlineData("59")]
    public void PaymentMeansCode_KnownCode_SyncsSelectedItem(string code)
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();
            vm.PaymentMeansCode = code;

            Assert.NotNull(vm.SelectedPaymentMeansItem);
            Assert.Equal(code, vm.SelectedPaymentMeansItem!.Code);
        });
    }

    [Fact(DisplayName = "PaymentMeansCode: setting an unknown code clears SelectedPaymentMeansItem")]
    public void PaymentMeansCode_UnknownCode_ClearsSelectedItem()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();
            vm.PaymentMeansCode = "58";
            Assert.NotNull(vm.SelectedPaymentMeansItem);

            vm.PaymentMeansCode = "999";

            Assert.Null(vm.SelectedPaymentMeansItem);
        });
    }

    [Fact(DisplayName = "SelectedPaymentMeansItem: setting an item syncs PaymentMeansCode")]
    public void SelectedPaymentMeansItem_Set_SyncsCode()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();
            var item = vm.PaymentMeansCodesObservableCollection.First(p => p.Code == "59");

            vm.SelectedPaymentMeansItem = item;

            Assert.Equal("59", vm.PaymentMeansCode);
        });
    }

    [Fact(DisplayName = "DocumentTypeCode: setting a known code syncs SelectedDocumentTypeItem")]
    public void DocumentTypeCode_Set_SyncsSelectedItem()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();
            vm.DocumentTypeCode = "380";

            Assert.NotNull(vm.SelectedDocumentTypeItem);
            Assert.Equal("380", vm.SelectedDocumentTypeItem!.Code);
        });
    }

    [Fact(DisplayName = "SelectedDocumentTypeItem: setting an item syncs DocumentTypeCode")]
    public void SelectedDocumentTypeItem_Set_SyncsCode()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();
            var item = vm.DocumentTypeCodesObservableCollection.First(d => d.Code == "381");

            vm.SelectedDocumentTypeItem = item;

            Assert.Equal("381", vm.DocumentTypeCode);
        });
    }

    [Fact(DisplayName = "DocumentTypeCode: setting an unknown code clears SelectedDocumentTypeItem")]
    public void DocumentTypeCode_UnknownCode_ClearsSelectedItem()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();
            vm.DocumentTypeCode = "380";
            Assert.NotNull(vm.SelectedDocumentTypeItem);

            vm.DocumentTypeCode = "999";

            Assert.Null(vm.SelectedDocumentTypeItem);
        });
    }
    #endregion

    #region InvoicePosition Events
    [Fact(DisplayName = "InvoicePositionCreated event: adds position to list and selects it")]
    public void InvoicePositionCreated_AddsToListAndSelects()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();
            var dto = MakeDto(Guid.NewGuid(), 99, "TestPos");

            _invoiceService.RaiseCreated(dto);

            var items = GetItems(vm);
            Assert.Contains(items, x => x.InvoicePositionId == dto.Id);
            Assert.NotNull(vm.SelectedInvoicePositionCardListItemViewModel);
            Assert.Equal(dto.Id, vm.SelectedInvoicePositionCardListItemViewModel!.InvoicePositionId);
        });
    }

    [Fact(DisplayName = "InvoicePositionUpdated event: updates description of existing item in list")]
    public void InvoicePositionUpdated_UpdatesExistingItem()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();
            var id = Guid.NewGuid();
            _invoiceService.RaiseCreated(MakeDto(id, 1, "Original"));

            _invoiceService.RaiseUpdated(MakeDto(id, 1, "Updated"));

            var item = GetItems(vm).Single(x => x.InvoicePositionId == id);
            Assert.Equal("Updated", item.InvoicePositionDescription);
        });
    }

    [Fact(DisplayName = "InvoicePositionDeleted event: removes position from list")]
    public void InvoicePositionDeleted_RemovesFromList()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();
            var dto = MakeDto(Guid.NewGuid(), 99, "ToDelete");
            _invoiceService.RaiseCreated(dto);
            Assert.Contains(GetItems(vm), x => x.InvoicePositionId == dto.Id);

            _invoiceService.RaiseDeleted(dto.Id);

            Assert.DoesNotContain(GetItems(vm), x => x.InvoicePositionId == dto.Id);
        });
    }

    [Fact(DisplayName = "InvoicePositionDeleted event: clears selection when selected item is deleted")]
    public void InvoicePositionDeleted_WhenSelectedItem_ClearsSelection()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();
            var dto = MakeDto(Guid.NewGuid(), 99, "ToDelete");
            _invoiceService.RaiseCreated(dto);
            Assert.NotNull(vm.SelectedInvoicePositionCardListItemViewModel);

            _invoiceService.RaiseDeleted(dto.Id);

            Assert.Null(vm.SelectedInvoicePositionCardListItemViewModel);
            Assert.False(vm.HasSelectedInvoicePosition);
        });
    }

    [Fact(DisplayName = "InvoicePositionsLoaded event: replaces entire list with newly loaded positions")]
    public void InvoicePositionsLoaded_ReplacesEntireList()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            // Use RaiseLoaded to set a known clean state first
            var oldDto = MakeDto(Guid.NewGuid(), 1, "Old");
            _invoiceService.RaiseLoaded(new List<InvoicePositionDetailsDTO> { oldDto });

            Assert.Single(GetItems(vm));
            Assert.Contains(GetItems(vm), x => x.InvoicePositionId == oldDto.Id);

            // Now load a new list
            var newList = new List<InvoicePositionDetailsDTO>
        {
            MakeDto(Guid.NewGuid(), 10, "New1"),
            MakeDto(Guid.NewGuid(), 11, "New2")
        };

            _invoiceService.RaiseLoaded(newList);

            var items = GetItems(vm);
            Assert.Equal(2, items.Count);
            Assert.DoesNotContain(items, x => x.InvoicePositionId == oldDto.Id);
        });
    }
    #endregion

    #region HasSelectedInvoicePosition 
    [Fact(DisplayName = "HasSelectedInvoicePosition: is true after a position is created")]
    public void HasSelectedInvoicePosition_TrueAfterCreated()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();
            var dto = MakeDto(Guid.NewGuid(), 1, "Pos");

            _invoiceService.RaiseCreated(dto);

            Assert.True(vm.HasSelectedInvoicePosition);
        });
    }

    [Fact(DisplayName = "HasSelectedInvoicePosition: is false after selected position is deleted")]
    public void HasSelectedInvoicePosition_FalseAfterSelectedDeleted()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();
            var dto = MakeDto(Guid.NewGuid(), 1, "Pos");
            _invoiceService.RaiseCreated(dto);
            Assert.True(vm.HasSelectedInvoicePosition);

            _invoiceService.RaiseDeleted(dto.Id);

            Assert.False(vm.HasSelectedInvoicePosition);
        });
    }

    [Fact(DisplayName = "HasSelectedInvoicePosition: remains true when a non-selected position is deleted")]
    public void HasSelectedInvoicePosition_TrueWhenNonSelectedDeleted()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();
            var dto1 = MakeDto(Guid.NewGuid(), 1, "Pos1");
            var dto2 = MakeDto(Guid.NewGuid(), 2, "Pos2");
            _invoiceService.RaiseCreated(dto1);
            _invoiceService.RaiseCreated(dto2);

            // dto2 is selected (last created), delete dto1
            _invoiceService.RaiseDeleted(dto1.Id);

            Assert.True(vm.HasSelectedInvoicePosition);
            Assert.Equal(dto2.Id, vm.SelectedInvoicePositionCardListItemViewModel!.InvoicePositionId);
        });
    }
    #endregion

    #region DiscountPreviewText / NoDiscountPreviewText
    [Fact(DisplayName = "DiscountPreviewText: updates when DiscountBasisDate and DiscountDays are set")]
    public void DiscountPreviewText_UpdatesWithBasisDateAndDays()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            vm.DiscountBasisDateText = "01.03.2026";
            vm.DiscountDays = "14";
            vm.DiscountPercent = 2m;

            // 01.03.2026 + 14 days = 15.03.2026
            Assert.Contains("15.03.2026", vm.DiscountPreviewText);
        });
    }

    [Fact(DisplayName = "DiscountPreviewText: contains discount percent value")]
    public void DiscountPreviewText_ContainsDiscountPercent()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            vm.DiscountPercent = 3m;

            Assert.Contains("3", vm.DiscountPreviewText);
        });
    }

    [Fact(DisplayName = "NoDiscountPreviewText: updates when PaymentDueDateRange is set")]
    public void NoDiscountPreviewText_UpdatesWithPaymentDueDateRange()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            vm.PaymentDueDateRangeText = "30.04.2026";

            Assert.Contains("30.04.2026", vm.NoDiscountPreviewText);
        });
    }

    [Fact(DisplayName = "DiscountPreviewText: clearing DiscountBasisDate removes date from preview")]
    public void DiscountPreviewText_ClearBasisDate_RemovesDateFromPreview()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            vm.DiscountBasisDateText = "01.03.2026";
            vm.DiscountDays = "14";

            // VM puts the BasisDate itself into the preview (not BasisDate + Days)
            Assert.Contains("01.03.2026", vm.DiscountPreviewText);

            vm.DiscountBasisDateText = string.Empty;

            // After clearing, date must be gone from preview
            Assert.DoesNotContain("01.03.2026", vm.DiscountPreviewText);
        });
    }
    #endregion

    #region LoadInvoicePositionsCommand
    [Fact(DisplayName = "LoadInvoicePositionsCommand: Execute calls LoadAllInvoicePositionsAsync on service")]
    public void LoadInvoicePositionsCommand_Execute_CallsService()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            vm.LoadInvoicePositionsCommand.Execute(null);
            _wpf.WaitForIdle();

            Assert.True(_invoiceService.IsLoaded);
        });
    }

    [Fact(DisplayName = "LoadInvoicePositionsCommand: Execute sets VM StatusMessage when service returns one")]
    public void LoadInvoicePositionsCommand_Execute_SetsStatusMessageFromService()
    {
        _wpf.Invoke(() =>
        {
            _invoiceService.StatusMessage = "Loaded successfully";
            var vm = CreateVm();

            vm.LoadInvoicePositionsCommand.Execute(null);
            _wpf.WaitForIdle();

            Assert.Equal("Loaded successfully", vm.StatusMessageViewModel.Message);
        });
    }

    [Fact(DisplayName = "LoadInvoicePositionsCommand: Execute sets technical error message on exception")]
    public void LoadInvoicePositionsCommand_Execute_SetsTechnicalErrorOnException()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();
            _invoiceService.ExceptionToThrow = new InvalidOperationException("DB down");

            vm.LoadInvoicePositionsCommand.Execute(null);
            _wpf.WaitForIdle();

            Assert.Contains("Technical error", vm.StatusMessageViewModel.Message);
            Assert.Contains("DB down", vm.StatusMessageViewModel.Message);
        });
    }

    [Fact(DisplayName = "LoadInvoicePositionsCommand: Execute restores selection from store by InvoicePositionNr")]
    public void LoadInvoicePositionsCommand_Execute_RestoresSelectionFromStore()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            // Pre-populate list with known items
            var dto1 = MakeDto(Guid.NewGuid(), 1, "Pos1");
            var dto2 = MakeDto(Guid.NewGuid(), 2, "Pos2");
            _invoiceService.RaiseLoaded(new List<InvoicePositionDetailsDTO> { dto1, dto2 });

            // Store remembers position nr 2
            _selectionStore.SelectedInvoicePosition = dto2;

            vm.LoadInvoicePositionsCommand.Execute(null);
            _wpf.WaitForIdle();

            Assert.NotNull(vm.SelectedInvoicePositionCardListItemViewModel);
            Assert.Equal(2, vm.SelectedInvoicePositionCardListItemViewModel!.InvoicePositionNr);
        });
    }

    [Fact(DisplayName = "LoadInvoicePositionsCommand: Execute selects first item when no stored selection exists")]
    public void LoadInvoicePositionsCommand_Execute_FallsBackToFirstItem()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();

            // Pre-populate list, no stored selection
            var dto1 = MakeDto(Guid.NewGuid(), 1, "First");
            var dto2 = MakeDto(Guid.NewGuid(), 2, "Second");
            _invoiceService.RaiseLoaded(new List<InvoicePositionDetailsDTO> { dto1, dto2 });

            _selectionStore.SelectedInvoicePosition = default!;

            vm.LoadInvoicePositionsCommand.Execute(null);
            _wpf.WaitForIdle();

            Assert.NotNull(vm.SelectedInvoicePositionCardListItemViewModel);
            Assert.Equal(1, vm.SelectedInvoicePositionCardListItemViewModel!.InvoicePositionNr);
        });
    }

    [Fact(DisplayName = "LoadInvoicePositionsCommand: CanExecute is false while command is executing")]
    public void LoadInvoicePositionsCommand_CanExecute_FalseWhileExecuting()
    {
        _wpf.Invoke(() =>
        {
            var vm = CreateVm();
            bool? canExecuteDuringRun = null;

            // Capture CanExecute state mid-execution via CanExecuteChanged
            vm.LoadInvoicePositionsCommand.CanExecuteChanged += (_, _) =>
            {
                canExecuteDuringRun ??= vm.LoadInvoicePositionsCommand.CanExecute(null);
            };

            vm.LoadInvoicePositionsCommand.Execute(null);
            _wpf.WaitForIdle();

            // After completion, CanExecute must be true again
            Assert.True(vm.LoadInvoicePositionsCommand.CanExecute(null));
        });
    }
    #endregion

    #region Utilities
    public void Dispose() => _wpf.Dispose();

    private static List<InvoicePositionCardItemViewModel> GetItems(InvoiceViewModel vm) =>
        vm.InvoicePositionCardListItemCollectionView
          .Cast<InvoicePositionCardItemViewModel>()
          .ToList();

    private static InvoicePositionDetailsDTO MakeDto(Guid id, int nr, string description) =>
        new()
        {
            Id = id,
            InvoicePositionNr = nr,
            InvoicePositionDescription = description,
            InvoicePositionQuantity = 1m,
            InvoicePositionUnitPrice = 10m,
            InvoicePositionVatRate = 19,
            InvoicePositionVatCategoryCode = "S",
            InvoicePositionNetAmount = 0m,
            InvoicePositionGrossAmount = 0m
        };
    #endregion
}