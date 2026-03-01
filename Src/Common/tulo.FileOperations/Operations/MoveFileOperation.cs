using tulo.FileOperations.ResultPatterns;

namespace tulo.FileOperations.Operations;

public class MoveFileOperation : FileOperation
{
    private readonly string _source;
    private readonly string _destination;
    private bool _wasMoved;

    public MoveFileOperation(string source, string destination)
    {
        _source = source;
        _destination = destination;
    }

    public override Result Execute()
    {
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

            File.Move(_source, _destination);
            _wasMoved = true;

            return Result.Success();
        }
        catch (IOException ex)
        {
            return Result.Failure(new Error(ex.Message, "FILE_MOVE_ERROR"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Result.Failure(new Error(ex.Message, "FILE_ACCESS_DENIED"));
        }
    }

    public override Result Rollback()
    {
        if (!_wasMoved)
            return Result.Success();

        try
        {
            if (File.Exists(_destination))
            {
                File.Move(_destination, _source);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error(ex.Message, "FILE_ROLLBACK_ERROR"));
        }
    }
}
