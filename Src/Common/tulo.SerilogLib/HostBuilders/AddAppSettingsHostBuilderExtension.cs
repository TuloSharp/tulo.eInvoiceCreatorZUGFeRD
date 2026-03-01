using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace tulo.SerilogLib.HostBuilders;

/// <summary>
/// Loads configuration files from multiple sources including:
/// - The base directory
/// - An environment-specific file
/// - An optional parent-directory-based external configuration
/// </summary>
public static class AddAppSettingsHostBuilderExtension
{
    public static IHostBuilder AddAppSettings(this IHostBuilder host, string parentConfigFilePath)
    {
        var machine = Environment.MachineName.ToLowerInvariant();
        var bootstrapLogger = Log.Logger.ForContext<AddAppSettingsLogContext>();

        host.ConfigureAppConfiguration((context, configBuilder) =>
        {
            // helper to reduce duplication & ensure consistent behavior/logging
            void AddJsonIfExists(
                string path,
                bool optional,
                string successMessage,
                string notFoundMessage,
                bool reloadOnChange = false)
            {
                if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                {
                    configBuilder.AddJsonFile(path, optional: optional, reloadOnChange: reloadOnChange);
                    bootstrapLogger.Information(successMessage);
                }
                else
                {
                    bootstrapLogger.Warning(notFoundMessage);
                }
            }

            // 1) load parent/plex config file (if provided)
            AddJsonIfExists(
                parentConfigFilePath,
                optional: false,
                successMessage: $"settings file '{parentConfigFilePath}' for machine '{machine}' is loaded...",
                notFoundMessage: $"settings file '{parentConfigFilePath}' for machine '{machine}' is not found...");

            // 2) machine-specific appsettings next to the app
            var machineNameAppSettingsPath = $"appsettings.{machine}.json";
            AddJsonIfExists(
                machineNameAppSettingsPath,
                optional: true,
                successMessage: $"settings file '{machineNameAppSettingsPath}' for machine '{machine}' is loaded...",
                notFoundMessage: $"settings file '{machineNameAppSettingsPath}' for machine '{machine}' is not found...",
                reloadOnChange: true);

            // 3) app data folder: ../{AppDirName}-appsettings/appsettings.json
            var contentRoot = context.HostingEnvironment.ContentRootPath;
            var parentDir = Directory.GetParent(contentRoot);

            if (parentDir != null)
            {
                var appDirName = new DirectoryInfo(contentRoot).Name;
                var appSettingsFolder = Path.Combine(parentDir.FullName, $"{appDirName}-appsettings");
                var appSettingsPath = Path.Combine(appSettingsFolder, "appsettings.json");

                AddJsonIfExists(
                    appSettingsPath,
                    optional: true,
                    successMessage: $"settings file 'appsettings.json' from app data folder '{appSettingsFolder}' is loaded...",
                    notFoundMessage: $"settings file 'appsettings.json' from app data folder '{appSettingsFolder}' is not found...",
                    reloadOnChange: true);
            }

            // 4) Additional parameters
            var additionalParametersFile = $"AdditionalParameters_{machine}.json";
            AddJsonIfExists(
                additionalParametersFile,
                optional: false,
                successMessage: $"{additionalParametersFile} is loaded",
                notFoundMessage: $"{additionalParametersFile} is not found");
        });

        return host;
    }
}

internal sealed class AddAppSettingsLogContext { }