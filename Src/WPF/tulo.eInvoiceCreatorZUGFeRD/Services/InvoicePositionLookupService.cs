using tulo.CommonMVVM.Collector;
using tulo.CoreLib.Translators;

namespace tulo.eInvoiceCreatorZUGFeRD.Services;
public sealed class InvoicePositionLookupService(ICollectorCollection collectorCollection) : IInvoicePositionLookupService
{
    private readonly ITranslatorUiProvider _translator = collectorCollection.GetService<ITranslatorUiProvider>();

    public string GetUnitText(string? unitCode) => unitCode switch
    {
        "H87" => _translator.Translate("UnitPiece"),
        "C62" => _translator.Translate("UnitPiece"),
        "HUR" => _translator.Translate("UnitHour"),
        "KGM" => _translator.Translate("UnitKilogram"),
        "LTR" => _translator.Translate("UnitLitre"),
        "MTR" => _translator.Translate("UnitMetre"),
        "LM" => _translator.Translate("UnitLinearMetre"),
        "M2" => _translator.Translate("UnitSquareMetre"),
        "M3" => _translator.Translate("UnitCubicMetre"),
        "DAY" => _translator.Translate("UnitDay"),
        "MIN" => _translator.Translate("UnitMinute"),
        "SEC" => _translator.Translate("UnitSecond"),
        "HAR" => _translator.Translate("UnitHectare"),
        "KMT" => _translator.Translate("UnitKilometre"),
        "LS" => _translator.Translate("UnitFlatRate"),
        "NAR" => _translator.Translate("UnitCount"),
        "NPR" => _translator.Translate("UnitPair"),
        "SET" => _translator.Translate("UnitSet"),
        "TNE" => _translator.Translate("UnitTonne"),
        "WEE" => _translator.Translate("UnitWeek"),
        "P1" => _translator.Translate("UnitPercent"),
        _ => unitCode ?? string.Empty
    };

    public string GetVatCategoryText(string? categoryCode) => categoryCode switch
    {
        "S" => _translator.Translate("VatCat_S_Text"),   // z.B. "Standard rate"
        "Z" => _translator.Translate("VatCat_Z_Text"),
        "E" => _translator.Translate("VatCat_E_Text"),
        "AE" => _translator.Translate("VatCat_AE_Text"),
        "K" => _translator.Translate("VatCat_K_Text"),
        "G" => _translator.Translate("VatCat_G_Text"),
        _ => categoryCode ?? string.Empty
    };

    public string GetVatCategoryTooltip(string? categoryCode) => categoryCode switch
    {
        "S" => _translator.Translate("ToolTipVatCategory_S"),
        "Z" => _translator.Translate("ToolTipVatCategory_Z"),
        "E" => _translator.Translate("ToolTipVatCategory_E"),
        "AE" => _translator.Translate("ToolTipVatCategory_AE"),
        "K" => _translator.Translate("ToolTipVatCategory_K"),
        "G" => _translator.Translate("ToolTipVatCategory_G"),
        _ => string.Empty
    };
}
