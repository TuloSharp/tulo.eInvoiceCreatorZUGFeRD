using tulo.FileOperations.ResultPatterns;

namespace tulo.FileOperations.Operations
{
    /// <summary>
    /// Interface for file transaction services.
    /// Defines methods for managing file operations in a transactional way.
    /// </summary>
    public interface IFileTransaction
    {
        /// <summary>
        /// Adds a file operation to the transaction.
        /// </summary>
        /// <param name="operation">The file operation to add.</param>
        void AddOperation(FileOperation operation);
        /// <summary>
        /// Executes all added file operations sequentially. Rolls back previous operations in case of failure.
        /// </summary>
        /// <returns>A result indicating the success or failure of the transaction.</returns>
        Result Execute();
    }
}