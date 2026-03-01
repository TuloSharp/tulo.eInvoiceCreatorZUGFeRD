using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using tulo.CommonMVVM.Collector;
using tulo.CommonMVVM.Services;
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
            //services.AddSingleton<PdfViewerViewModel>();
            //services.AddTransient<SpinnerMessageViewModel>();

            //Renavigation Services
            //services.AddTransient<IRenavigationService<InvoiceViewModel>, RenavigationService<InvoiceViewModel>>();
            //services.AddTransient<IRenavigationService<SellerViewModel>, RenavigationService<SellerViewModel>>();
            //services.AddTransient<IRenavigationService<AboutViewModel>, RenavigationService<AboutViewModel>>();

            //Main
            services.AddSingleton<MainViewModel>();

            services.AddSingleton<CreateViewModel<InvoiceViewModel>>(services => () => services.GetRequiredService<InvoiceViewModel>());
            services.AddSingleton<CreateViewModel<SellerViewModel>>(services => () => services.GetRequiredService<SellerViewModel>());
            services.AddSingleton<CreateViewModel<AboutViewModel>>(services => () => services.GetRequiredService<AboutViewModel>());
            //services.AddSingleton<CreateViewModel<PdfViewerViewModel>>(services => () => services.GetRequiredService<PdfViewerViewModel>());
            //services.AddSingleton<CreateViewModel<SpinnerMessageViewModel>>(services => () => services.GetRequiredService<SpinnerMessageViewModel>());
        });

        return host;
    }

    private static InvoiceViewModel CreateInvoiceViewModel(IServiceProvider serviceProvider)
    {
        //return InvoiceViewModel.LoadInvoiceViewModel(serviceProvider.GetRequiredService<IRenavigationService<InvoiceViewModel>>(),
        //                                                       serviceProvider.GetRequiredService<ICollectorCollection>());
        //return new InvoiceViewModel(serviceProvider.GetRequiredService<ICollectorCollection>());
        return InvoiceViewModel.LoadInvoiceViewModel(serviceProvider.GetRequiredService<ICollectorCollection>());
    }

}
