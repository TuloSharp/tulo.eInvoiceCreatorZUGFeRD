using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
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

namespace tulo.eInvoiceAppTests.Services;

public class InvoiceBuilderServiceTests : IDisposable
{
    private readonly WpfTestContext _wpf = new();
    private readonly ICollectorCollection _collector;
    private readonly IInvoicePositionStore _store;
    private readonly AppOptions _appOptions;

    public InvoiceBuilderServiceTests()
    {
        _appOptions = new AppOptions();
        _store = new InvoicePositionStore();
        var fakeInvoicePositionService = new FakeInvoicePositionService();

        _collector = new CollectorCollection();
        _collector.AddService<IInvoicePositionStore>(_store);
        _collector.AddService<IOptions<AppOptions>>(Options.Create(_appOptions));
        _collector.AddService<ISelectedInvoicePositionStore>(new SelectedInvoicePositionStore(fakeInvoicePositionService));
        _collector.AddService<IGlobalPropsUiManage>(new GlobalPropsUiManage());
        _collector.AddService<ITranslatorUiProvider>(new TranslatorUiProvider(new Dictionary<string, string>()));
        _collector.AddService<IInvoicePositionService>(new FakeInvoicePositionService());
        _collector.AddService<ILoggerFactory>(NullLoggerFactory.Instance);
    }

    private InvoiceBuilderService CreateService() => new(_collector);
    private InvoiceViewModel CreateVm() => _wpf.Invoke(() => new InvoiceViewModel(_collector));

    private static InvoicePositionDetailsDTO MakeDto(
        string description = "Test Item",
        decimal quantity = 1m,
        string unit = "H87",
        decimal unitPrice = 100m,
        int vatRate = 19,
        string vatCategoryCode = "S",
        decimal? netAmountAfterDiscount = null,
        decimal netAmount = 100m) => new()
        {
            InvoicePositionDescription = description,
            InvoicePositionQuantity = quantity,
            InvoicePostionUnit = unit,
            InvoicePositionUnitPrice = unitPrice,
            InvoicePositionVatRate = vatRate,
            InvoicePositionVatCategoryCode = vatCategoryCode,
            InvoicePositionNetAmountAfterDiscount = netAmountAfterDiscount,
            InvoicePositionNetAmount = netAmount,
        };

    #region 1. BuildAsync – Null Check

    [Fact(DisplayName = "BuildAsync: null VM returns empty Invoice without throwing")]
    public async Task BuildAsync_NullVm_ReturnsEmptyInvoice()
    {
        var result = await CreateService().BuildAsync(null!);

        Assert.NotNull(result);
        Assert.Empty(result.Lines);
    }

    #endregion

    #region 2. FillInvoiceFromViewModel – Header

    [Fact(DisplayName = "FillInvoiceFromViewModel: InvoiceNumber is mapped and trimmed")]
    public async Task FillInvoiceFromViewModel_InvoiceNumber_IsMappedAndTrimmed()
    {
        var vm = _wpf.Invoke(() => { var v = CreateVm(); v.InvoiceNumber = " INV-001 "; return v; });

        var result = await CreateService().BuildAsync(vm);

        Assert.Equal("INV-001", result.InvoiceNumber);
    }

    [Fact(DisplayName = "FillInvoiceFromViewModel: Currency is mapped")]
    public async Task FillInvoiceFromViewModel_Currency_IsMapped()
    {
        var vm = _wpf.Invoke(() => { var v = CreateVm(); v.Currency = "EUR"; return v; });

        var result = await CreateService().BuildAsync(vm);

        Assert.Equal("EUR", result.Currency);
    }

    [Fact(DisplayName = "FillInvoiceFromViewModel: DocumentTypeCode is mapped")]
    public async Task FillInvoiceFromViewModel_DocumentTypeCode_IsMapped()
    {
        var vm = _wpf.Invoke(() => { var v = CreateVm(); v.DocumentTypeCode = "380"; return v; });

        var result = await CreateService().BuildAsync(vm);

        Assert.Equal("380", result.DocumentTypeCode);
    }

    [Fact(DisplayName = "FillInvoiceFromViewModel: DocumentName is mapped")]
    public async Task FillInvoiceFromViewModel_DocumentName_IsMapped()
    {
        var vm = _wpf.Invoke(() => { var v = CreateVm(); v.DocumentName = "Rechnung"; return v; });

        var result = await CreateService().BuildAsync(vm);

        Assert.Equal("Rechnung", result.DocumentName);
    }

    #endregion

    #region 3. FillInvoiceFromViewModel – Buyer

    [Fact(DisplayName = "FillInvoiceFromViewModel: Buyer.Name is mapped from CompanyBuyerParty")]
    public async Task FillInvoiceFromViewModel_BuyerName_IsMapped()
    {
        var vm = _wpf.Invoke(() => { var v = CreateVm(); v.CompanyBuyerParty = "ACME GmbH"; return v; });

        var result = await CreateService().BuildAsync(vm);

        Assert.Equal("ACME GmbH", result.Buyer!.Name);
    }

    [Fact(DisplayName = "FillInvoiceFromViewModel: Buyer.LegalOrganizationId equals FiscalId")]
    public async Task FillInvoiceFromViewModel_BuyerLegalOrganizationId_EqualsFiscalId()
    {
        var vm = _wpf.Invoke(() => { var v = CreateVm(); v.FiscalIdBuyerParty = "12345"; return v; });

        var result = await CreateService().BuildAsync(vm);

        Assert.Equal(result.Buyer!.FiscalId, result.Buyer.LegalOrganizationId);
    }

    [Fact(DisplayName = "FillInvoiceFromViewModel: Buyer.Street combines street and house number with space")]
    public async Task FillInvoiceFromViewModel_BuyerStreet_CombinesStreetAndHouseNumber()
    {
        var vm = _wpf.Invoke(() =>
        {
            var v = CreateVm();
            v.StreetBuyerParty = "Musterstraße";
            v.HouseNumberBuyerParty = "42";
            return v;
        });

        var result = await CreateService().BuildAsync(vm);

        Assert.Equal("Musterstraße 42", result.Buyer!.Street);
    }

    [Fact(DisplayName = "FillInvoiceFromViewModel: Buyer.Street without house number contains only street")]
    public async Task FillInvoiceFromViewModel_BuyerStreet_WithoutHouseNumber_ContainsOnlyStreet()
    {
        var vm = _wpf.Invoke(() =>
        {
            var v = CreateVm();
            v.StreetBuyerParty = "Musterstraße";
            v.HouseNumberBuyerParty = string.Empty;
            return v;
        });

        var result = await CreateService().BuildAsync(vm);

        Assert.Equal("Musterstraße", result.Buyer!.Street);
    }

    [Fact(DisplayName = "FillInvoiceFromViewModel: Buyer.VatId is mapped")]
    public async Task FillInvoiceFromViewModel_BuyerVatId_IsMapped()
    {
        var vm = _wpf.Invoke(() => { var v = CreateVm(); v.VatIdBuyerParty = "DE999999999"; return v; });

        var result = await CreateService().BuildAsync(vm);

        Assert.Equal("DE999999999", result.Buyer!.VatId);
    }

    [Fact(DisplayName = "FillInvoiceFromViewModel: Buyer.ID is mapped from ErpCustomerNumber")]
    public async Task FillInvoiceFromViewModel_BuyerID_IsMappedFromErpCustomerNumber()
    {
        var vm = _wpf.Invoke(() => { var v = CreateVm(); v.ErpCustomerNumberBuyerParty = "CUS-001"; return v; });

        var result = await CreateService().BuildAsync(vm);

        Assert.Equal("CUS-001", result.Buyer!.ID);
    }
    #endregion

    #region 4. FillInvoiceFromViewModel – Payment

    [Fact(DisplayName = "FillInvoiceFromViewModel: HasDiscount true uses DiscountPreviewText as PaymentTermsText")]
    public async Task FillInvoiceFromViewModel_HasDiscountTrue_UsesDiscountPreviewText()
    {
        var vm = _wpf.Invoke(() =>
        {
            var v = CreateVm();
            v.HasDiscount = true;
            v.DiscountPreviewText = "2% Skonto bis 15.01.2025"; // direkt setzen – Placeholder ist im Test leer
            v.NoDiscountPreviewText = "Zahlbar bis 31.01.2025";
            return v;
        });

        var result = await CreateService().BuildAsync(vm);

        Assert.Equal("2% Skonto bis 15.01.2025", result.Payment!.PaymentTermsText);
    }

    [Fact(DisplayName = "FillInvoiceFromViewModel: HasDiscount false uses NoDiscountPreviewText as PaymentTermsText")]
    public async Task FillInvoiceFromViewModel_HasDiscountFalse_UsesNoDiscountPreviewText()
    {
        var vm = _wpf.Invoke(() =>
        {
            var v = CreateVm();
            v.HasDiscount = false;
            v.NoDiscountPreviewText = "Zahlbar bis 31.01.2025";
            return v;
        });

        var result = await CreateService().BuildAsync(vm);

        Assert.Equal("Zahlbar bis 31.01.2025", result.Payment!.PaymentTermsText);
    }

    #endregion

    #region 5. FillInvoiceFromViewModel – Discount Terms

    [Fact(DisplayName = "FillInvoiceFromViewModel: complete discount data adds exactly 2 PaymentTerms")]
    public async Task FillInvoiceFromViewModel_CompleteDiscountData_AddsTwoPaymentTerms()
    {
        var vm = _wpf.Invoke(() =>
        {
            var v = CreateVm();
            v.HasDiscount = true;
            v.DiscountBasisDateText = "01.01.2025"; // → setzt DiscountBasisDate = DateOnly(2025,1,1)
            v.DiscountDays = "14";
            v.DiscountPercent = 2m;
            v.DiscountPreviewText = "2% Skonto bis 15.01.2025"; // direkt – Placeholder leer im Test
            v.NoDiscountPreviewText = "Zahlbar bis 31.01.2025";
            return v;
        });

        var result = await CreateService().BuildAsync(vm);

        Assert.Equal(2, result.Payment!.Terms.Count);
    }

    [Fact(DisplayName = "FillInvoiceFromViewModel: incomplete discount data leaves Terms empty")]
    public async Task FillInvoiceFromViewModel_IncompleteDiscountData_LeavesTermsEmpty()
    {
        var vm = _wpf.Invoke(CreateVm); // HasDiscount = false, nichts gesetzt

        var result = await CreateService().BuildAsync(vm);

        Assert.Empty(result.Payment!.Terms);
    }

    [Fact(DisplayName = "FillInvoiceFromViewModel: first PaymentTerm contains discount with correct percent")]
    public async Task FillInvoiceFromViewModel_FirstPaymentTerm_ContainsDiscountWithCorrectPercent()
    {
        var vm = _wpf.Invoke(() =>
        {
            var v = CreateVm();
            v.HasDiscount = true;
            v.DiscountBasisDateText = "01.01.2025";
            v.DiscountDays = "14";
            v.DiscountPercent = 2m;
            v.DiscountPreviewText = "2% Skonto bis 15.01.2025";
            v.NoDiscountPreviewText = "Zahlbar bis 31.01.2025";
            return v;
        });

        var result = await CreateService().BuildAsync(vm);

        Assert.NotNull(result.Payment!.Terms[0].DiscountTerms);
        Assert.Equal(2m, result.Payment.Terms[0].DiscountTerms!.CalculationPercent);
    }

    [Fact(DisplayName = "FillInvoiceFromViewModel: second PaymentTerm has no discount and matches DueDate")]
    public async Task FillInvoiceFromViewModel_SecondPaymentTerm_HasNoDiscountAndMatchesDueDate()
    {
        var vm = _wpf.Invoke(() =>
        {
            var v = CreateVm();
            v.HasDiscount = true;
            v.PaymentDueDateText = "31.01.2025";
            v.DiscountBasisDateText = "01.01.2025";
            v.DiscountDays = "14";
            v.DiscountPercent = 2m;
            v.DiscountPreviewText = "2% Skonto bis 15.01.2025";
            v.NoDiscountPreviewText = "Zahlbar bis 31.01.2025";
            return v;
        });

        var result = await CreateService().BuildAsync(vm);

        Assert.Null(result.Payment!.Terms[1].DiscountTerms);
        Assert.Equal(new DateTime(2025, 1, 31), result.Payment.Terms[1].DueDate);
    }

    #endregion


    #region 6. FillLinesFromStoreAsync – TaxCategory (Bug Regression)

    [Fact(DisplayName = "FillLinesFromStoreAsync: TaxCategory uses VatCategoryCode from DTO (regression: was always 'S')")]
    public async Task FillLinesFromStoreAsync_TaxCategory_UsesVatCategoryCodeFromDto()
    {
        await _store.AddAsync(MakeDto(vatCategoryCode: "Z"));
        var vm = _wpf.Invoke(CreateVm);

        var result = await CreateService().BuildAsync(vm);

        Assert.Equal("Z", result.Lines[0].TaxCategory);
    }

    [Fact(DisplayName = "FillLinesFromStoreAsync: TaxCategory falls back to 'S' when VatCategoryCode is empty")]
    public async Task FillLinesFromStoreAsync_TaxCategory_FallsBackToS_WhenCodeIsEmpty()
    {
        await _store.AddAsync(MakeDto(vatCategoryCode: string.Empty));
        var vm = _wpf.Invoke(CreateVm);

        var result = await CreateService().BuildAsync(vm);

        Assert.Equal("S", result.Lines[0].TaxCategory);
    }

    [Theory(DisplayName = "FillLinesFromStoreAsync: TaxCategory correctly maps all known VAT category codes")]
    [InlineData("S")]
    [InlineData("Z")]
    [InlineData("E")]
    [InlineData("AE")]
    [InlineData("K")]
    [InlineData("G")]
    public async Task FillLinesFromStoreAsync_TaxCategory_MapsAllKnownCodes(string code)
    {
        await _store.AddAsync(MakeDto(vatCategoryCode: code));
        var vm = _wpf.Invoke(CreateVm);

        var result = await CreateService().BuildAsync(vm);

        Assert.Equal(code, result.Lines[0].TaxCategory);
    }

    #endregion

    #region 7. FillLinesFromStoreAsync – UnitCode

    [Fact(DisplayName = "FillLinesFromStoreAsync: UnitCode uses DTO unit when set")]
    public async Task FillLinesFromStoreAsync_UnitCode_UsesDto()
    {
        await _store.AddAsync(MakeDto(unit: "HUR"));
        var vm = _wpf.Invoke(CreateVm);

        var result = await CreateService().BuildAsync(vm);

        Assert.Equal("HUR", result.Lines[0].UnitCode);
    }

    [Fact(DisplayName = "FillLinesFromStoreAsync: UnitCode falls back to 'C62' when DTO unit is empty")]
    public async Task FillLinesFromStoreAsync_UnitCode_FallsBackToC62_WhenEmpty()
    {
        await _store.AddAsync(MakeDto(unit: string.Empty));
        var vm = _wpf.Invoke(CreateVm);

        var result = await CreateService().BuildAsync(vm);

        Assert.Equal("C62", result.Lines[0].UnitCode);
    }

    #endregion

    #region 8. FillLinesFromStoreAsync – EAN / GlobalId

    [Fact(DisplayName = "FillLinesFromStoreAsync: EAN present sets GlobalId and GlobalIdSchemeId to '0160'")]
    public async Task FillLinesFromStoreAsync_EanPresent_SetsGlobalIdAndSchemeId()
    {
        await _store.AddAsync(new InvoicePositionDetailsDTO
        {
            InvoicePositionDescription = "Test",
            InvoicePositionQuantity = 1,
            InvoicePositionUnitPrice = 10,
            InvoicePositionEan = "4006381333931"
        });
        var vm = _wpf.Invoke(CreateVm);

        var result = await CreateService().BuildAsync(vm);

        Assert.Equal("4006381333931", result.Lines[0].GlobalId);
        Assert.Equal("0160", result.Lines[0].GlobalIdSchemeId);
    }

    [Fact(DisplayName = "FillLinesFromStoreAsync: EAN empty sets GlobalIdSchemeId to empty string")]
    public async Task FillLinesFromStoreAsync_EanEmpty_SetsSchemeIdToEmpty()
    {
        await _store.AddAsync(MakeDto());
        var vm = _wpf.Invoke(CreateVm);

        var result = await CreateService().BuildAsync(vm);

        Assert.Equal(string.Empty, result.Lines[0].GlobalIdSchemeId);
    }

    #endregion

    #region 9. FillLinesFromStoreAsync – Dates

    [Fact(DisplayName = "FillLinesFromStoreAsync: OrderDate is converted from DateOnly to DateTime")]
    public async Task FillLinesFromStoreAsync_OrderDate_IsConvertedToDateTime()
    {
        await _store.AddAsync(new InvoicePositionDetailsDTO
        {
            InvoicePositionDescription = "Test",
            InvoicePositionQuantity = 1,
            InvoicePositionUnitPrice = 10,
            InvoicePositionOrderDate = new DateOnly(2025, 6, 15)
        });
        var vm = _wpf.Invoke(CreateVm);

        var result = await CreateService().BuildAsync(vm);

        Assert.Equal(new DateTime(2025, 6, 15), result.Lines[0].BuyerOrderDate);
    }

    [Fact(DisplayName = "FillLinesFromStoreAsync: OrderDate null sets BuyerOrderDate to null")]
    public async Task FillLinesFromStoreAsync_OrderDateNull_SetsBuyerOrderDateToNull()
    {
        await _store.AddAsync(MakeDto());
        var vm = _wpf.Invoke(CreateVm);

        var result = await CreateService().BuildAsync(vm);

        Assert.Null(result.Lines[0].BuyerOrderDate);
    }

    [Fact(DisplayName = "FillLinesFromStoreAsync: DeliveryNoteDate is converted from DateOnly to DateTime")]
    public async Task FillLinesFromStoreAsync_DeliveryNoteDate_IsConvertedToDateTime()
    {
        await _store.AddAsync(new InvoicePositionDetailsDTO
        {
            InvoicePositionDescription = "Test",
            InvoicePositionQuantity = 1,
            InvoicePositionUnitPrice = 10,
            InvoicePositionDeliveryNoteDate = new DateOnly(2025, 3, 1)
        });
        var vm = _wpf.Invoke(CreateVm);

        var result = await CreateService().BuildAsync(vm);

        Assert.Equal(new DateTime(2025, 3, 1), result.Lines[0].DeliveryNoteDate);
    }

    [Fact(DisplayName = "FillLinesFromStoreAsync: DeliveryNoteDate null sets DeliveryNoteDate to null")]
    public async Task FillLinesFromStoreAsync_DeliveryNoteDateNull_SetsDeliveryNoteDateToNull()
    {
        await _store.AddAsync(MakeDto());
        var vm = _wpf.Invoke(CreateVm);

        var result = await CreateService().BuildAsync(vm);

        Assert.Null(result.Lines[0].DeliveryNoteDate);
    }

    #endregion

    #region 10. FillLinesFromStoreAsync – ForcedLineTotalAmount

    [Fact(DisplayName = "FillLinesFromStoreAsync: uses NetAmountAfterDiscount when present")]
    public async Task FillLinesFromStoreAsync_ForcedLineTotalAmount_UsesNetAmountAfterDiscount()
    {
        await _store.AddAsync(MakeDto(netAmountAfterDiscount: 85m, netAmount: 100m));
        var vm = _wpf.Invoke(CreateVm);

        var result = await CreateService().BuildAsync(vm);

        Assert.Equal(85m, result.Lines[0].ForcedLineTotalAmount);
    }

    [Fact(DisplayName = "FillLinesFromStoreAsync: falls back to NetAmount when NetAmountAfterDiscount is null")]
    public async Task FillLinesFromStoreAsync_ForcedLineTotalAmount_FallsBackToNetAmount()
    {
        await _store.AddAsync(MakeDto(netAmountAfterDiscount: null, netAmount: 100m));
        var vm = _wpf.Invoke(CreateVm);

        var result = await CreateService().BuildAsync(vm);

        Assert.Equal(100m, result.Lines[0].ForcedLineTotalAmount);
    }

    #endregion

    #region 11. FillLinesFromStoreAsync – Store Behavior

    [Fact(DisplayName = "FillLinesFromStoreAsync: empty store results in empty Lines")]
    public async Task FillLinesFromStoreAsync_EmptyStore_LeavesLinesEmpty()
    {
        // Store is empty by default – no AddAsync needed
        var vm = _wpf.Invoke(CreateVm);

        var result = await CreateService().BuildAsync(vm);

        Assert.Empty(result.Lines);
    }

    [Fact(DisplayName = "FillLinesFromStoreAsync: Lines are cleared before re-filling")]
    public async Task FillLinesFromStoreAsync_Lines_AreClearedBeforeRefilling()
    {
        await _store.AddAsync(MakeDto(description: "Only Item"));
        var vm = _wpf.Invoke(CreateVm);
        var service = CreateService();

        await service.BuildAsync(vm);              // first call
        var result = await service.BuildAsync(vm); // second call

        Assert.Single(result.Lines); // not duplicated
    }

    [Fact(DisplayName = "FillLinesFromStoreAsync: multiple DTOs produce multiple Lines")]
    public async Task FillLinesFromStoreAsync_MultipleDtos_ProduceMultipleLines()
    {
        await _store.AddAsync(MakeDto(description: "Item 1"));
        await _store.AddAsync(MakeDto(description: "Item 2"));
        await _store.AddAsync(MakeDto(description: "Item 3"));
        var vm = _wpf.Invoke(CreateVm);

        var result = await CreateService().BuildAsync(vm);

        Assert.Equal(3, result.Lines.Count);
    }

    #endregion

    #region 12. ApplySellerFromAppOptions

    [Fact(DisplayName = "ApplySellerFromAppOptions: all seller fields are mapped from AppOptions")]
    public async Task ApplySellerFromAppOptions_AllFields_AreMapped()
    {
        _appOptions.Invoice = new InvoiceOptions
        {
            Seller = new SellerOptions
            {
                Name = "INTUS GmbH",
                Street = "Musterstraße 1",
                Zip = "12345",
                City = "Berlin",
                CountryCode = "DE",
                VatId = "DE123456789"
            }
        };
        var vm = _wpf.Invoke(CreateVm);

        var result = await CreateService().BuildAsync(vm);

        Assert.Equal("INTUS GmbH", result.Seller!.Name);
        Assert.Equal("Musterstraße 1", result.Seller.Street);
        Assert.Equal("12345", result.Seller.Zip);
        Assert.Equal("Berlin", result.Seller.City);
        Assert.Equal("DE", result.Seller.CountryCode);
        Assert.Equal("DE123456789", result.Seller.VatId);
    }

    [Fact(DisplayName = "ApplySellerFromAppOptions: null seller in AppOptions is skipped without throwing")]
    public async Task ApplySellerFromAppOptions_NullSeller_IsSkipped()
    {
        _appOptions.Invoice = new InvoiceOptions { Seller = null! };
        var vm = _wpf.Invoke(CreateVm);

        var result = await CreateService().BuildAsync(vm);

        Assert.True(string.IsNullOrEmpty(result.Seller?.Name));
        Assert.True(string.IsNullOrEmpty(result.Seller?.VatId));
        Assert.True(string.IsNullOrEmpty(result.Seller?.City));
    }

    [Fact(DisplayName = "ApplySellerFromAppOptions: ContactEmail is preferred over GeneralEmail")]
    public async Task ApplySellerFromAppOptions_ContactEmail_PreferredOverGeneralEmail()
    {
        _appOptions.Invoice = new InvoiceOptions
        {
            Seller = new SellerOptions
            {
                ContactEmail = "contact@intus.de",
                GeneralEmail = "info@intus.de"
            }
        };
        var vm = _wpf.Invoke(CreateVm);

        var result = await CreateService().BuildAsync(vm);

        Assert.Equal("contact@intus.de", result.Seller!.ContactEmail);
    }

    [Fact(DisplayName = "ApplySellerFromAppOptions: GeneralEmail is used as fallback when ContactEmail is empty")]
    public async Task ApplySellerFromAppOptions_GeneralEmail_UsedWhenContactEmailEmpty()
    {
        _appOptions.Invoice = new InvoiceOptions
        {
            Seller = new SellerOptions
            {
                ContactEmail = string.Empty,
                GeneralEmail = "info@intus.de"
            }
        };
        var vm = _wpf.Invoke(CreateVm);

        var result = await CreateService().BuildAsync(vm);

        Assert.Equal("info@intus.de", result.Seller!.ContactEmail);
    }

    #endregion

    #region 13. ApplyPaymentAccountFromAppOptions

    [Fact(DisplayName = "ApplyPaymentAccountFromAppOptions: Iban, Bic and AccountName are mapped")]
    public async Task ApplyPaymentAccountFromAppOptions_AllFields_AreMapped()
    {
        _appOptions.Invoice = new InvoiceOptions
        {
            Payment = new PaymentOptions
            {
                Iban = "DE89370400440532013000",
                Bic = "COBADEFFXXX",
                AccountName = "INTUS GmbH"
            }
        };
        var vm = _wpf.Invoke(CreateVm);

        var result = await CreateService().BuildAsync(vm);

        Assert.Equal("DE89370400440532013000", result.Payment!.Iban);
        Assert.Equal("COBADEFFXXX", result.Payment.Bic);
        Assert.Equal("INTUS GmbH", result.Payment.AccountName);
    }

    [Fact(DisplayName = "ApplyPaymentAccountFromAppOptions: null payment in AppOptions is skipped without throwing")]
    public async Task ApplyPaymentAccountFromAppOptions_NullPayment_IsSkipped()
    {
        _appOptions.Invoice = new InvoiceOptions { Payment = null! };
        var vm = _wpf.Invoke(CreateVm);

        var result = await CreateService().BuildAsync(vm);

        Assert.NotNull(result);
    }

    #endregion

    #region 14. ApplyNotesFromAppOptions

    [Fact(DisplayName = "ApplyNotesFromAppOptions: notes are added from AppOptions")]
    public async Task ApplyNotesFromAppOptions_Notes_AreAdded()
    {
        _appOptions.Invoice = new InvoiceOptions
        {
            Notes = new List<InvoiceNoteOptions>
            {
                new() { SubjectCode = "AAI", Content = "Thank you for your business" },
                new() { SubjectCode = "REG", Content = "VAT regulation note" }
            }
        };
        var vm = _wpf.Invoke(CreateVm);

        var result = await CreateService().BuildAsync(vm);

        Assert.Equal(2, result.Notes.Count);
        Assert.Equal("AAI", result.Notes[0].SubjectCode);
        Assert.Equal("Thank you for your business", result.Notes[0].Content);
    }

    [Fact(DisplayName = "ApplyNotesFromAppOptions: empty SubjectCode falls back to 'REG'")]
    public async Task ApplyNotesFromAppOptions_EmptySubjectCode_FallsBackToReg()
    {
        _appOptions.Invoice = new InvoiceOptions
        {
            Notes = new List<InvoiceNoteOptions>
            {
                new() { SubjectCode = string.Empty, Content = "Some note" }
            }
        };
        var vm = _wpf.Invoke(CreateVm);

        var result = await CreateService().BuildAsync(vm);

        Assert.Equal("REG", result.Notes[0].SubjectCode);
    }

    [Fact(DisplayName = "ApplyNotesFromAppOptions: null notes in AppOptions is skipped without throwing")]
    public async Task ApplyNotesFromAppOptions_NullNotes_IsSkipped()
    {
        _appOptions.Invoice = new InvoiceOptions { Notes = null! };
        var vm = _wpf.Invoke(CreateVm);

        var result = await CreateService().BuildAsync(vm);

        Assert.Empty(result.Notes);
    }

    #endregion

    #region 15. RecalculateHeaderAmounts

    [Fact(DisplayName = "RecalculateHeaderAmounts: DuePayableAmount = Net + Tax (Quantity × UnitPrice × VatRate)")]
    public async Task RecalculateHeaderAmounts_DuePayableAmount_IsCorrect()
    {
        // 1 × 100 = 100 net, 100 × 19% = 19 tax → total = 119
        await _store.AddAsync(MakeDto(quantity: 1m, unitPrice: 100m, vatRate: 19, netAmount: 100m));
        var vm = _wpf.Invoke(CreateVm);

        var result = await CreateService().BuildAsync(vm);

        Assert.Equal(119m, result.HeaderDuePayableAmount);
    }

    [Fact(DisplayName = "RecalculateHeaderAmounts: ForcedLineTotalAmount is used instead of Quantity × UnitPrice")]
    public async Task RecalculateHeaderAmounts_UsesForcedLineTotalAmount_InsteadOfCalculation()
    {
        // ForcedLineTotalAmount = 80, 80 × 19% = 15.20 tax → total = 95.20
        await _store.AddAsync(MakeDto(quantity: 1m, unitPrice: 100m, vatRate: 19, netAmountAfterDiscount: 80m));
        var vm = _wpf.Invoke(CreateVm);

        var result = await CreateService().BuildAsync(vm);

        Assert.Equal(95.20m, result.HeaderDuePayableAmount);
    }

    [Fact(DisplayName = "RecalculateHeaderAmounts: multiple lines are summed correctly")]
    public async Task RecalculateHeaderAmounts_MultipleLines_SummedCorrectly()
    {
        // Line 1: 100 net + 19 tax = 119
        // Line 2: 200 net + 38 tax = 238 → total = 357
        await _store.AddAsync(MakeDto(quantity: 1m, unitPrice: 100m, vatRate: 19, netAmount: 100m));
        await _store.AddAsync(MakeDto(quantity: 1m, unitPrice: 200m, vatRate: 19, netAmount: 200m));
        var vm = _wpf.Invoke(CreateVm);

        var result = await CreateService().BuildAsync(vm);

        Assert.Equal(357m, result.HeaderDuePayableAmount);
    }

    [Fact(DisplayName = "RecalculateHeaderAmounts: result is rounded to 2 decimal places")]
    public async Task RecalculateHeaderAmounts_Result_IsRoundedTo2DecimalPlaces()
    {
        await _store.AddAsync(MakeDto(quantity: 1m, unitPrice: 99.99m, vatRate: 19, netAmount: 99.99m));
        var vm = _wpf.Invoke(CreateVm);

        var result = await CreateService().BuildAsync(vm);

        Assert.Equal(Math.Round(result.HeaderDuePayableAmount, 2), result.HeaderDuePayableAmount);
    }

    [Fact(DisplayName = "RecalculateHeaderAmounts: zero tax rate produces no tax")]
    public async Task RecalculateHeaderAmounts_ZeroTaxRate_ProducesNoTax()
    {
        // 1 × 100 net, 0% tax → total = 100
        await _store.AddAsync(MakeDto(quantity: 1m, unitPrice: 100m, vatRate: 0, netAmount: 100m));
        var vm = _wpf.Invoke(CreateVm);

        var result = await CreateService().BuildAsync(vm);

        Assert.Equal(100m, result.HeaderDuePayableAmount);
    }

    #endregion

    public void Dispose() => _wpf.Dispose();
}
