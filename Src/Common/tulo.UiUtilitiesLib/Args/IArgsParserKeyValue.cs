namespace tulo.UiUtilitiesLib.Args
{
    /// <summary>
    /// Parses command-line arguments into a dictionary, allowing access to key-value pairs like --path &lt;value&gt; --isTest &lt;value&gt;.
    /// </summary>
    public interface IArgsParserKeyValue
    {
        /// <summary>
        /// Parses command-line arguments into a dictionary of key-value pairs.
        /// The keys are the argument names (e.g., '--path') and the values are the associated values (e.g., '/mydir').
        /// Arguments with no values (e.g., flags) are excluded from the dictionary.
        /// </summary>
        /// <param name="args">The command-line arguments to parse.</param>
        /// <returns>A dictionary where the keys are argument names and the values are the corresponding values passed to them.</returns>
        Dictionary<string, string> ParseArguments(string[] args);
    }
}