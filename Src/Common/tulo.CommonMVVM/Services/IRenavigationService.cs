using tulo.CommonMVVM.ViewModels;

namespace tulo.CommonMVVM.Services;

public interface IRenavigationService
{
    Type ViewModelType { get; }
    void Renavigate();
}

public interface IRenavigationService<TViewModel> : IRenavigationService
    where TViewModel : BaseViewModel
{
    // no new member needed, serves only as type safety
}