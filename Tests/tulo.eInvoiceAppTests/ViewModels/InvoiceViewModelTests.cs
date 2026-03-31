using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using tulo.CommonMVVM.Collector;
using tulo.CommonMVVM.GlobalProperties;
using tulo.CoreLib.Translators;
using tulo.eInvoice.eInvoiceApp.DTOs;
using tulo.eInvoice.eInvoiceApp.Options;
using tulo.eInvoice.eInvoiceApp.Services;
using tulo.eInvoice.eInvoiceApp.Stores.Invoices;
using tulo.eInvoice.eInvoiceApp.ViewModels.Invoices;
using tulo.eInvoiceAppTests.Fakes;
using tulo.eInvoiceAppTests.TestInfrastructure;

namespace tulo.eInvoiceAppTests.ViewModels;

public class InvoiceViewModelTests : IDisposable
{
    private readonly WpfTestContext _wpf = new();
    private readonly FakeInvoicePositionService _invoiceService;
    private readonly FakeSelectedInvoicePositionStore _selectionStore;
    private readonly ICollectorCollection _collector;

    public InvoiceViewModelTests()
    {
        _invoiceService = new FakeInvoicePositionService();
        _selectionStore = new FakeSelectedInvoicePositionStore();
        _collector = new CollectorCollection();

        var testTranslations = new Dictionary<string, string>
        {
            ["ToolTipInvoiceNumber"] = "Invoice number tooltip",
            ["ToolTipPaymentMeansCode_58"] = "Payment means 58",
            ["PlaceholderDiscountPreviewText"] = "Pay until dd.MM.yyyy with ...% discount"
        };

        var translator = new TranslatorUiProvider(testTranslations);

        _collector.AddService<IInvoicePositionService>(_invoiceService);
        _collector.AddService<ISelectedInvoicePositionStore>(_selectionStore);
        _collector.AddService<ITranslatorUiProvider>(translator);
        _collector.AddService<IGlobalPropsUiManage>(new GlobalPropsUiManage());
        _collector.AddService<IAppOptions>(new AppOptions());
        _collector.AddService<ILoggerFactory>(NullLoggerFactory.Instance);
    }

    private InvoiceViewModel CreateVm() =>
        _wpf.Invoke(() => new InvoiceViewModel(_collector));

    [Fact]
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

    [Fact]
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

    [Fact]
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

    public void Dispose() => _wpf.Dispose();

    #region Utilities
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
