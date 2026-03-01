using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace tulo.SerilogLib.HostBuilders;

/// <summary>
/// Provides an extension method for <see cref="IHostBuilder"/> to register
/// a global unhandled exception handler during application startup.
/// </summary>
public static class AddUnhandledExceptionHostBuilderExtension
{
    public static IHostBuilder AddUnhandledExceptionHandler(this IHostBuilder host)
    {
        host.ConfigureServices((context, services) =>
        {
            var enabled = context.Configuration.GetValue<bool>("EnableLogger4UnhandledException");
            if (enabled)
            {
                services.AddHostedService<UnhandledExceptionHostedService>();
            }
        });

        return host;
    }

    private sealed class UnhandledExceptionHostedService : IHostedService
    {
        private readonly ILogger<UnhandledExceptionHostedService> _logger;

        public UnhandledExceptionHostedService(ILogger<UnhandledExceptionHostedService> logger)
            => _logger = logger;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
            TaskScheduler.UnobservedTaskException -= OnUnobservedTaskException;
            return Task.CompletedTask;
        }

        private void OnUnhandledException(object? sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                LogException(ex, "Unhandled exception", new { e.IsTerminating });
            }
            else
            {
                _logger.LogError(
                    "Unhandled exception occurred, but ExceptionObject was not an Exception. IsTerminating={IsTerminating}",
                    e.IsTerminating);
            }
        }

        private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            // Mark as observed to prevent escalation (behavior depends on runtime/config).
            e.SetObserved();

            LogException(e.Exception, "Unobserved task exception", new { Observed = true });
        }

        private void LogException(Exception ex, string title, object? extra = null)
        {
            // Fast path: TargetSite often provides useful type/method context without stack parsing.
            var typeName = ex.TargetSite?.DeclaringType?.FullName;
            var methodName = ex.TargetSite?.Name;

            if (!string.IsNullOrWhiteSpace(typeName) && !string.IsNullOrWhiteSpace(methodName))
            {
                _logger.LogError(ex,
                    "{Title} in {Type}.{Method}: {Message}. Extra={Extra}",
                    title, typeName, methodName, ex.Message, extra);
                return;
            }

            // Fallback: try to find the first non-System / non-Microsoft frame for more meaningful context.
            var (stackType, stackMethod) = TryGetFirstNonFrameworkFrame(ex);

            _logger.LogError(ex,
                "{Title} in {Type}.{Method}: {Message}. Extra={Extra}",
                title, stackType ?? "Unknown", stackMethod ?? "Unknown", ex.Message, extra);
        }

        private static (string? type, string? method) TryGetFirstNonFrameworkFrame(Exception ex)
        {
            var st = new StackTrace(ex, false);
            var frames = st.GetFrames();
            if (frames == null) return (null, null);

            foreach (var frame in frames)
            {
                var m = frame.GetMethod();
                var dt = m?.DeclaringType;
                var full = dt?.FullName;

                if (string.IsNullOrWhiteSpace(full)) continue;
                if (full.StartsWith("System.", StringComparison.Ordinal)) continue;
                if (full.StartsWith("Microsoft.", StringComparison.Ordinal)) continue;

                return (full, m!.Name);
            }

            return (null, null);
        }
    }
}