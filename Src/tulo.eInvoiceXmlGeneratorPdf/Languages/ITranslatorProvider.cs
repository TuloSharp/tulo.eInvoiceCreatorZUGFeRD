namespace tulo.XMLeInvoiceToPdf.Languages;
public interface ITranslatorProvider
{
    string Translate(string key, string fallback = "not found:");
}
