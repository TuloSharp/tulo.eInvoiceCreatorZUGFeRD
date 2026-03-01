using s2industries.ZUGFeRD;

namespace tulo.CreateZugferdPdfA3.ConverterToPdfA3;
public interface IZugferdPdfA3ConverterService
{
    Task<Result<string>> ConvertAsync(string inputPdfPath, string inputXmlPath, string outputPdfPath, ZUGFeRDVersion zugferdVersion, Profile profile, ZUGFeRDFormats format = ZUGFeRDFormats.CII);
}
