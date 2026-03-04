using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using tulo.CommonMVVM.Collector;
using tulo.CommonMVVM.GlobalProperties;
using tulo.CommonMVVM.Stores;
using tulo.CoreLib.SystemConfig;

namespace tulo.eInvoice.eInvoiceApp.HostBuilders;

public static class AddCollectorHostBuilderExtensions
{
    public static IHostBuilder AddCollector(this IHostBuilder host)
    {
        host.ConfigureServices((HostBuilderContext context, IServiceCollection services) =>
        {
            services.AddSingleton<ICollectorCollection>(serviceProvider =>
            {
                var collectorCollection = new CollectorCollection();

                // utility method to add service only if registered
                void TryAdd<T>() where T : class
                {
                    var service = serviceProvider.GetService<T>();
                    if (service != null)
                        collectorCollection.AddService(service);
                }

                // Services
                TryAdd<IGlobalPropsUiManage>();
                TryAdd<ILoggerFactory>();
                TryAdd<ISystemConfiguration>();

                // Stores
                TryAdd<INavigationStore>();
                TryAdd<IModalStackNavigationStore>();
                //TryAdd<ISelectedAccountStore>();

                return collectorCollection;
            });
        });
        return host;
    }
}