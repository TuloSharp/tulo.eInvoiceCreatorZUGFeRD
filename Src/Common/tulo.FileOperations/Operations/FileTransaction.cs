using tulo.FileOperations.ResultPatterns;

namespace tulo.FileOperations.Operations;
/// <summary>
/// Executes file operations in a transactional sequence. If a failure occurs, a rollback of previously executed operations is performed.
/// </summary>
public class FileTransaction : IFileTransaction
{
    private readonly List<FileOperation> _operations = [];

    public void AddOperation(FileOperation operation)
    {
        _operations.Add(operation);
    }

    public Result Execute()
 {
     var executed = new Stack<FileOperation>();

     try
     {
         foreach (var operation in _operations)
         {
             Result opResult;

             try
             {
                 opResult = operation.Execute();
             }
             catch (Exception ex)
             {
                 opResult = Result.Failure( new Error( $"Uexpected exception  in {operation.GetType().Name}: {ex.Message}", "FILE_TXN_UNEXPECTED"));
             }

             if (opResult.IsFailure)
             {
                // Rollback is triggered here in Execute() (error case)
                 var rollbackResult = Rollback(executed);

                 if (rollbackResult.IsFailure)
                 {
                     return Result.Failure( new Error( $"Operation {operation.GetType().Name} failed: {opResult.Error.Message} | " +
                             $"Rollback-Error: {rollbackResult.Error.Message}", "FILE_TXN_ROLLBACK"));
                 }

                 return opResult; // return original error
             }

             executed.Push(operation);
         }

         return Result.Success();
     }
     finally
     {
         executed.Clear();
         _operations.Clear();
     }
 }

 private Result Rollback(Stack<FileOperation> executed)
 {
     while (executed.Count > 0)
     {
         var op = executed.Pop();

         try
         {
             var result = op.Rollback();
             if (result.IsFailure)
                 return result;
         }
         catch (Exception ex)
         {
             return Result.Failure( new Error( $"Unexpected exception during rollback in {op.GetType().Name}: {ex.Message}", "FILE_TXN_ROLLBACK_EXCEPTION"));
         }
         finally
         {
             // Optional: Release resources if FileOperation is IDisposable
             (op as IDisposable)?.Dispose();
         }
     }

     return Result.Success();
    }
}
