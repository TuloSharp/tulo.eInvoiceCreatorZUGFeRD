using System.Globalization;

namespace tulo.eInvoiceCreatorZUGFeRD.Services;
/// <summary>
/// Provides culture management for the application, including resolving,
/// applying, and querying the current and supported cultures.
/// </summary>
public interface ICultureService
{
    /// <summary>
    /// Gets the culture that is currently active in the application.
    /// </summary>
    CultureInfo CurrentCulture { get; }

    /// <summary>
    /// Gets the read-only list of cultures supported by the application.
    /// </summary>
    IReadOnlyList<CultureInfo> SupportedCultures { get; }

    /// <summary>
    /// Resolves the culture to be used on application startup.
    /// The resolution order is typically: user preference → system culture → fallback default.
    /// </summary>
    /// <returns>The <see cref="CultureInfo"/> to apply on startup.</returns>
    CultureInfo ResolveStartupCulture();

    /// <summary>
    /// Applies the specified culture to the application by culture name (e.g. "de-DE", "en-US").
    /// Updates <see cref="CurrentCulture"/> and sets the thread culture accordingly.
    /// </summary>
    /// <param name="cultureName">
    /// The IETF language tag of the culture to apply (e.g. "de-DE", "en-US").
    /// </param>
    /// <exception cref="CultureNotFoundException">
    /// Thrown when <paramref name="cultureName"/> does not match a known culture.
    /// </exception>
    void ApplyCulture(string cultureName);

    /// <summary>
    /// Applies the specified culture to the application.
    /// Updates <see cref="CurrentCulture"/> and sets the thread culture accordingly.
    /// </summary>
    /// <param name="culture">The <see cref="CultureInfo"/> to apply.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="culture"/> is <see langword="null"/>.
    /// </exception>
    void ApplyCulture(CultureInfo culture);
}

