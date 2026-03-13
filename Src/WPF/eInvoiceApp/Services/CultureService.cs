using Microsoft.Extensions.Options;
using System.Globalization;
using tulo.eInvoice.eInvoiceApp.Options;

namespace tulo.eInvoice.eInvoiceApp.Services;
public sealed class CultureService : ICultureService
{
    private static readonly CultureInfo _fallbackCulture = CultureInfo.GetCultureInfo("de-DE");
    private readonly List<CultureInfo> _supportedCultures;
    private readonly CultureInfo _defaultCulture;

    public CultureInfo CurrentCulture { get; private set; }

    public IReadOnlyList<CultureInfo> SupportedCultures => _supportedCultures;

    public CultureService(IOptions<AppOptions> appOptions)
    {
        var config = appOptions.Value;

        _supportedCultures = (config.Localization.SupportedCultures ?? Enumerable.Empty<string>())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(TryGetCulture)
            .Where(x => x is not null)
            .Cast<CultureInfo>()
            .ToList();

        _defaultCulture = TryGetCulture(config.Localization.DefaultCulture) ?? _fallbackCulture;

        if (_supportedCultures.Count == 0)
        {
            _supportedCultures = [_defaultCulture];
        }
        else if (!_supportedCultures.Any(c =>
                     string.Equals(c.Name, _defaultCulture.Name, StringComparison.OrdinalIgnoreCase)))
        {
            _supportedCultures.Insert(0, _defaultCulture);
        }

        CurrentCulture = _defaultCulture;
    }

    public CultureInfo ResolveStartupCulture()
    {
        var osCulture = CultureInfo.CurrentUICulture;

        var match = FindSupported(osCulture.Name)
                 ?? FindSupported(osCulture.TwoLetterISOLanguageName);

        return match ?? _defaultCulture;
    }

    public void ApplyCulture(string cultureName)
    {
        var culture = FindSupported(cultureName) ?? _defaultCulture;
        ApplyCulture(culture);
    }

    public void ApplyCulture(CultureInfo culture)
    {
        CurrentCulture = culture;

        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
    }

    private CultureInfo? FindSupported(string value)
    {
        return _supportedCultures.FirstOrDefault(c =>
            string.Equals(c.Name, value, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(c.TwoLetterISOLanguageName, value, StringComparison.OrdinalIgnoreCase));
    }

    private static CultureInfo? TryGetCulture(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        try
        {
            return CultureInfo.GetCultureInfo(name);
        }
        catch
        {
            return null;
        }
    }
}
