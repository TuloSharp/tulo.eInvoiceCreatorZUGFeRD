using tulo.CoreLib.Interfaces.StartupValidation;

namespace tulo.CoreLib.Interfaces.LookupTables
{
    /// <summary>
    /// Provides a LookupTable interface for retrieving specific <see cref="IStartupValidation"/> implementations by name.
    /// </summary>
    public interface IStartupValidationLookupTable
    {
        /// <summary>
        /// Retrieves a specific <see cref="IStartupValidation"/> implementation based on the provided name.
        /// </summary>
        /// <param name="name">The unique name identifying the desired <see cref="IStartupValidation"/> implementation.</param>
        /// <returns>
        /// An instance of <see cref="IStartupValidation"/> matching the specified name, or <c>null</c> if no match is found.
        /// </returns>
        IStartupValidation? GetStartupValidationByName(string name);
    }
}
