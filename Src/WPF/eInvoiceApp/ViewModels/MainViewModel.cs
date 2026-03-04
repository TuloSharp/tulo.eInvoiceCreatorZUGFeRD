using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using tulo.CommonMVVM.Collector;
using tulo.CommonMVVM.GlobalProperties;
using tulo.CommonMVVM.Stores;
using tulo.CommonMVVM.UiCommands;
using tulo.CommonMVVM.ViewModels;
using tulo.eInvoice.eInvoiceApp.Commands.Common;
using tulo.eInvoice.eInvoiceApp.Properties;
using tulo.eInvoice.eInvoiceApp.Utilities;
using tulo.eInvoice.eInvoiceApp.ViewModels.Factories;
using tulo.eInvoice.eInvoiceApp.ViewModels.Invoices;
using tulo.LoadingSpinnerControl.ViewModels;
using tulo.ResourcesWpfLib.Commands;
using tulo.ResourcesWpfLib.Viewmodels;

namespace tulo.eInvoice.eInvoiceApp.ViewModels;
public class MainViewModel : BaseViewModel, IResizeWindowViewModel
{
    private readonly INavigatorViewModelFactory _navigatorViewModelFactory;
    private readonly INavigationStore _navigationStore;
    private readonly IModalStackNavigationStore _modalStackNavigationStore;
    private readonly IGlobalPropsUiManage _globalPropes4UiControl;

    #region Selected Font Size
    //private double _selectedFontSize = 12;
    //public double SelectedFontSize
    //{
    //    get => _selectedFontSize;
    //    set => SetField(ref _selectedFontSize, value);
    //}

    private double _uiFontSize = 12;

    public double UiFontSize
    {
        get => _uiFontSize;
        set
        {
            var clamped = Math.Max(12, Math.Min(14, value));
            SetField(ref _uiFontSize, clamped);
        }
    }
    #endregion

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
    public static string SelectedViewModel => nameof(InvoiceViewModel);
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
    public ICommand IsAltShortcutKeyPressedCommand { get; }
    public ICommand IsAltShortcutKeyReleasedCommand { get; }
    public ICommand CloseMainWindowCommand { get; }
    public ICommand MinimizedMainWindowCommand { get; }
    public ICommand ResizeMainWindowCommand { get; }
    public ICommand DragMoveMainWindowCommand { get; }
    public ICommand MouseLeftDoubleClickResizeWindowCommand { get; }
    public ICommand ExecuteAltF4Command { get; }
    #endregion

    public MainViewModel(INavigatorViewModelFactory navigatorViewModelFactory, ICollectorCollection collectorCollection)
    {
        #region window size states
        var windowCurrentState = Settings.Default.WindowState.ToLower();
        if (windowCurrentState == "normal")
        {
            IsWindowCustomResized = true;
        }
        else if (windowCurrentState == "maximized")
        {
            IsWindowMaximized = true;
        }

        Counter2AvoidSaveProperties = 0;
        Left = Settings.Default.NormalLeft;
        Top = Settings.Default.NormalTop;
        Width = Settings.Default.NormalWidth;
        Height = Settings.Default.NormalHeight;
        WindowState = (WindowState)Enum.Parse(typeof(WindowState), Settings.Default.WindowState);
        #endregion

        #region Get Services / Stores from CollectorCollection
        _navigationStore = collectorCollection.GetService<INavigationStore>();
        _modalStackNavigationStore = collectorCollection.GetService<IModalStackNavigationStore>();
        _navigatorViewModelFactory = navigatorViewModelFactory;
        _globalPropes4UiControl = collectorCollection.GetService<IGlobalPropsUiManage>(); ;
        #endregion

        _navigationStore.CurrentViewModelChanged += OnNavigatorStateChanged_CurrentViewModelChanged;
        _modalStackNavigationStore.CurrentViewModelChanged += OnModalStackNavigationStore_CurrentModalStackViewModelChanged;

        UpdateCurrentViewModelCommand = new UpdateCurrentViewModelCommand(_navigatorViewModelFactory, collectorCollection);
        UpdateCurrentViewModelCommand.Execute(NavTypes.InvoiceView);

        #region Management UI Commands
        MakeScreenshotCommand = new SaveScreenshotAsPngCommand();
        IsAltShortcutKeyPressedCommand = new IsAltShortcutKeyPressedCommand(collectorCollection);
        IsAltShortcutKeyReleasedCommand = new IsAltShortcutKeyReleasedCommand(collectorCollection);
        CloseMainWindowCommand = new CloseMainWindowCommand();
        MinimizedMainWindowCommand = new MinimizedWindowCommand();
        ResizeMainWindowCommand = new ResizeWindowCommand(this);
        DragMoveMainWindowCommand = new MouseDownWindowCommand(this);
        MouseLeftDoubleClickResizeWindowCommand = new MouseLeftDoubleClickResizeWindowCommand(this);
        ExecuteAltF4Command = new ExecuteAltF4Command();
        #endregion

        FillAllInvoiceToolTips();
        FillAllLabelsAndContents();
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
            MainWindowTitle = "Create ZUGFeRD Invoice";
            IsMainWindow = true;
        }
    }

    #region ToolTips
    public string ToolTipInvoiceViewIcon { get; set; }= string.Empty;
    public string ToolTipBuyerViewIcon { get; set; }  = string.Empty;
    public string ToolTipAboutViewIcon { get; set; } = string.Empty;

    private void FillAllInvoiceToolTips()
    {
        ToolTipInvoiceViewIcon = "Create a new invoice.";
        ToolTipBuyerViewIcon = "Manage buyer information.";
        ToolTipAboutViewIcon = "View app info and features.";
    }
    #endregion

    #region Labels&Contents
    private string _mainWindowTitle = string.Empty;
    public string MainWindowTitle
    {
        get => _mainWindowTitle;
        set => SetField(ref _mainWindowTitle, value);
    }
    
    private void FillAllLabelsAndContents()
    {
        MainWindowTitle = "Create ZUGFeRD Invoice";
    }
    #endregion

    public override void Dispose()
    {
        _navigationStore.CurrentViewModelChanged -= OnNavigatorStateChanged_CurrentViewModelChanged;
        _modalStackNavigationStore.CurrentViewModelChanged -= OnModalStackNavigationStore_CurrentModalStackViewModelChanged;

        base.Dispose();
    }
}
