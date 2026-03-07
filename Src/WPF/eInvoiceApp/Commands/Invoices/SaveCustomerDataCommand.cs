
using System.IO;
using System.Text.Json;
using tulo.CommonMVVM.Collector;
using tulo.CommonMVVM.Commands;
using tulo.CoreLib.Translators;
using tulo.eInvoice.eInvoiceApp.ViewModels.Invoices;
using tulo.eInvoiceXmlGeneratorCii.Models;

namespace tulo.eInvoice.eInvoiceApp.Commands.Invoices;

public class SaveCustomerDataCommand(InvoiceViewModel invoiceViewModel, ICollectorCollection collectorCollection) : BaseCommand
{
    #region Services / Stores filled via CollectorCollection
    private readonly InvoiceViewModel _invoiceViewModel = invoiceViewModel;
    private readonly ITranslatorUiProvider _translatorUiProvider = collectorCollection.GetService<ITranslatorUiProvider>();
    #endregion
    public override void Execute(object parameter)
    {
        var title = _translatorUiProvider.Translate("TitelSaveCustomerData");
        var fileName = _invoiceViewModel.VatIdBuyerParty + _translatorUiProvider.Translate("FileNameCustomerData");
        var filter = _translatorUiProvider.Translate("FilterJsonFile") + " (*.json)|*.json";
        
        var buyer = new Party
        {
            ID = _invoiceViewModel.ErpCustomerNumberBuyerParty ?? string.Empty,
            Name = _invoiceViewModel.CompanyBuyerParty ?? string.Empty,
            Street = BuildStreet(_invoiceViewModel.StreetBuyerParty, _invoiceViewModel.HouseNumberBuyerParty),
            Zip = _invoiceViewModel.PostalCodeBuyerParty ?? string.Empty,
            City = _invoiceViewModel.CityBuyerParty ?? string.Empty,
            CountryCode = _invoiceViewModel.CountryCodeBuyerParty ?? string.Empty,
            VatId = _invoiceViewModel.VatIdBuyerParty ?? string.Empty,
            LeitwegId = _invoiceViewModel.LeitwegIdBuyerParty ?? string.Empty,
            FiscalId = _invoiceViewModel.FiscalIdBuyerParty ?? string.Empty,
            GeneralEmail = _invoiceViewModel.EmailAddressBuyerParty ?? string.Empty,
            ContactPersonName = _invoiceViewModel.PersonBuyerParty ?? string.Empty,
            ContactPhone = _invoiceViewModel.PhoneBuyerParty ?? string.Empty,
            ContactEmail = _invoiceViewModel.EmailAddressBuyerParty ?? string.Empty
        };

       

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        string json = JsonSerializer.Serialize(buyer, options);

        var saveFileDialog = new Microsoft.Win32.SaveFileDialog
        {
            Title = title,
            Filter = filter,
            DefaultExt = ".json",
            AddExtension = true,
            FileName = fileName
        };

        bool? result = saveFileDialog.ShowDialog();

        if (result == true)
        {
            File.WriteAllText(saveFileDialog.FileName, json);
        }
    }

    private static string BuildStreet(string? street, string? houseNumber)
    {
        street ??= string.Empty;
        houseNumber ??= string.Empty;

        if (string.IsNullOrWhiteSpace(street))
            return houseNumber;

        if (string.IsNullOrWhiteSpace(houseNumber))
            return street;

        return $"{street} {houseNumber}";
    }
}
