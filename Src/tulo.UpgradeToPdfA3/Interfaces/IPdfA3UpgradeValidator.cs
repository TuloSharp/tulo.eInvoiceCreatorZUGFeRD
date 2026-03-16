using tulo.UpgradeToPdfA3.Options;
using tulo.UpgradeToPdfA3.ResultPattern;

namespace tulo.UpgradeToPdfA3.Interfaces;

public interface IPdfA3UpgradeValidator
{
    OperationResult Validate(string inputPdfAPath, string outputPdfA3Path, string xmlFileName, byte[] xmlBytes, IUpgradeToPdfA3Options appOptions);
}
