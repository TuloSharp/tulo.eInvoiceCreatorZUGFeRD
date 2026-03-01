using tulo.FileOperations.Operations;
using tulo.FileOperations.ResultPatterns;

namespace tulo.FileOperations.Builders;

public class FileTransactionBuilder : IFileTransactionBuilder
{
    private readonly IFileTransaction _transaction;

    /// <summary>
    /// Creates a new builder with an internal <see cref="FileTransaction"/>.
    /// </summary>
    public FileTransactionBuilder()
        : this(new FileTransaction())
    {
    }

    /// <summary>
    /// Creates a new builder with a provided <see cref="IFileTransaction"/>.
    /// </summary>
    public FileTransactionBuilder(IFileTransaction transaction)
    {
        _transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
    }

    public IFileTransactionBuilder Copy(string source, string destination)
    {
        _transaction.AddOperation(new CopyFileOperation(source, destination));
        return this;
    }

    public IFileTransactionBuilder Move(string source, string destination)
    {
        _transaction.AddOperation(new MoveFileOperation(source, destination));
        return this;
    }

    public IFileTransactionBuilder Delete(string path)
    {
        _transaction.AddOperation(new DeleteFileOperation(path));
        return this;
    }

    public IFileTransactionBuilder AddOperation(FileOperation operation)
    {
        if (operation == null) throw new ArgumentNullException(nameof(operation));

        _transaction.AddOperation(operation);
        return this;
    }

    public Result Execute()
    {
        return _transaction.Execute();
    }
}

