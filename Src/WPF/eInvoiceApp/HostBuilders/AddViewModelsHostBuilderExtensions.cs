using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using tulo.CommonMVVM.Collector;
using tulo.CommonMVVM.ViewModels;
using tulo.eInvoice.eInvoiceApp.ViewModels;
using tulo.eInvoice.eInvoiceApp.ViewModels.About;
using tulo.eInvoice.eInvoiceApp.ViewModels.Factories;
using tulo.eInvoice.eInvoiceApp.ViewModels.Invoices;
using tulo.eInvoice.eInvoiceApp.ViewModels.Sellers;

namespace tulo.eInvoice.eInvoiceApp.HostBuilders;

public static class AddViewModelsHostBuilderExtensions
{
    public static IHostBuilder AddViewModels(this IHostBuilder host)
    {
        host.ConfigureServices((context, services) =>
        {
            services.AddSingleton<INavigatorViewModelFactory, NavigatorViewModelFactory>();

            //register view models
            services.AddSingleton(CreateInvoiceViewModel);
            services.AddSingleton<SellerViewModel>();
            services.AddTransient<AboutViewModel>();

            //Main
            services.AddSingleton<MainViewModel>();

            services.AddSingleton<CreateViewModel<InvoiceViewModel>>(services => () => services.GetRequiredService<InvoiceViewModel>());
            services.AddSingleton<CreateViewModel<SellerViewModel>>(services => () => services.GetRequiredService<SellerViewModel>());
            services.AddSingleton<CreateViewModel<AboutViewModel>>(services => () => services.GetRequiredService<AboutViewModel>());
        });

        return host;
    }

    private static InvoiceViewModel CreateInvoiceViewModel(IServiceProvider serviceProvider)
    {
        //return new InvoiceViewModel(serviceProvider.GetRequiredService<ICollectorCollection>());
        return InvoiceViewModel.LoadInvoiceViewModel(serviceProvider.GetRequiredService<ICollectorCollection>());
    }

}
