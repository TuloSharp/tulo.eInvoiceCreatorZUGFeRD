using tulo.CommonMVVM.ViewModels;

namespace tulo.CommonMVVM.Stores
{
    public interface IModalNavigationStore
    {
        /// <summary>
        /// event is fired to asing the new CurrentViewModel
        /// </summary>
        event Action CurrentViewModelChanged;
        /// <summary>
        /// The CurrentViewModel can be sotred into this porperty
        /// </summary>
        BaseViewModel CurrentViewModel { get; set; }
        /// <summary>
        /// The CurrentViewModel is set to null
        /// </summary>
        void Close();
        /// <summary>
        /// Property show if the Modal View is opened or closed
        /// </summary>
        bool IsModalOpen { get; }
    }
}
