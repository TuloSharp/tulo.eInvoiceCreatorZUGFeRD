namespace tulo.CommonMVVM.ViewModels;

//delegate without parameter
public delegate TViewModel CreateViewModel<TViewModel>() where TViewModel : BaseViewModel;
//delegate with parameter
public delegate TViewModel CreateViewModel<TParameter, TViewModel>(TParameter parameter) where TViewModel : BaseViewModel;
