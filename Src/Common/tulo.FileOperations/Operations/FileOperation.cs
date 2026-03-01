using tulo.FileOperations.ResultPatterns;

namespace tulo.FileOperations.Operations
{
    /// <summary>
    /// Represents an abstract base class for file operations that can be executed and rolled back.
    /// Derives from this class to implement specific file operations such as copy, delete, etc.
    /// </summary>
    public abstract class FileOperation
    {
        /// <summary>
        /// Executes the file operation.
        /// This method should contain the logic to perform the operation on the file(s).
        /// </summary>
        /// <remarks>
        /// Derived classes should implement the specific logic for the file operation, such as copying, deleting, renaming, etc.
        /// </remarks>
        public abstract Result Execute();

        /// <summary>
        /// Rolls back the file operation if it has failed or needs to be undone.
        /// This method should reverse the effects of the executed operation.
        /// </summary>
        /// <remarks>
        /// Derived classes should implement the specific logic for rolling back the operation, e.g., removing a copied file,
        /// restoring a deleted file, etc.
        /// </remarks>
        public abstract Result Rollback();
    }
}
