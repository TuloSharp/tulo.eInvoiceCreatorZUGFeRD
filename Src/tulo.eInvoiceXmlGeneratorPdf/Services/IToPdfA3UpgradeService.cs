using tulo.XMLeInvoiceToPdf.Options;
using tulo.XMLeInvoiceToPdf.ResultPattern;

namespace tulo.XMLeInvoiceToPdf.Services;
public interface IToPdfA3UpgradeService
{
    OperationResult UpgradeToPdfA3(string inputPdfAPath, string outputPdfA3Path, string xmlFileName, byte[] xmlBytes, IAppOptions appOptions);
}
