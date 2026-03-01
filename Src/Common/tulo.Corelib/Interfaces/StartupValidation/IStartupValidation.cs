using tulo.CoreLib.Components.ResultPattern;

namespace tulo.CoreLib.Interfaces.StartupValidation
{
    /// <summary>
    /// Defines a contract for startup validation.
    /// </summary>
    public interface IStartupValidation
    {
        /// <summary>
        /// Gets the name of the plugin.
        /// This is typically used for logging and identifying the plugin during execution.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Validates the startup conditions and returns the result.
        /// </summary>
        /// <returns>
        /// A <see cref="Result{T}"/> containing a boolean value indicating whether the validation was successful.
        /// </returns>
        Result<bool> Validate();
    }
}
