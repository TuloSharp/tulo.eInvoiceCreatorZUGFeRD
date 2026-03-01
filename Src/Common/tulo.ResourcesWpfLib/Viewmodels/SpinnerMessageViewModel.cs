using tulo.CommonMVVM.ViewModels;

namespace tulo.ResourcesWpfLib.Viewmodels;

public class SpinnerMessageViewModel : BaseViewModel
{
    public bool IsLoading { get; set; }

    private string _displayedText;
    public string DisplayedText
    {
        get => _displayedText;
        set => SetField(ref _displayedText, value);  
    }

    public SpinnerMessageViewModel()
    {
        IsLoading = true;
    }
}