namespace tulo.CoreLib.SystemConfig
{
    /// <summary>
    /// Represents the system configuration for the application, including environment, application, and RabbitMQ parameters.
    /// </summary>
    public class SystemConfiguration : ISystemConfiguration
    {
        #region App Instance Parameters
        /// <summary>
        /// Gets or sets the timeout (in milliseconds) for cancelling a transaction in each request.
        /// </summary>
        public int Timeout4CancellingTransaction { get; set; } = 0;
        #endregion

        #region Instance Parameters
        /// <summary>
        /// Gets or sets the path to the help file.
        /// </summary>
        public string Path4HelpDoc { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the parent process ID to avoid opening more .NET instances.
        /// </summary>
        public string ParentProcessId { get; set; } = string.Empty;

        #endregion

        #region RMQ Config Parameters
        /// <summary>
        /// Gets or sets the prefix for the queue name, used to connect with the corresponding database.
        /// </summary>
        public string Prefix { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the RabbitMQ port. The default value is null.
        /// </summary>
        public int? Port { get; set; }

        /// <summary>
        /// Gets or sets the RabbitMQ hostname, specifying which server to use.
        /// </summary>
        public string Hostname { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the username to connect to RabbitMQ.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the password to connect to RabbitMQ.
        /// </summary>
        public string Password { get; set; } = string.Empty;

        #endregion
    }
}
