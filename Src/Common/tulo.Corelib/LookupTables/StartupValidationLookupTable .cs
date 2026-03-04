using tulo.CoreLib.Interfaces.LookupTables;
using tulo.CoreLib.Interfaces.StartupValidation;

namespace tulo.CoreLib.LookupTables
{
    public class StartupValidationLookupTable(IEnumerable<IStartupValidation> startupValidation) : IStartupValidationLookupTable
    {
        private readonly IEnumerable<IStartupValidation> _startupValidation = startupValidation;

        public IStartupValidation? GetStartupValidationByName(string name)
        {
            return _startupValidation.FirstOrDefault(sv => string.Equals(sv.Name, name, StringComparison.OrdinalIgnoreCase));
        }
    }
}