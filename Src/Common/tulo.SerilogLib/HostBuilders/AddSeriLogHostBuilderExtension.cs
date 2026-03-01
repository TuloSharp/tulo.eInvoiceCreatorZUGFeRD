using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using tulo.SerilogLib.Common;

namespace tulo.SerilogLib.HostBuilders;

public static class AddSeriLogHostBuilderExtension
{
    /// <summary>
    /// Configures Serilog as the logging provider for the host.
    /// </summary>
    /// <param name="host">The <see cref="IHostBuilder"/> to configure.</param>
    /// <returns>The configured <see cref="IHostBuilder"/>.</returns>
    public static IHostBuilder AddSerilog(this IHostBuilder host)
    {
        var bootstrapLogger = Log.Logger.ForContext<AddSerilog>();

        host.UseSerilog((host, services, loggerConfig) =>
        {
            var levelSwitch = services.GetRequiredService<LoggingLevelSwitch>();
            var liveSink = services.GetRequiredService<IObservableLogSink>() as ILogEventSink;

            levelSwitch.MinimumLevel = host.Configuration.GetSection("Serilog:MinimumLevel:Default").Get<LogEventLevel>();

            loggerConfig.MinimumLevel.ControlledBy(levelSwitch)
                        .WriteTo.Sink(liveSink!)
                        .ReadFrom.Configuration(host.Configuration)
                        .Enrich.With<UsernameEnrichment>()
                        .Enrich.WithThreadId()
                        .Enrich.WithProcessId()
                        .Enrich.FromLogContext();

            // MSSqlServer sink exists
            if (SerilogConfigUtility.HasMSSqlServerSink(host.Configuration))
            {
                loggerConfig.Enrich.WithProperty("IsAcknowledged", false);
                loggerConfig.Enrich.WithProperty("Direction", "none");
            }
        });

        bootstrapLogger.Information("Serilog has been initialized successfully.");

        return host;
    }
}

internal class AddSerilog;
