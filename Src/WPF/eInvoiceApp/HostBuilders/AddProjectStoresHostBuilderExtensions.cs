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


            //services.AddSingleton<ISelectedAccountStore, SelectedAccountStore>(delegate (IServiceProvider sp)
            //{

            //    return new AccountStore(new Account
            //    {
            //        AccountHolder =
            //    {
            //        Username = Environment.UserName
            //    },
            //        UserRole = Role.Standard,
            //    });
            //});

        });

        return host;
    }
}
