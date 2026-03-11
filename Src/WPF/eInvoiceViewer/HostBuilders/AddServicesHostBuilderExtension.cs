using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Core;
using System.IO;
using tulo.CommonMVVM.GlobalProperties;
using tulo.CoreLib.Translators;
using tulo.eInvoice.eInvoiceViewer.Options;
using tulo.eInvoice.eInvoiceViewer.Utilities;
using tulo.XMLeInvoiceToPdf.Languages;
using tulo.XMLeInvoiceToPdf.Services;

namespace tulo.eInvoice.eInvoiceViewer.HostBuilders;
public static class AddServicesHostBuilderExtension
{
    /// <summary>
    /// Adds application services (repositories) to the host.
    /// </summary>
    /// <param name="host">The <see cref="IHostBuilder"/> to configure.</param>
    /// <returns>The configured <see cref="IHostBuilder"/>.</returns>
    public static IHostBuilder AddServices(this IHostBuilder host, string fileToOpen)
    {
        var bootstrapLogger = Log.Logger.ForContext<WebServicesHostBuilderExtension>();

        host.ConfigureServices((context, services) =>
        {
            AddServicesInternal(services, context.Configuration, fileToOpen);
        });

        bootstrapLogger.Information("Application Services has been initialized successfully.");
        return host;
    }

    /// <summary>
    /// Adds application services (repositories) to the host.
    /// </summary>
    /// <param name="host">The <see cref="IHostApplicationBuilder"/> to configure.</param>
    /// <returns>The configured <see cref="IHostApplicationBuilder"/>.</returns>
    public static IHostApplicationBuilder AddServices(this IHostApplicationBuilder host, string? fileToOpen)
    {
        var bootstrapLogger = Log.Logger.ForContext<WebServicesHostBuilderExtension>();

        AddServicesInternal(host.Services, host.Configuration, fileToOpen);

        bootstrapLogger.Information("Application Services has been initialized successfully.");
        return host;
    }

    /// <summary>
    /// Adds application services (repositories) to the host.
    /// </summary>
    /// <param name="host">The <see cref="WebApplicationBuilder"/> to configure.</param>
    /// <returns>The configured <see cref="WebApplicationBuilder"/>.</returns>
    public static WebApplicationBuilder AddServices(this WebApplicationBuilder host, string? fileToOpen)
    {
        var bootstrapLogger = Log.Logger.ForContext<WebServicesHostBuilderExtension>();

        AddServicesInternal(host.Services, host.Configuration, fileToOpen);

        bootstrapLogger.Information("Application Services has been initialized successfully.");
        return host;
    }

    /// <summary>
    /// Internal method to register application services with consistent configuration.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    private static void AddServicesInternal(IServiceCollection services, IConfiguration configuration, string? pathToFile)
    {
        services.AddSingleton<SettingsPropertyUpdateUtility>();

        #region Ui Global Control Props
        services.AddSingleton<IGlobalPropsUiManage, GlobalPropsUiManage>();
        #endregion


        #region Options
        services.AddOptions<AppOptions>()
            .Bind(configuration, o => o.BindNonPublicProperties = true)
            //.Validate(o => !string.IsNullOrWhiteSpace(o.Archive.Path), "Archive:Path is required.")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IAppOptions>(sp => sp.GetRequiredService<IOptions<AppOptions>>().Value);
        #endregion

        #region Translation
        services.AddSingleton<ITranslatorProvider>(sp =>
        {
            // Optional: external override via Config/Env/Registry later
            // e.g. config["Translation:Path"] or environment variable
            var cfg = sp.GetRequiredService<IConfiguration>();
            var externalPath = cfg["Translation:Path"]; // may be empty

            if (!string.IsNullOrWhiteSpace(externalPath) && File.Exists(externalPath))
            {
                return new TranslatorProvider(externalPath);
            }

            // Fallback: Embedded
            var asm = typeof(TranslatorProvider).Assembly;
            var resourceName = "tulo.XMLeInvoiceToPdf.Languages.de.xml";
            return new TranslatorProvider(asm, resourceName);
        });
        //translator for ui
        services.AddSingleton<ITranslatorUiProvider>(sp =>
        {
            var opt = sp.GetRequiredService<IAppOptions>();

            var culture = opt?.Language?.Culture;
            culture = string.IsNullOrWhiteSpace(culture) ? "en" : culture.Trim();

            // optional: "de-DE" -> "de"
            var dash = culture.IndexOf('-');
            if (dash > 0) culture = culture.Substring(0, dash);

            var file = Path.Combine(AppContext.BaseDirectory, "Languages", $"Ui_{culture}.xml");
            return new TranslatorUiProvider(file);
        });

        #endregion

        #region AppRunner
        services.AddSingleton<IPdfGeneratorResolver, PdfGeneratorResolver>();
        services.AddSingleton<IPdfGeneratorFromInvoice, PdfGeneratorFromInvoiceCii>();
        services.AddSingleton<IPdfGeneratorFromInvoice, PdfGeneratorFromInvoiceUbl>();
        services.AddSingleton<IAppRunner, AppRunner>();
        #endregion

        #region File Context
        services.AddSingleton<IStartupFileContext>(_ => new StartupFileContext(pathToFile));
        #endregion
    }
    internal class WebServicesHostBuilderExtension { }
}
