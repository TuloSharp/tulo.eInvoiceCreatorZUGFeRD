using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using tulo.CommonMVVM.Collector;
using tulo.CommonMVVM.ViewModels;
using tulo.eInvoiceCreatorZUGFeRD.ViewModels;
using tulo.eInvoiceCreatorZUGFeRD.ViewModels.About;
using tulo.eInvoiceCreatorZUGFeRD.ViewModels.Factories;
using tulo.eInvoiceCreatorZUGFeRD.ViewModels.Invoices;
using tulo.eInvoiceCreatorZUGFeRD.ViewModels.Sellers;

namespace tulo.eInvoiceCreatorZUGFeRD.HostBuilders;

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
