using tulo.CommonMVVM.ViewModels;

namespace tulo.LoadingSpinnerControl.ViewModels;

public class SpinnerMessageViewModel : BaseViewModel
{
    private bool _isLoading = true;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetField(ref _isLoading, value);
    }

    private string _displayedText = string.Empty;
    public string DisplayedText
    {
        get => _displayedText;
        set => SetField(ref _displayedText, value);
    }

    private string _textMessage = string.Empty;
    public string TextMessage
    {
        get => _textMessage;
        set => SetField(ref _textMessage, value);
    }

    public SpinnerMessageViewModel()
    {
        // Already defaulted to true in the field initializer,
        // but keeping it explicit is fine:
        IsLoading = true;
    }
}
