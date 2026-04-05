namespace tulo.eInvoiceApp.Services;
/// <summary>
/// Provides human-readable display text and tooltip lookups
/// for invoice position unit codes and VAT category codes.
/// </summary>
public interface IInvoicePositionLookupService
{
    /// <summary>
    /// Returns the localized display text for the given unit code
    /// (e.g. "H87" → "Piece", "HUR" → "Hour", "KGM" → "Kilogram").
    /// </summary>
    /// <param name="unitCode">
    /// The unit code to look up (e.g. "H87", "HUR", "KGM").
    /// Returns an empty string or fallback text if <see langword="null"/>,
    /// empty, or unknown.
    /// </param>
    /// <returns>
    /// The localized display text for the unit, or an empty string
    /// if the code is not recognized.
    /// </returns>
    string GetUnitText(string? unitCode);

    /// <summary>
    /// Returns the localized display text for the given VAT category code
    /// (e.g. "S" → "Standard rate", "Z" → "Zero rated", "E" → "Exempt").
    /// </summary>
    /// <param name="categoryCode">
    /// The VAT category code to look up (e.g. "S", "Z", "E", "AE", "K", "G").
    /// Returns an empty string or fallback text if <see langword="null"/>,
    /// empty, or unknown.
    /// </param>
    /// <returns>
    /// The localized display text for the VAT category, or an empty string
    /// if the code is not recognized.
    /// </returns>
    string GetVatCategoryText(string? categoryCode);

    /// <summary>
    /// Returns the localized tooltip text for the given VAT category code,
    /// providing a more detailed description suitable for UI tooltips
    /// (e.g. "S" → "Taxable supply subject to VAT at standard rate").
    /// </summary>
    /// <param name="categoryCode">
    /// The VAT category code to look up (e.g. "S", "Z", "E", "AE", "K", "G").
    /// Returns an empty string or fallback text if <see langword="null"/>,
    /// empty, or unknown.
    /// </param>
    /// <returns>
    /// The localized tooltip description for the VAT category, or an empty string
    /// if the code is not recognized.
    /// </returns>
    string GetVatCategoryTooltip(string? categoryCode);
}

