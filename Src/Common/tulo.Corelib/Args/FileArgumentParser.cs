namespace tulo.CoreLib.Args
{
    /// <summary>
    /// Parses file arguments from a given array of command-line arguments, filtering by file type.
    /// </summary>
    public class FileArgumentParser : IFileArgumentParser
    {
        private readonly IReadOnlyList<string> _filePaths;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileArgumentParser"/> class.
        /// </summary>
        /// <param name="args">The array of command-line arguments to parse.</param>
        /// <param name="fileType">The file extension to filter by (including the dot, e.g., ".txt").</param>
        public FileArgumentParser(string[] args, string fileType)
        {
            _filePaths = args?
                .Where(arg => File.Exists(arg) && Path.GetExtension(arg).Equals(fileType, StringComparison.OrdinalIgnoreCase))
                .ToList()
                ?? new List<string>();
        }

        /// <summary>
        /// Gets the list of file paths that match the specified file type and exist on disk.
        /// </summary>
        /// <returns>A read-only list of valid file paths.</returns>
        public IReadOnlyList<string> GetFilePaths() => _filePaths;
    }
}
