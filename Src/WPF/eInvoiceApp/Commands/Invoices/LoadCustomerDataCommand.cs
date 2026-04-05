using Microsoft.Win32;
using System.IO;
using System.Text.Json;
using tulo.CommonMVVM.Collector;
using tulo.CommonMVVM.Commands;
using tulo.CoreLib.Translators;
using tulo.eInvoiceApp.ViewModels.Invoices;
using tulo.eInvoiceXmlGeneratorCii.Models;

namespace tulo.eInvoiceApp.Commands.Invoices;

public class LoadCustomerDataCommand(InvoiceViewModel invoiceViewModel, ICollectorCollection collectorCollection) : BaseCommand
{
    #region Services / Stores filled via CollectorCollection
    private readonly InvoiceViewModel _invoiceViewModel = invoiceViewModel;
    private readonly ITranslatorUiProvider _translatorUiProvider = collectorCollection.GetService<ITranslatorUiProvider>();
    #endregion

    public override void Execute(object parameter)
    {
        var title = _translatorUiProvider.Translate("TitelLoadCustomerData");
        var filter = _translatorUiProvider.Translate("FilterJsonFile") + " (*.json)|*.json";

        var openFileDialog = new OpenFileDialog
        {
            Title = title,
            Filter = filter,
            DefaultExt = ".json",
            CheckFileExists = true,
            Multiselect = false
        };

        bool? result = openFileDialog.ShowDialog();

        if (result != true)
        {
            return;
        }

        string json = File.ReadAllText(openFileDialog.FileName);

        var buyer = JsonSerializer.Deserialize<Party>(json);

        if (buyer == null)
        {
            return;
        }

        _invoiceViewModel.ErpCustomerNumberBuyerParty = buyer.ID ?? string.Empty;
        _invoiceViewModel.CompanyBuyerParty = buyer.Name ?? string.Empty;

        SplitStreetAndHouseNumber(
            buyer.Street,
            out string street,
            out string houseNumber);

        _invoiceViewModel.StreetBuyerParty = street;
        _invoiceViewModel.HouseNumberBuyerParty = houseNumber;

        _invoiceViewModel.PostalCodeBuyerParty = buyer.Zip ?? string.Empty;
        _invoiceViewModel.CityBuyerParty = buyer.City ?? string.Empty;
        _invoiceViewModel.CountryCodeBuyerParty = buyer.CountryCode ?? string.Empty;
        _invoiceViewModel.VatIdBuyerParty = buyer.VatId ?? string.Empty;
        _invoiceViewModel.LeitwegIdBuyerParty = buyer.LeitwegId ?? string.Empty;
        _invoiceViewModel.FiscalIdBuyerParty = buyer.FiscalId ?? string.Empty;
        _invoiceViewModel.EmailAddressBuyerParty = buyer.GeneralEmail ?? string.Empty;
        _invoiceViewModel.PersonBuyerParty = buyer.ContactPersonName ?? string.Empty;
        _invoiceViewModel.PhoneBuyerParty = buyer.ContactPhone ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(buyer.ContactEmail))
        {
            _invoiceViewModel.EmailAddressBuyerParty = buyer.ContactEmail;
        }
    }

    private static void SplitStreetAndHouseNumber(string? fullStreet, out string street, out string houseNumber)
    {
        street = string.Empty;
        houseNumber = string.Empty;

        if (string.IsNullOrWhiteSpace(fullStreet))
        {
            return;
        }

        fullStreet = fullStreet.Trim();

        int lastSpaceIndex = fullStreet.LastIndexOf(' ');
        if (lastSpaceIndex <= 0 || lastSpaceIndex == fullStreet.Length - 1)
        {
            street = fullStreet;
            return;
        }

        string possibleHouseNumber = fullStreet[(lastSpaceIndex + 1)..];
        string possibleStreet = fullStreet[..lastSpaceIndex];

        bool containsDigit = false;
        foreach (char c in possibleHouseNumber)
        {
            if (char.IsDigit(c))
            {
                containsDigit = true;
                break;
            }
        }

        if (containsDigit)
        {
            street = possibleStreet;
            houseNumber = possibleHouseNumber;
        }
        else
        {
            street = fullStreet;
        }
    }
}