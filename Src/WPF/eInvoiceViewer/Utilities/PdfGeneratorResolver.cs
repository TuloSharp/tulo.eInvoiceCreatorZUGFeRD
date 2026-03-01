using tulo.CoreLib.Components.ResultPattern;
using tulo.XMLeInvoiceToPdf.Services;

namespace tulo.eInvoice.eInvoiceViewer.Utilities;
public sealed class PdfGeneratorResolver : IPdfGeneratorResolver
{
    private readonly IReadOnlyDictionary<string, IPdfGeneratorFromInvoice> _byName;

    public PdfGeneratorResolver(IEnumerable<IPdfGeneratorFromInvoice> generators)
    {
        _byName = generators.Where(g => !string.IsNullOrWhiteSpace(g.Name)).GroupBy(g => g.Name.Trim(), StringComparer.OrdinalIgnoreCase).ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
    }

    public Result<IPdfGeneratorFromInvoice> ResolveByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<IPdfGeneratorFromInvoice>.Failure(new Error("Generator name is empty.", ErrorCodes.InvalidInput));

        var key = name.Trim();

        if (_byName.TryGetValue(key, out var gen))
            return Result<IPdfGeneratorFromInvoice>.Success(gen);

        return Result<IPdfGeneratorFromInvoice>.Failure(new Error($"No PDF generator registered for name '{key}'. Available: {string.Join(", ", _byName.Keys)}", ErrorCodes.NotFound));
    }
}

