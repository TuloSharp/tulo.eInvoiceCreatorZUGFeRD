
using System.IO;
using System.Text.Json;
using tulo.CommonMVVM.Commands;
using tulo.eInvoice.eInvoiceApp.ViewModels.Invoices;
using tulo.eInvoiceXmlGeneratorCii.Models;

namespace tulo.eInvoice.eInvoiceApp.Commands.Invoices;

public class SaveCustomerDataCommand(InvoiceViewModel invoiceViewModel) : BaseCommand
{
    private readonly InvoiceViewModel _invoiceViewModel = invoiceViewModel;
    public override void Execute(object parameter)
    {
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
            Title = "Buyer als JSON speichern",
            Filter = "JSON-Datei (*.json)|*.json",
            DefaultExt = ".json",
            AddExtension = true,
            FileName = "buyer"
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
