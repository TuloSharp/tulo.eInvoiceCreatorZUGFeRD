using System.Windows.Input;
using tulo.CommonMVVM.Collector;
using tulo.CommonMVVM.ViewModels;
using tulo.CoreLib.Translators;
using tulo.eInvoice.eInvoiceViewer.Commands;
using tulo.eInvoice.eInvoiceViewer.Utilities;

namespace tulo.eInvoice.eInvoiceViewer.ViewModels;
public class ContentXmlToPdfViewerViewModel : BaseViewModel
{
    private readonly ICollectorCollection _collectorCollection;
    private readonly ITranslatorUiProvider _translatorUiProvider;

    private string _documentSource = string.Empty;
    public string DocumentSource
    {
        get => _documentSource;
        set => SetField(ref _documentSource, value);
    }

    private bool _hasMessage;
    public bool HasMessage
    {
        get => _hasMessage;
        set => SetField(ref _hasMessage, value);
    }

    public MessageViewModel StatusMessageViewModel { get; }
    public string StatusMessage
    {
        get => StatusMessageViewModel.Message;
        set
        {
            if (StatusMessageViewModel.Message == value)
                return;

            StatusMessageViewModel.Message = value;
            HasMessage = !string.IsNullOrWhiteSpace(value);

            OnPropertyChanged(nameof(StatusMessage));
        }
    }

    public static string SelectedViewModel => nameof(ContentXmlToPdfViewerViewModel);

    #region Commands
    public ICommand XmlToPdfContentCommand { get; } = null!;
    public ICommand SelectXmlFilePathCommand { get; } = null!;
    public ICommand FileDroppedCommand { get; } = null!;
    #endregion

    public ContentXmlToPdfViewerViewModel(ICollectorCollection collectorCollection)
    {
        _collectorCollection = collectorCollection;
        #region Get Services / Stores from CollectorCollection
        var startupFileContext = _collectorCollection.GetService<IStartupFileContext>();
        _translatorUiProvider = collectorCollection.GetService<ITranslatorUiProvider>();
        #endregion

        StatusMessageViewModel = new MessageViewModel();

        XmlToPdfContentCommand = new XmlToPdfContentCommand(this, _collectorCollection);
        SelectXmlFilePathCommand = new SelectXmlFilePathCommand(this, _collectorCollection);
        FileDroppedCommand = new FileDroppedCommand(this, _collectorCollection);

        var filePath = startupFileContext.FilePath;
        XmlToPdfContentCommand.Execute(filePath);

        FillAllLabelsAndContents();
        FillAllToolTips();
    }

    #region Labels&Contents
    public string DragAndDropText { get; set; } = string.Empty;

    void FillAllLabelsAndContents()
    { 
        DragAndDropText = _translatorUiProvider.Translate("DragAndDropText");
    }
    #endregion

    #region ToolTips
    public string ToolTipDragAndDrop { get; set; } = string.Empty;
    public string ToolTipSelectFile { get; set; } = string.Empty;

    void FillAllToolTips()
    {
        ToolTipDragAndDrop = _translatorUiProvider.Translate("ToolTipDragAndDrop");
        ToolTipSelectFile = _translatorUiProvider.Translate("ToolTipSelectFile");
    } 
    #endregion
}
