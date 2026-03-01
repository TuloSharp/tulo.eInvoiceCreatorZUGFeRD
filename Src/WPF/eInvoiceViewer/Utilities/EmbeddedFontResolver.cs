using PdfSharp.Fonts;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace tulo.eInvoice.eInvoiceViewer.Utilities;
public class EmbeddedFontResolver : IFontResolver
{
    private static class FaceKeys
    {
        public const string Verdana = "Verdana";
        public const string Arial = "Arial";
        public const string CascadiaCode = "CascadiaCode";
        public const string Roboto = "Roboto";
    }

    private static readonly Dictionary<string, string> _familyMap =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { "verdana", FaceKeys.Verdana },
            { "arial", FaceKeys.Arial },
            { "cascadiacode", FaceKeys.CascadiaCode },
            { "roboto", FaceKeys.Roboto   }
        };

    private static readonly Dictionary<string, string> _resourceMap;

    static EmbeddedFontResolver()
    {
        _resourceMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var asm = Assembly.GetExecutingAssembly();
        var resources = asm.GetManifestResourceNames();

        foreach (var kvp in new Dictionary<string, string>
                 {
                     { FaceKeys.Verdana, "VERDANA.TTF" },
                     { FaceKeys.Arial, "ARIAL.TTF" },
                     { FaceKeys.CascadiaCode, "CASCADIACODE.TTF" },
                     { FaceKeys.Roboto, "ROBOTO.TTF"   }
                 })
        {
            var match = resources.FirstOrDefault(r => r.EndsWith("Fonts." + kvp.Value, StringComparison.OrdinalIgnoreCase));
            if (match == null)
                Debug.WriteLine($"Font Resource '{kvp.Value}' not found. Available resources:\n{string.Join("\n", resources)}");

            _resourceMap[kvp.Key] = match!;
        }
    }

    public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        if (string.IsNullOrEmpty(familyName))
            return null;

        if (_familyMap.TryGetValue(familyName.Trim(), out var faceKey))
        {
            return new FontResolverInfo(faceKey);
        }

        return PlatformFontResolver.ResolveTypeface(familyName, isBold, isItalic);
    }

    public byte[]? GetFont(string faceName)
    {
        if (string.IsNullOrEmpty(faceName))
            return null;

        if (!_resourceMap.TryGetValue(faceName, out var resourceName))
            return null;

        var asm = Assembly.GetExecutingAssembly();
        using var stream = asm.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            // Debug: check Ressource names
            var names = asm.GetManifestResourceNames();
            Debug.WriteLine($"Font Resource '{resourceName}' nicht gefunden. Verfügbare Resources:\n{string.Join("\n", names)}");
        }

        using var ms = new MemoryStream();
        stream!.CopyTo(ms);
        return ms.ToArray();
    }
}

