using Microsoft.Extensions.Hosting;
using System.IO;
using tulo.eInvoice.eInvoiceViewer.HostBuilders;
using tulo.SerilogLib.HostBuilders;

namespace tulo.eInvoice.eInvoiceViewer;

/// <summary>
/// Provides methods to initialize and build the application host for the WPF PDF viewer.
/// </summary>
public class UiApplication
{
    private readonly string[] _args;
    private readonly string? _parentConfigFilePath;

    public string? FileToOpen { get; }

    public UiApplication(string[] args = null!)
    {
        _args = args ?? Array.Empty<string>();

        // ✅ 1) read Config
        _parentConfigFilePath = GetArgValue(_args, "--config");

        // ✅ 2) file to open (XML)
        FileToOpen = GetArgValue(_args, "--pathFile");

        // ✅ 3) Explorer fallback: if no --pathFile, but 1st Arg is a file
        if (FileToOpen == null && _args.Length > 0)
        {
            var first = _args[0].Trim('"');
            if (File.Exists(first) && first.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            {
                FileToOpen = first;
            }
        }
    }

    private static string? GetArgValue(string[] args, string key)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].Equals(key, StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                return args[i + 1].Trim('"');
            }
        }
        return null;
    }

    /// <summary>
    /// Configures the specified <see cref="IHostBuilder"/> with required services, collectors, view models, and views.
    /// Also parses file arguments from the command line.
    /// </summary>
    /// <returns>The configured <see cref="IHostBuilder"/> instance.</returns>
    public IHostBuilder InitializeHostBuilder()
    {
        var host = Host.CreateDefaultBuilder(_args)
                                 .AddAppSettings(_parentConfigFilePath!)
                                 .AddSerilogServices()
                                 .AddSerilog()
                                 .AddUnhandledExceptionHandler()
                                 .AddProjectStores()
                                 .AddServices(FileToOpen!)
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
