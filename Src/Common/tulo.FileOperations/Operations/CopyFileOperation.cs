using tulo.FileOperations.ResultPatterns;

namespace tulo.FileOperations.Operations;

public class CopyFileOperation : FileOperation
{
    private readonly string _source;
    private readonly string _destination;
    private bool _wasCopied;

    public CopyFileOperation(string source, string destination)
    {
        // Keine throws mehr – einfach nur speichern
        _source = source;
        _destination = destination;
    }

    public override Result Execute()
    {
        // Argument-Check über Result
        if (string.IsNullOrWhiteSpace(_source) || string.IsNullOrWhiteSpace(_destination))
            return Result.Failure(new Error("Source oder Destination ist leer.", "FILE_INVALID_ARGUMENT"));

        if (!File.Exists(_source))
            return Result.Failure(new Error($"Quelldatei nicht gefunden: {_source}", "FILE_NOT_FOUND"));

        try
        {
            var directory = Path.GetDirectoryName(_destination);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // bewusst: Ziel darf nicht existieren
            File.Copy(_source, _destination, overwrite: false);
            _wasCopied = true;

            return Result.Success();
        }
        catch (IOException ex)
        {
            return Result.Failure(new Error(ex.Message, "FILE_IO_ERROR"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Result.Failure(new Error(ex.Message, "FILE_ACCESS_DENIED"));
        }
    }

    public override Result Rollback()
    {
        if (!_wasCopied)
            return Result.Success();

        try
        {
            if (File.Exists(_destination))
            {
                File.Delete(_destination);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error(ex.Message, "FILE_ROLLBACK_ERROR"));
        }
    }
}