using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using tulo.eInvoice.eInvoiceViewer.ViewModels;

namespace tulo.eInvoice.eInvoiceViewer.HostBuilders
{
    public static class AddViewModelsHostBuilderExtensions
    {
        public static IHostBuilder AddViewModels(this IHostBuilder host)
        {
            host.ConfigureServices((context, services) =>
            {
                //services.AddSingleton<INavigatorViewModelFactory, NavigatorViewModelFactory>();

                //register view models
                //services.AddTransient(CreatemployeeViewModel);
                //services.AddTransient<AboutViewModel>();
                //services.AddSingleton<SettingsViewModel>();
                //services.AddSingleton<PdfViewerViewModel>();
                //services.AddTransient<SpinnerMessageViewModel>();

                ////Renavigation Services
                //services.AddTransient<IRenavigationService<EmployeeCardListViewModel>, RenavigationService<EmployeeCardListViewModel>>();
                //services.AddTransient<IRenavigationService<AboutViewModel>, RenavigationService<AboutViewModel>>();
                //services.AddTransient<IRenavigationService<SettingsViewModel>, RenavigationService<SettingsViewModel>>();

                //Main
                services.AddSingleton<MainViewModel>();

                //services.AddSingleton<CreateViewModel<EmployeeCardListViewModel>>(services => () => services.GetRequiredService<EmployeeCardListViewModel>());
                //services.AddSingleton<CreateViewModel<AboutViewModel>>(services => () => services.GetRequiredService<AboutViewModel>());
                //services.AddSingleton<CreateViewModel<SettingsViewModel>>(services => () => services.GetRequiredService<SettingsViewModel>());
                //services.AddSingleton<CreateViewModel<PdfViewerViewModel>>(services => () => services.GetRequiredService<PdfViewerViewModel>());
                //services.AddSingleton<CreateViewModel<SpinnerMessageViewModel>>(services => () => services.GetRequiredService<SpinnerMessageViewModel>());

            });

            return host;
        }

        //private static EmployeeCardListViewModel CreatemployeeViewModel(IServiceProvider serviceProvider)
        //{
        //    return EmployeeCardListViewModel.LoadEmployeeViewModel(serviceProvider.GetRequiredService<IRenavigationService<EmployeeCardListViewModel>>(),
        //                                                           serviceProvider.GetRequiredService<ICollectorCollection>());
        //}

     
    }
}