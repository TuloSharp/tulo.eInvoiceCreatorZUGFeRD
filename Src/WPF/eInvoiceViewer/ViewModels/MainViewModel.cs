using System.Windows;
using System.Windows.Input;
using tulo.CommonMVVM.Collector;
using tulo.CommonMVVM.Stores;
using tulo.CommonMVVM.ViewModels;
using tulo.CoreLib.SystemConfig;
using tulo.ResourcesWpfLib.Commands;
using tulo.ResourcesWpfLib.Viewmodels;

namespace tulo.eInvoice.eInvoiceViewer.ViewModels;
public class MainViewModel : BaseViewModel, IResizeWindowViewModel
{
    private readonly INavigationStore _navigationStore;
    private readonly IModalNavigationStore _modalNavigationStore;
    private readonly ISystemConfiguration _systemConfiguration;

    private int Counter2AvoidSaveProperties { get; set; }

    #region Properties to manage VIEWS into the shell
    private bool _isMainWindow;
    public bool IsMainWindow
    {
        get => _isMainWindow;
        set
        {
            if (_isMainWindow == value) return;
            _isMainWindow = value;
            OnPropertyChanged(nameof(IsMainWindow));
        }
    }

    private string _mainWindowTitle = string.Empty;
    public string MainWindowTitle
    {
        get => _mainWindowTitle;
        set
        {
            if (_mainWindowTitle == value) return;
            _mainWindowTitle = value;
            OnPropertyChanged(nameof(MainWindowTitle));
        }
    }
    #endregion

    #region Helper Properties for UI ViewModel
    private bool _isAltShortcutKeyPressed;
    public bool IsAltShortcuKeyPressed
    {
        get => _isAltShortcutKeyPressed;
        set => SetField(ref _isAltShortcutKeyPressed, value);
    }
    public bool IsKeyAlreadyPressed { get; set; }

    public bool IsDuplicate { get; set; }
    public bool EnableSaveRequestInUI { get; set; }
    public bool IsEnableToSaveData { get; set; }
    #endregion

    #region Window UI Commands
    private bool _isWindowMaximized;
    public bool IsWindowMaximized
    {
        get => _isWindowMaximized;
        set
        {
            if (_isWindowMaximized == value) return;
            _isWindowMaximized = value;
            OnPropertyChanged(nameof(IsWindowMaximized));
        }
    }

    private bool _isWindowCustomResized;
    public bool IsWindowCustomResized
    {
        get => _isWindowCustomResized;
        set
        {
            if (_isWindowCustomResized == value) return;
            _isWindowCustomResized = value;
            OnPropertyChanged(nameof(IsWindowCustomResized));
        }
    }

    public ICommand CloseMainWindowCommand { get; }
    public ICommand MinimizedMainWindowCommand { get; }
    public ICommand ResizeMainWindowCommand { get; }
    public ICommand DragMoveMainWindowCommand { get; }
    public ICommand MouseLeftDoubleClickResizeWindowCommand { get; }
    public ICommand ExecuteAltF4Command { get; }
    //public ICommand OpenAppInfoMessageCommand { get; }

    private double _left;
    public double Left
    {
        get => _left;
        set
        {
            if (_left == value) return;
            _left = value;
            Counter2AvoidSaveProperties++;
            OnPropertyChanged(nameof(Left));
            if (_windowState == WindowState.Normal && Counter2AvoidSaveProperties > 4)
                SaveWindowPosition();
        }
    }

    private double _top;
    public double Top
    {
        get => _top;
        set
        {
            if (_top == value) return;
            _top = value;
            Counter2AvoidSaveProperties++;
            OnPropertyChanged(nameof(Top));
            if (_windowState == WindowState.Normal && Counter2AvoidSaveProperties > 4)
                SaveWindowPosition();
        }
    }

    private double _width;
    public double Width
    {
        get => _width;
        set
        {
            if (_width == value) return;
            _width = value;
            Counter2AvoidSaveProperties++;
            OnPropertyChanged(nameof(Width));
            if (_windowState == WindowState.Normal && Counter2AvoidSaveProperties > 4)
                SaveWindowSize();
        }
    }

    private double _height;
    public double Height
    {
        get => _height;
        set
        {
            if (_height == value) return;
            _height = value;
            Counter2AvoidSaveProperties++;
            OnPropertyChanged(nameof(Height));
            if (_windowState == WindowState.Normal && Counter2AvoidSaveProperties > 4)
                SaveWindowSize();
        }
    }

    private void SaveWindowPosition()
    {
        Properties.Settings.Default.NormalLeft = _left;
        Properties.Settings.Default.NormalTop = _top;
        Properties.Settings.Default.Save();
    }

    private void SaveWindowSize()
    {
        Properties.Settings.Default.NormalWidth = _width;
        Properties.Settings.Default.NormalHeight = _height;
        Properties.Settings.Default.Save();
    }

    private WindowState _windowState;
    public WindowState WindowState
    {
        get => _windowState;
        set
        {
            if (_windowState == value) return;
            _windowState = value;
            OnPropertyChanged(nameof(WindowState));
            SaveWindowState();
        }
    }

    private void SaveWindowState()
    {
        Properties.Settings.Default.WindowState = _windowState.ToString();
        Properties.Settings.Default.Save();
    }
    #endregion

    #region Management UI Commands
    public ICommand MakeScreenshotCommand { get; }
    //public ICommand IsAltKeyPressedCommand { get; }
    #endregion

    public string StatusMessage { get; set; } = string.Empty;
    public string CurrentViewModelName { get; set; } = string.Empty;

    public ContentXmlToPdfViewerViewModel ContentXmlToPdfViewerViewModel { get; }

    public MainViewModel(ICollectorCollection collectorCollection)
    {
        IsMainWindow = true;
        MainWindowTitle = "eInvoice Viewer";

        #region window size states
        var windowCurrentState = Properties.Settings.Default.WindowState.ToLower();
        if (windowCurrentState == "normal")
        {
            IsWindowCustomResized = true;
        }
        else if (windowCurrentState == "maximized")
        {
            IsWindowMaximized = true;
        }

        Counter2AvoidSaveProperties = 0;
        Left = Properties.Settings.Default.NormalLeft;
        Top = Properties.Settings.Default.NormalTop;
        Width = Properties.Settings.Default.NormalWidth;
        Height = Properties.Settings.Default.NormalHeight;
        WindowState = (WindowState)Enum.Parse(typeof(WindowState), Properties.Settings.Default.WindowState);
        #endregion

        #region resolve service from CollectorCollection
        _navigationStore = collectorCollection.GetService<INavigationStore>();
        _modalNavigationStore = collectorCollection.GetService<IModalNavigationStore>();
        _systemConfiguration = collectorCollection.GetService<ISystemConfiguration>();
        #endregion

        #region Window UI Commands
        CloseMainWindowCommand = new CloseMainWindowCommand();
        MinimizedMainWindowCommand = new MinimizedWindowCommand();
        ResizeMainWindowCommand = new ResizeWindowCommand(this);
        DragMoveMainWindowCommand = new MouseDownWindowCommand(this);
        MouseLeftDoubleClickResizeWindowCommand = new MouseLeftDoubleClickResizeWindowCommand(this);
        ExecuteAltF4Command = new ExecuteAltF4Command();
       
        #endregion

        #region Management UI Commands
        MakeScreenshotCommand = new SaveScreenshotAsPngCommand();
        //IsAltKeyPressedCommand = new IsAltKeyPressedCommand(this);
        //ShortcutKeyIsReleased = new ShortcutKeyIsReleased(this);
        #endregion

        ContentXmlToPdfViewerViewModel = new ContentXmlToPdfViewerViewModel(collectorCollection);
    }

    public override void Dispose()
    {
        base.Dispose();
    }
}
