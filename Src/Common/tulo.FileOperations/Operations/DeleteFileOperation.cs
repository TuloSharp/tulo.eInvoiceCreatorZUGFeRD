using tulo.FileOperations.ResultPatterns;

namespace tulo.FileOperations.Operations;

public class DeleteFileOperation : FileOperation
{
    private readonly string _path;
    private string? _backupPath;
    private bool _wasDeleted;

    public DeleteFileOperation(string path)
    {
        _path = path;
    }

    public override Result Execute()
    {
        if (string.IsNullOrWhiteSpace(_path))
            return Result.Failure(new Error("Pfad ist leer.", "FILE_INVALID_ARGUMENT"));

        if (!File.Exists(_path))
            return Result.Success(); // Nichts zu tun

        try
        {
            _backupPath = Path.GetTempFileName();
            File.Copy(_path, _backupPath, overwrite: true);
            File.Delete(_path);

            _wasDeleted = true;
            return Result.Success();
        }
        catch (IOException ex)
        {
            return Result.Failure(new Error(ex.Message, "FILE_DELETE_ERROR"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Result.Failure(new Error(ex.Message, "FILE_ACCESS_DENIED"));
        }
    }

    public override Result Rollback()
    {
        if (!_wasDeleted || string.IsNullOrEmpty(_backupPath) || !File.Exists(_backupPath))
            return Result.Success(); // nichts zu retten

        try
        {
            if (!File.Exists(_path))
            {
                File.Copy(_backupPath, _path, overwrite: true);
            }

            File.Delete(_backupPath);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error(ex.Message, "FILE_ROLLBACK_ERROR"));
        }
    }
}
