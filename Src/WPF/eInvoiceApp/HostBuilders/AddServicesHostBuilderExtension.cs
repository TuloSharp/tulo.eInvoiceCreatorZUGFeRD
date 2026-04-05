using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using System.IO;
using tulo.CommonMVVM.GlobalProperties;
using tulo.CoreLib.Interfaces.SnapShots;
using tulo.CoreLib.PDFs;
using tulo.CoreLib.Services;
using tulo.CoreLib.Translators;
using tulo.eInvoiceApp.Options;
using tulo.eInvoiceApp.Services;
using tulo.eInvoiceApp.Stores.Invoices;
using tulo.eInvoiceApp.Utilities;
using tulo.eInvoiceXmlGeneratorCii.Mappers;
using tulo.eInvoiceXmlGeneratorCii.Services;
using tulo.UpgradeToPdfA3.Interfaces;
using tulo.UpgradeToPdfA3.Options;
using tulo.UpgradeToPdfA3.Services;
using tulo.XMLeInvoiceToPdf.Languages;
using tulo.XMLeInvoiceToPdf.Services;

namespace tulo.eInvoiceApp.HostBuilders;

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

        #region Options

        services.AddOptions<AppOptions>()
            .Bind(configuration)
            .Validate(o => o.Vats != null && o.Vats.VatList != null && o.Vats.VatList.Count > 0,
                "Vats:VatList must not be empty.")
            .ValidateOnStart();

        services.AddSingleton<AppOptions>(sp => sp.GetRequiredService<IOptions<AppOptions>>().Value);

        services.AddSingleton<IAppOptions>(sp => sp.GetRequiredService<AppOptions>());

        services.AddOptions<UpgradeToPdfA3Options>()
            .Bind(configuration.GetSection("PdfA3"))
            .Validate(o => o.PdfA3 != null, "PdfA3:PdfA3 section is missing.")
            .Validate(o => !string.IsNullOrWhiteSpace(o.PdfA3.IccProfilePath), "PdfA3:PdfA3:IccProfilePath must not be empty.")
            .ValidateOnStart();

        services.AddSingleton<UpgradeToPdfA3Options>(sp => sp.GetRequiredService<IOptions<UpgradeToPdfA3Options>>().Value);

        services.AddSingleton<IUpgradeToPdfA3Options>(sp => sp.GetRequiredService<UpgradeToPdfA3Options>());

        #endregion

        #region Create Invoice
        services.AddSingleton<IInvoiceBuilderService, InvoiceBuilderService>();
        services.AddSingleton<IPdfGeneratorFromInvoice, PdfGeneratorFromInvoiceCii>();
        services.AddSingleton<IXmlCiiExporter, XmlCiiExporter>();
        services.AddSingleton<ICiiMapper, CiiMapper>();
        services.AddSingleton<IXmlObjectCleaner, XmlObjectCleaner>();
        services.AddSingleton<IPdfWatermarkService, PdfWatermarkService>();
        #endregion

        #region PDF/A and PDF/A-3
        services.AddSingleton<IPdfAConverterValidator, PdfAConverterValidator>();
        services.AddSingleton<IPdfADocumentInfoWriter, PdfADocumentInfoWriter>();
        services.AddSingleton<IPdfALanguageWriter, PdfALanguageWriter>();
        services.AddSingleton<IPdfAMetadataWriter, PdfAMetadataWriter>();
        services.AddSingleton<IPdfAOutputIntentWriter, PdfAOutputIntentWriter>();
        services.AddSingleton<IToPdfAConverterService, ToPdfAConverterService>();
        services.AddSingleton<IPdfA3UpgradeValidator, PdfA3UpgradeValidator>();
        services.AddSingleton<IPdfA3AttachmentWriter, PdfA3AttachmentWriter>();
        services.AddSingleton<IToPdfA3UpgradeService, ToPdfA3UpgradeService>();
        #endregion

        #region Localization
        services.AddSingleton<ICultureService, CultureService>();
        #endregion

        #region Translation
        services.AddSingleton<ITranslatorProvider>(sp =>
        {
            var cultureService = sp.GetRequiredService<ICultureService>();
            var culture = cultureService.CurrentCulture.Name;

            var asm = typeof(TranslatorProvider).Assembly;
            var resourceName = $"tulo.XMLeInvoiceToPdf.Languages.{culture}.xml";

            return new TranslatorProvider(asm, resourceName);
        });

        services.AddSingleton<ITranslatorUiProvider>(sp =>
        {
            var cultureService = sp.GetRequiredService<ICultureService>();
            var culture = cultureService.CurrentCulture.Name;

            var file = Path.Combine(AppContext.BaseDirectory, "Languages", $"Ui_{culture}.xml");
            return new TranslatorUiProvider(file);
        });
        #endregion

        #region Invoice
        services.AddSingleton<IInvoicePositionService, InvoicePositionService>();
        services.AddSingleton<IInvoicePositionLookupService, InvoicePositionLookupService>();
        #endregion
    }
    internal class WebServicesHostBuilderExtension { }
}
