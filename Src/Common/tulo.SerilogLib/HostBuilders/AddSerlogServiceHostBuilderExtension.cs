using tulo.SerilogLib.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;

namespace tulo.SerilogLib.HostBuilders;
public static class AddSerlogServiceHostBuilderExtension
{
    /// <summary>
    /// Adds application services (repositories) to the host.
    /// </summary>
    /// <param name="host">The <see cref="IHostBuilder"/> to configure.</param>
    /// <returns>The configured <see cref="IHostBuilder"/>.</returns>
    public static IHostBuilder AddSerilogServices(this IHostBuilder host)
    {
        var bootstrapLogger = Log.Logger.ForContext<WebServicesHostBuilderExtension>();

        host.ConfigureServices((context, services) =>
        {
            AddServicesInternal(services, context.Configuration);
        });

        bootstrapLogger.Information("Application additional Serilog Services has been initialized successfully.");
        return host;
    }

    /// <summary>
    /// Adds application services (repositories) to the host.
    /// </summary>
    /// <param name="host">The <see cref="IHostApplicationBuilder"/> to configure.</param>
    /// <returns>The configured <see cref="IHostApplicationBuilder"/>.</returns>
    public static IHostApplicationBuilder AddServices(this IHostApplicationBuilder host)
    {
        var bootstrapLogger = Log.Logger.ForContext<WebServicesHostBuilderExtension>();

        AddServicesInternal(host.Services, host.Configuration);

        bootstrapLogger.Information("Application additional Serilog Services has been initialized successfully.");
        return host;
    }

    /// <summary>
    /// Adds application services (repositories) to the host.
    /// </summary>
    /// <param name="host">The <see cref="WebApplicationBuilder"/> to configure.</param>
    /// <returns>The configured <see cref="WebApplicationBuilder"/>.</returns>
    public static WebApplicationBuilder AddServices(this WebApplicationBuilder host)
    {
        var bootstrapLogger = Log.Logger.ForContext<WebServicesHostBuilderExtension>();

        AddServicesInternal(host.Services, host.Configuration);

        bootstrapLogger.Information("Application additional Serilog Services has been initialized successfully.");
        return host;
    }

    /// <summary>
    /// Internal method to register application services with consistent configuration.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    private static void AddServicesInternal(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(new LoggingLevelSwitch());
        services.AddSingleton<IObservableLogSink, ObservableLogSink>();
    }
    internal class WebServicesHostBuilderExtension { }
}
