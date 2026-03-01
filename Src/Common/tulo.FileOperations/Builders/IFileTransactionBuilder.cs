using tulo.FileOperations.Operations;
using tulo.FileOperations.ResultPatterns;

namespace tulo.FileOperations.Builders;

public interface IFileTransactionBuilder
{
    /// <summary>
    /// Adds a copy operation to the transaction.
    /// </summary>
    /// <param name="source">Source file.</param>
    /// <param name="destination">Destination file.</param>
    /// <returns>The same builder (for Fluent API).</returns>
    IFileTransactionBuilder Copy(string source, string destination);

    /// <summary>
    /// Adds a move operation to the transaction.
    /// </summary>
    IFileTransactionBuilder Move(string source, string destination);

    /// <summary>
    /// Adds a delete operation to the transaction.
    /// </summary>
    IFileTransactionBuilder Delete(string path);

    /// <summary>
    /// Adds a user-defined FileOperation.
    /// This also allows you to use your own operations.
    /// </summary>
    IFileTransactionBuilder AddOperation(FileOperation operation);

    /// <summary>
    /// Executes all added operations as a transaction.
    /// </summary>
    /// <returns>
    /// <see cref="Result"/> with success or error.
    /// If an error occurs, operations that have already been executed are rolled back.
    /// </returns>
    Result Execute();
}
