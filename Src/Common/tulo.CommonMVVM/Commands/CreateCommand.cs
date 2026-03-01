using System.Windows.Input;
using tulo.CommonMVVM.ViewModels;

namespace tulo.CommonMVVM.Commands;

public delegate ICommand CreateCommand<TViewModel>(TViewModel viewModel) where TViewModel : BaseViewModel;
