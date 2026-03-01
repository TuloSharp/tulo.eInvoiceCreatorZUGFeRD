namespace tulo.CoreLib.SystemConfig;

/// <summary>
/// Provides configuration settings for the system, including environment variables, 
/// help file paths, transaction timeouts, process management, registry identifiers, 
/// and executable paths. Inherits RabbitMQ configuration.
/// </summary>
public interface ISystemConfiguration
{
    /// <summary>
    /// Gets or sets the path to the help file.
    /// </summary>
    string Path4HelpDoc { get; set; }

    /// <summary>
    /// Gets or sets the timeout (in milliseconds) for cancelling a transaction in each request.
    /// </summary>
    int Timeout4CancellingTransaction { get; set; }

    /// <summary>
    /// Gets or sets the parent process ID to avoid opening more .NET instances.
    /// </summary>
    string ParentProcessId { get; set; }
}
