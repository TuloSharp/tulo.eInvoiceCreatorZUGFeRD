namespace tulo.UiUtilitiesLib.Args
{
    /// <summary>
    /// Defines a contract for parsing and retrieving file path arguments.
    /// </summary>
    public interface IFileArgumentParser
    {
        /// <summary>
        /// Gets a read-only list of file paths parsed from arguments.
        /// </summary>
        /// <returns>
        /// A read-only list of strings representing file paths.
        /// </returns>
        IReadOnlyList<string> GetFilePaths();
    }
}
