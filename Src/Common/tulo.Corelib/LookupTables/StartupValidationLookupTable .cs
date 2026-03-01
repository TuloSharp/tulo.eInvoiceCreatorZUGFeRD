using tulo.CoreLib.Interfaces.LookupTables;
using tulo.CoreLib.Interfaces.StartupValidation;

namespace tulo.CoreLib.LookupTables
{
    /// <inheritdoc />
    public class StartupValidationLookupTable : IStartupValidationLookupTable
    {
        private readonly IEnumerable<IStartupValidation> _startupValidation;

        /// <inheritdoc />
        public StartupValidationLookupTable(IEnumerable<IStartupValidation> startupValidation)
        {
            _startupValidation = startupValidation;
        }

        /// <inheritdoc />
        public IStartupValidation? GetStartupValidationByName(string name)
        {
            return _startupValidation.FirstOrDefault(sv => string.Equals(sv.Name, name, StringComparison.OrdinalIgnoreCase));
        }
    }
}