namespace tulo.XMLeInvoiceToPdf.Languages;
/// <summary>
/// Provides translation lookups for the application,
/// resolving a given key to its corresponding localized string
/// based on the currently active culture.
/// </summary>
public interface ITranslatorProvider
{
    /// <summary>
    /// Resolves the localized string for the specified translation key.
    /// </summary>
    /// <param name="key">
    /// The translation key to look up (e.g. <c>"LabelSave"</c>, <c>"ToolTipReturn"</c>).
    /// Must not be <see langword="null"/> or empty.
    /// </param>
    /// <param name="fallback">
    /// The fallback string to return when the key is not found
    /// in the current translation dictionary.
    /// Defaults to <c>"not found:"</c> to make missing translations
    /// clearly visible during development and testing.
    /// </param>
    /// <returns>
    /// The localized string for the given key, or the <paramref name="fallback"/>
    /// value if the key is not found.
    /// </returns>
    string Translate(string key, string fallback = "not found:");
}
