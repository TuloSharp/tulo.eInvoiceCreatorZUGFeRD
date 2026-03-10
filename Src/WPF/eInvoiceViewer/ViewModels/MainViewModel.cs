using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using tulo.CommonMVVM.Collector;
using tulo.CommonMVVM.Stores;
using tulo.CommonMVVM.ViewModels;
using tulo.eInvoice.eInvoiceViewer.Commands.Common;
using tulo.eInvoice.eInvoiceViewer.Properties;
using tulo.eInvoice.eInvoiceViewer.Utilities;
using tulo.eInvoice.eInvoiceViewer.ViewModels.Factories;
using tulo.ResourcesWpfLib.Commands;
using tulo.ResourcesWpfLib.Viewmodels;

namespace tulo.eInvoice.eInvoiceViewer.ViewModels;
public class MainViewModel : BaseViewModel, IResizeWindowViewModel
{
    private readonly INavigatorViewModelFactory _navigatorViewModelFactory;
    private readonly INavigationStore _navigationStore;
    private readonly IModalStackNavigationStore _modalStackNavigationStore;

    #region BaseViewModel
    public BaseViewModel CurrentViewModel => _navigationStore.CurrentViewModel;
    public BaseViewModel CurrentModalViewModel => _modalStackNavigationStore.CurrentViewModel;
    public ObservableCollection<BaseViewModel> Modals => _modalStackNavigationStore.Modals;

    public bool IsModalOpen => _modalStackNavigationStore.IsModalOpen;


    private ICommand _updateCurrentViewModelCommand = null!;
    public ICommand UpdateCurrentViewModelCommand
    {
        get => _updateCurrentViewModelCommand;
        set => SetField(ref _updateCurrentViewModelCommand, value);
    }
    #endregion

    #region Size& Pos Window Properties

    private int Counter2AvoidSaveProperties { get; set; }

    private double _left;
    public double Left
    {
        get => _left;
        set
        {
            if (!SetField(ref _left, value)) return;

            Counter2AvoidSaveProperties++;
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
            if (!SetField(ref _top, value)) return;

            Counter2AvoidSaveProperties++;
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
            if (!SetField(ref _width, value)) return;

            Counter2AvoidSaveProperties++;
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
            if (!SetField(ref _height, value)) return;

            Counter2AvoidSaveProperties++;
            if (_windowState == WindowState.Normal && Counter2AvoidSaveProperties > 4)
                SaveWindowSize();
        }
    }

    private void SaveWindowPosition()
    {
        Settings.Default.NormalLeft = _left;
        Settings.Default.NormalTop = _top;
        Settings.Default.Save();
    }

    private void SaveWindowSize()
    {
        //only for this app 
        Settings.Default.NormalWidth = 740;
        //Settings.Default.NormalWidth = _width;
        Settings.Default.NormalHeight = _height;
        Settings.Default.Save();
    }

    private WindowState _windowState;
    public WindowState WindowState
    {
        get => _windowState;
        set
        {
            if (!SetField(ref _windowState, value)) return;
            SaveWindowState();
        }
    }

    private void SaveWindowState()
    {
        Settings.Default.WindowState = _windowState.ToString();
        Settings.Default.Save();
    }

    #endregion

    #region IManagementVisibleView
    private bool _focusableSecondaryControls;
    public bool FocusableSecondaryControls
    {
        get => _focusableSecondaryControls;
        set => SetField(ref _focusableSecondaryControls, value);
    }

    private bool _isMainWindow;
    public bool IsMainWindow
    {
        get => _isMainWindow;
        set => SetField(ref _isMainWindow, value);
    }
    #endregion

    #region UI Control Properties + IUiControlPropsViewModel
    private bool _isEnabledSaveRequestInUI;
    public bool IsEnabledSaveRequestInUI
    {
        get => _isEnabledSaveRequestInUI;
        set => SetField(ref _isEnabledSaveRequestInUI, value);
    }

    private bool _isAltShortcutKeyPressed;
    public bool IsAltShortcutKeyPressed
    {
        get => _isAltShortcutKeyPressed;
        set => SetField(ref _isAltShortcutKeyPressed, value);
    }

    private bool _isShortcutKeyAlreadyPressed;
    public bool IsAltShortcutKeyAlreadyPressed
    {
        get => _isShortcutKeyAlreadyPressed;
        set => SetField(ref _isShortcutKeyAlreadyPressed, value);
    }

    private bool _isDuplicate;
    public bool IsDuplicate
    {
        get => _isDuplicate;
        set => SetField(ref _isDuplicate, value);
    }

    private string _statusMessage = string.Empty;
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetField(ref _statusMessage, value);
    }
    public static string SelectedViewModel => nameof(ContentXmlToPdfViewerViewModel);
    public string CurrentViewModelName => SelectedViewModel;
    #endregion

    #region IResizeWindowViewModel
    private bool _isWindowMaximized;
    public bool IsWindowMaximized
    {
        get => _isWindowMaximized;
        set => SetField(ref _isWindowMaximized, value);
    }

    private bool _isWindowCustomResized;
    public bool IsWindowCustomResized
    {
        get => _isWindowCustomResized;
        set => SetField(ref _isWindowCustomResized, value);
    }

    #endregion

    #region Commands
    public ICommand MakeScreenshotCommand { get; }
    public ICommand CloseMainWindowCommand { get; }
    public ICommand MinimizedMainWindowCommand { get; }
    public ICommand ResizeMainWindowCommand { get; }
    public ICommand DragMoveMainWindowCommand { get; }
    public ICommand MouseLeftDoubleClickResizeWindowCommand { get; }
    public ICommand ExecuteAltF4Command { get; }
    #endregion

    //public ContentXmlToPdfViewerViewModel ContentXmlToPdfViewerViewModel { get; }

    public MainViewModel(INavigatorViewModelFactory navigatorViewModelFactory, ICollectorCollection collectorCollection)
    {
        //IsMainWindow = true;
        //MainWindowTitle = "eInvoice Viewer";

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
        _modalStackNavigationStore = collectorCollection.GetService<IModalStackNavigationStore>();
        #endregion

        _navigatorViewModelFactory = navigatorViewModelFactory;
        
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

        _navigationStore.CurrentViewModelChanged += OnNavigatorStateChanged_CurrentViewModelChanged;
        _modalStackNavigationStore.CurrentViewModelChanged += OnModalStackNavigationStore_CurrentModalStackViewModelChanged;

        UpdateCurrentViewModelCommand = new UpdateCurrentViewModelCommand(_navigatorViewModelFactory, collectorCollection);
        UpdateCurrentViewModelCommand.Execute(NavTypes.ContentXmlToPdfViewerView);
    }

    private void OnModalStackNavigationStore_CurrentModalStackViewModelChanged()
    {
        OnPropertyChanged(nameof(CurrentModalViewModel));
        OnPropertyChanged(nameof(IsModalOpen));
        OnPropertyChanged(nameof(Modals));
    }

    private void OnNavigatorStateChanged_CurrentViewModelChanged()
    {
        OnPropertyChanged(nameof(CurrentViewModel));

        //this used when the renavigation is called, the navigation item views are changed to the inital state
        var onChangedCurrentViewModel = CurrentViewModel.GetType().Name;
        if (onChangedCurrentViewModel == SelectedViewModel)
        {
            //MainWindowTitle = _translatorUiProvider.Translate("MainWindowTitle");
            IsMainWindow = true;
        }
    }

    #region Labels&Contents
    private string _mainWindowTitle = string.Empty;
    public string MainWindowTitle
    {
        get => _mainWindowTitle;
        set => SetField(ref _mainWindowTitle, value);
    }

    //private void FillAllLabelsAndContents()
    //{
    //    MainWindowTitle = _translatorUiProvider.Translate("MainWindowTitle");
    //}
    #endregion

    public override void Dispose()
    {
        _navigationStore.CurrentViewModelChanged -= OnNavigatorStateChanged_CurrentViewModelChanged;
        _modalStackNavigationStore.CurrentViewModelChanged -= OnModalStackNavigationStore_CurrentModalStackViewModelChanged;
        base.Dispose();
    }
}
