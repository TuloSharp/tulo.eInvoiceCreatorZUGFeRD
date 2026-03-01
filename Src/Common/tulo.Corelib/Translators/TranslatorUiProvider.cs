using System.Diagnostics;
using System.Reflection;
using System.Xml;

namespace tulo.CoreLib.Translators;

public class TranslatorUiProvider : ITranslatorUiProvider
{
    private Dictionary<string, string> _translations = new();

    public TranslatorUiProvider(string translationPath)
    {
        if (!string.IsNullOrWhiteSpace(translationPath) && File.Exists(translationPath))
        {
            LoadFromFile(translationPath);
        }
        else
        {
            Trace.WriteLine($"Translation file not found: '{translationPath}'");
        }
    }

    // 2) Embedded-Constructor
    public TranslatorUiProvider(Assembly assembly, string resourceName)
    {
        LoadFromEmbeddedResource(assembly, resourceName);
    }

    private void LoadFromFile(string xmlPath)
    {
        try
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlPath);
            ParseXml(xmlDoc);
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Error loading translations from file: {ex.Message}");
        }
    }

    private void LoadFromEmbeddedResource(Assembly assembly, string resourceName)
    {
        try
        {
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                Trace.WriteLine($"Embedded resource not found: '{resourceName}'. Available: " +
                                string.Join(", ", assembly.GetManifestResourceNames()));
                return;
            }

            var xmlDoc = new XmlDocument();
            xmlDoc.Load(stream);
            ParseXml(xmlDoc);
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Error loading translations from embedded resource: {ex.Message}");
        }
    }

    private void ParseXml(XmlDocument xmlDoc)
    {
        foreach (XmlNode xmlNode in xmlDoc.SelectNodes("//properties/entry")!)
        {
            string? key = xmlNode.Attributes?["key"]?.InnerText;
            string value = xmlNode.InnerText;

            if (!string.IsNullOrEmpty(key))
            {
                _translations[key] = value;
            }
        }
    }

    public string Translate(string key, string fallback = "not found:")
    {
        return _translations.ContainsKey(key) ? _translations[key].Replace("|", "\n") : fallback + key;
    }
}
