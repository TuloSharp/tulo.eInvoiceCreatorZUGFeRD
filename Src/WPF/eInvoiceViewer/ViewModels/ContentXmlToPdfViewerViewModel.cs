using System.Reflection;
using System.Windows.Input;
using tulo.CommonMVVM.Collector;
using tulo.CommonMVVM.ViewModels;
using tulo.eInvoice.eInvoiceViewer.Commands;
using tulo.eInvoice.eInvoiceViewer.Utilities;

namespace tulo.eInvoice.eInvoiceViewer.ViewModels;
public class ContentXmlToPdfViewerViewModel : BaseViewModel
{
    private readonly ICollectorCollection _collectorCollection;

    private string _documentSource = string.Empty;
    public string DocumentSource
    {
        get => _documentSource;
        set => SetField(ref _documentSource, value);
    }

    private string _versionApp = string.Empty;
    public string VersionApp
    {
        get => _versionApp;
        set => SetField(ref _versionApp, value);
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
        #endregion

        StatusMessageViewModel = new MessageViewModel();

        XmlToPdfContentCommand = new XmlToPdfContentCommand(this, _collectorCollection);
        SelectXmlFilePathCommand = new SelectXmlFilePathCommand(this, _collectorCollection);
        FileDroppedCommand = new FileDroppedCommand(this, _collectorCollection);

        VersionApp = GetProgramVersion();

        var filePath = startupFileContext.FilePath;
        XmlToPdfContentCommand.Execute(filePath);
    }

    public static string GetProgramVersion()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        return version != null ? $"Version: v{version.Major}.{version.Minor}.{version.Build}" : "Unknown Version";
    }
}
