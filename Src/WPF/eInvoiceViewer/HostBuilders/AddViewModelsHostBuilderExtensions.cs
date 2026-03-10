using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using tulo.CommonMVVM.Collector;
using tulo.CommonMVVM.ViewModels;
using tulo.eInvoice.eInvoiceViewer.ViewModels;
using tulo.eInvoice.eInvoiceViewer.ViewModels.Factories;

namespace tulo.eInvoice.eInvoiceViewer.HostBuilders
{
    public static class AddViewModelsHostBuilderExtensions
    {
        public static IHostBuilder AddViewModels(this IHostBuilder host)
        {
            host.ConfigureServices((context, services) =>
            {
                services.AddSingleton<INavigatorViewModelFactory, NavigatorViewModelFactory>();

                //register view models
                services.AddSingleton(CreateContentXmlToPdfViewerViewModel);
                services.AddTransient<AboutViewModel>();

                //Main
                services.AddSingleton<MainViewModel>();
                services.AddSingleton<CreateViewModel<ContentXmlToPdfViewerViewModel>>(services => () => services.GetRequiredService<ContentXmlToPdfViewerViewModel>());
                services.AddSingleton<CreateViewModel<AboutViewModel>>(services => () => services.GetRequiredService<AboutViewModel>());
            });

            return host;
        }

        private static ContentXmlToPdfViewerViewModel CreateContentXmlToPdfViewerViewModel(IServiceProvider serviceProvider)
        {
            return new ContentXmlToPdfViewerViewModel(serviceProvider.GetRequiredService<ICollectorCollection>());
           
        }
    }
}