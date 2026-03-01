namespace tulo.CoreLib.Translators;
public interface ITranslatorUiProvider
{
    string Translate(string key, string fallback = "not found:");
}
