namespace tulo.eInvoice.eInvoiceApp.Services;
public interface IInvoicePositionLookupService
{
    string GetUnitText(string? unitCode);
    string GetVatCategoryText(string? categoryCode);
    string GetVatCategoryTooltip(string? categoryCode);
}
