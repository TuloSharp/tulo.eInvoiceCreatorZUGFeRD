using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using tulo.CommonMVVM.Stores;

namespace tulo.eInvoiceCreatorZUGFeRD.HostBuilders;

public static class AddCommonStoresHostBuilderExtensions
{
    public static IHostBuilder AddCommonStores(this IHostBuilder host)
    {
        host.ConfigureServices((context, services) =>
        {
            services.AddSingleton<INavigationStore, NavigationStore>();
            services.AddSingleton<IModalStackNavigationStore, ModalStackNavigationStore>();
            //services.AddSingleton<ISelectedAccountStore, SelectedAccountStore>(sp =>
            //{
            //    var systemConfig = sp.GetRequiredService<ISystemConfiguration>();

            //    var account = new Account();

            //    account.AccountHolder.Username = Environment.UserName;
            //    account.UserRole = Role.Standard;


            //    return new AccountStore(account);
            //});
        });

        return host;
    }
}

