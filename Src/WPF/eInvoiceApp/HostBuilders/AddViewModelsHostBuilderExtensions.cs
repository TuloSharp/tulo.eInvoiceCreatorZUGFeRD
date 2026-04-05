using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using tulo.CommonMVVM.Collector;
using tulo.CommonMVVM.ViewModels;
using tulo.eInvoiceApp.ViewModels;
using tulo.eInvoiceApp.ViewModels.About;
using tulo.eInvoiceApp.ViewModels.Factories;
using tulo.eInvoiceApp.ViewModels.Invoices;
using tulo.eInvoiceApp.ViewModels.Sellers;

namespace tulo.eInvoiceApp.HostBuilders;

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
