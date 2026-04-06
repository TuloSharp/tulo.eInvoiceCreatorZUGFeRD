using Microsoft.Extensions.Hosting;
using tulo.eInvoiceCreatorZUGFeRD.HostBuilders;
using tulo.SerilogLib.HostBuilders;

namespace tulo.eInvoiceCreatorZUGFeRD;

/// <summary>
/// Responsible for initializing and building the application host for the WPF PDF viewer.
/// Encapsulates host configuration including services, stores, view models, views,
/// logging, and unhandled exception handling.
/// </summary>
public class UiApplication
{
    /// <summary>
    /// The file path to the parent application's configuration file.
    /// When provided, it is used to load additional app settings during host initialization.
    /// </summary>
    private readonly string? _parentConfigFilePath;

    /// <summary>
    /// Initializes a new instance of <see cref="UiApplication"/>.
    /// </summary>
    /// <param name="parentConfigFilePath">
    /// Optional path to a parent application configuration file (e.g. <c>appsettings.json</c>).
    /// Pass <see langword="null"/> to use default configuration only.
    /// </param>
    public UiApplication(string? parentConfigFilePath = null)
    {
        _parentConfigFilePath = parentConfigFilePath;
    }

    /// <summary>
    /// Creates and configures an <see cref="IHostBuilder"/> with all dependencies required
    /// by the WPF PDF viewer application.
    /// </summary>
    /// <remarks>
    /// The following registrations are applied in order:
    /// <list type="bullet">
    ///   <item><description>App settings from the optional parent config file path.</description></item>
    ///   <item><description>Serilog structured logging services.</description></item>
    ///   <item><description>Global unhandled exception handler.</description></item>
    ///   <item><description>Project-level data stores.</description></item>
    ///   <item><description>Application services.</description></item>
    ///   <item><description>Data collectors.</description></item>
    ///   <item><description>View models.</description></item>
    ///   <item><description>Views (WPF windows and user controls).</description></item>
    /// </list>
    /// </remarks>
    /// <returns>
    /// A fully configured <see cref="IHostBuilder"/> instance ready to be built.
    /// </returns>
    public IHostBuilder InitializeHostBuilder()
    {
        var host = Host.CreateDefaultBuilder()
                       .AddAppSettings(_parentConfigFilePath!)
                       .AddSerilogServices()
                       .AddSerilog()
                       .AddUnhandledExceptionHandler()
                       .AddProjectStores()
                       .AddServices()
                       .AddCollector()
                       .AddViewModels()
                       .AddViews();

        return host;
    }

    /// <summary>
    /// Builds the <see cref="IHost"/> from the given <see cref="IHostBuilder"/>,
    /// finalizing the dependency injection container and all registered services.
    /// </summary>
    /// <param name="hostBuilder">
    /// The <see cref="IHostBuilder"/> instance to build.
    /// Typically the result of <see cref="InitializeHostBuilder"/>.
    /// </param>
    /// <returns>
    /// A fully built <see cref="IHost"/> instance ready to be started.
    /// </returns>
    public IHost BuildHost(IHostBuilder hostBuilder) => hostBuilder.Build();
}
