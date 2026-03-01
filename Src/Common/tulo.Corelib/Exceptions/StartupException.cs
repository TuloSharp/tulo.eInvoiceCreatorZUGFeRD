namespace tulo.CoreLib.Exceptions;

public class StartupException : Exception
{
    public StartupException() { }

    public StartupException(string message) : base(message) { }

    public StartupException(string message, Exception innerException) : base(message, innerException) { }
}
