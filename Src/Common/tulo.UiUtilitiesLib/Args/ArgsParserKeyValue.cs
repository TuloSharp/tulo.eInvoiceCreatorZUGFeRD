namespace tulo.UiUtilitiesLib.Args
{
    /// <summary>
    /// Provides functionality to parse command-line arguments.
    /// </summary>
    public class ArgsParserKeyValue : IArgsParserKeyValue
    {
        /// <inheritdoc />
        public Dictionary<string, string> ParseArguments(string[] args)
        {
            var parsedArgs = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];

                if (arg.StartsWith("--"))
                {
                    // This is a flag argument like --path
                    string key = arg;
                    string? value = null;

                    // If there's a value after the flag (e.g., --path /mydir), set the value
                    if (i + 1 < args.Length && !args[i + 1].StartsWith("--"))
                    {
                        value = args[i + 1];
                        value = RemoveQuotes(value); // Remove quotes if they exist (for values with spaces)
                        i++; // Skip the next item since it's the value of this argument
                    }

                    // Add to the dictionary
                    if (value != null)
                        parsedArgs[key] = value;
                }
            }

            return parsedArgs;
        }

        // Helper method to remove quotes around a value, if any
        private string RemoveQuotes(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.StartsWith("\"") && value.EndsWith("\""))
            {
                return value.Substring(1, value.Length - 2); // Remove the first and last characters (the quotes)
            }
            return value;
        }
    }
}
