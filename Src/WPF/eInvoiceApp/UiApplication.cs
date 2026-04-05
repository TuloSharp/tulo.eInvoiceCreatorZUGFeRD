using Microsoft.Extensions.Hosting;
using tulo.eInvoiceApp.HostBuilders;
using tulo.SerilogLib.HostBuilders;

namespace tulo.eInvoiceApp;

/// <summary>
/// Provides methods to initialize and build the application host for the WPF PDF viewer.
/// </summary>
public class UiApplication
{
    private readonly string? _parentConfigFilePath;

    /// <summary>
    /// Configures the specified <see cref="IHostBuilder"/> with required services, collectors, view models, and views.
    /// Also parses file arguments from the command line.
    /// </summary>
    /// <returns>The configured <see cref="IHostBuilder"/> instance.</returns>
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
    /// Builds the application host from the specified <see cref="IHostBuilder"/>.
    /// </summary>
    /// <param name="hostBuilder">The host builder to build.</param>
    /// <returns>The built <see cref="IHost"/> instance.</returns>
    public IHost BuildHost(IHostBuilder hostBuilder) => hostBuilder.Build();
}
