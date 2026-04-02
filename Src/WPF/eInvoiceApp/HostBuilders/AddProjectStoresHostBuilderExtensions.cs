using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using tulo.CommonMVVM.Stores;

namespace tulo.eInvoice.eInvoiceApp.HostBuilders;

public static class AddProjectStoresHostBuilderExtensions
{
    public static IHostBuilder AddProjectStores(this IHostBuilder host)
    {
        host.ConfigureServices((context, services) =>
        {
            #region Navigation
            services.AddSingleton<INavigationStore, NavigationStore>();
            services.AddSingleton<IModalStackNavigationStore, ModalStackNavigationStore>();
            #endregion
        });

        return host;
    }
}
