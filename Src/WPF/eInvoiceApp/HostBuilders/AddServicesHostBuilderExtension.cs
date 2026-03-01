using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using System.IO;
using tulo.CommonMVVM.GlobalProperties;
using tulo.CoreLib.Interfaces.SnapShots;
using tulo.CoreLib.Services;
using tulo.CoreLib.Translators;
using tulo.eInvoice.eInvoiceApp.Options;
using tulo.eInvoice.eInvoiceApp.Services;
using tulo.eInvoice.eInvoiceApp.Stores.Invoices;
using tulo.eInvoice.eInvoiceApp.Utilities;
using tulo.eInvoiceXmlGeneratorCii.Mappers;
using tulo.eInvoiceXmlGeneratorCii.Services;
using tulo.UiUtilitiesLib.PDFs;
using tulo.XMLeInvoiceToPdf.Languages;
using tulo.XMLeInvoiceToPdf.Services;

namespace tulo.eInvoice.eInvoiceApp.HostBuilders;
public static class AddServicesHostBuilderExtension
{
    /// <summary>
    /// Adds application services (repositories) to the host.
    /// </summary>
    /// <param name="host">The <see cref="IHostBuilder"/> to configure.</param>
    /// <returns>The configured <see cref="IHostBuilder"/>.</returns>
    public static IHostBuilder AddServices(this IHostBuilder host)
    {
        var bootstrapLogger = Log.Logger.ForContext<WebServicesHostBuilderExtension>();

        host.ConfigureServices((context, services) =>
        {
            AddServicesInternal(services, context.Configuration);
        });

        bootstrapLogger.Information("Application Services has been initialized successfully.");
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

        AddServicesInternal(host.Services, host.Configuration);

        bootstrapLogger.Information("Application Services has been initialized successfully.");
        return host;
    }

    /// <summary>
    /// Internal method to register application services with consistent configuration.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    private static void AddServicesInternal(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<SettingsPropertyUpdateUtility>();

        #region Common
        services.AddSingleton<IGlobalPropsUiManage, GlobalPropsUiManage>();
        services.AddSingleton<ISnapShotService, SnapShotService>();
        #endregion

        #region Stores
        services.AddSingleton<IInvoicePositionStore, InvoicePositionStore>();
        services.AddSingleton<ISelectedInvoicePositionStore, SelectedInvoicePositionStore>();
        #endregion

        #region Invoice
        services.AddSingleton<IInvoicePositionService, InvoicePositionService>();
        #endregion

        #region Options
        services.AddOptions<AppOptions>()
            .Bind(configuration, o => o.BindNonPublicProperties = true)
              //.Validate(o => !string.IsNullOrWhiteSpace(o.Archive.Path), "Archive:Path is required.")
            .Validate(o => o.Vats.VatList.Count > 0,"Vats:VatList must not be empty.")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IAppOptions>(sp => sp.GetRequiredService<IOptions<AppOptions>>().Value);
        #endregion

        #region Create Invoice
        services.AddSingleton<IInvoiceBuilderService, InvoiceBuilderService>();
       
        //translator for invoice creation (e.g. for error messages) - optional, can be overridden by external file later
        services.AddSingleton<ITranslatorProvider>(sp =>
        {
            var opt = sp.GetRequiredService<IAppOptions>();

            var culture = opt?.Language?.Culture;
            culture = string.IsNullOrWhiteSpace(culture) ? "en" : culture.Trim();

            // optional: "de-DE" -> "de"
            var dash = culture.IndexOf('-');
            if (dash > 0) culture = culture.Substring(0, dash);

            var asm = typeof(TranslatorProvider).Assembly;

            var resourceName = $"tulo.XMLeInvoiceToPdf.Languages.{culture}.xml";
            return new TranslatorProvider(asm, resourceName);

        });
        services.AddSingleton<IPdfGeneratorFromInvoice, PdfGeneratorFromInvoiceCii>();
        services.AddSingleton<IXmlCiiExporter, XmlCiiExporter>();
        services.AddSingleton<ICiiMapper, CiiMapper>();
        services.AddSingleton<IXmlObjectCleaner, XmlObjectCleaner>();
        services.AddSingleton<IPdfWatermarkService, PdfWatermarkService>();

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

    }
    internal class WebServicesHostBuilderExtension { }
}
