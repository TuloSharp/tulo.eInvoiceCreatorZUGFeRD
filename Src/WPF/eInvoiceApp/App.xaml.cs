using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PdfSharp.Fonts;
using Serilog;
using Serilog.Core;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using tulo.CommonMVVM.Collector;
using tulo.CoreLib.Exceptions;
using tulo.CoreLib.Interfaces.SnapShots;
using tulo.CoreLib.PDFs;
using tulo.CoreLib.Translators;
using tulo.CreateZugferdPdfA3.ConverterToPdfA3;
using tulo.eInvoice.eInvoiceApp;
using tulo.eInvoice.eInvoiceApp.Options;
using tulo.eInvoice.eInvoiceApp.Properties;
using tulo.eInvoice.eInvoiceApp.Services;
using tulo.eInvoice.eInvoiceApp.Stores.Invoices;
using tulo.eInvoice.eInvoiceApp.Utilities;
using tulo.eInvoiceApp;
using tulo.eInvoiceXmlGeneratorCii.Mappers;
using tulo.eInvoiceXmlGeneratorCii.Services;
using tulo.SerilogLib.Common;
using tulo.XMLeInvoiceToPdf.Services;
using tulo.XMLeInvoiceToPdf.Utilities;
using WpfApplication = System.Windows.Application;

namespace tulo.eInvoiceManager;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : WpfApplication
{
    /// <summary>
    /// The application host used for dependency injection and service management.
    /// </summary>
    private IHost? _host;

    //private readonly string _xmlFilepath = "string.Empty";

    /// <summary>
    /// Logger instance for the application.
    /// </summary>
    private Microsoft.Extensions.Logging.ILogger? _logger;

    /// <summary>
    /// folder path for WebView2 user data
    /// </summary>
    public static string WebView2UserDataFolder { get; private set; } = string.Empty;

    /// <summary>
    /// Mutex used to enforce single-instance application behavior.
    /// </summary>
    private static Mutex? _mutex;

    /// <summary>
    /// Handles application startup logic, including:
    /// - Host and logger initialization
    /// - Configuration loading
    /// - Service registration
    /// - Single-instance enforcement
    /// - Main window display
    /// </summary>
    /// <param name="e">Startup event arguments.</param>
    protected override void OnStartup(StartupEventArgs e)
    {
        #region Set Working Directory + Project name
        var exePath = Assembly.GetExecutingAssembly().Location;
        var exeDir = Path.GetDirectoryName(exePath);
        if (!string.IsNullOrEmpty(exeDir))
        {
            Directory.SetCurrentDirectory(exeDir);
        }
        var _projectName = Assembly.GetEntryAssembly()?.GetName().Name?.Split('.')?.Last();
        #endregion

        #region WebView2
        var appName = Assembly.GetEntryAssembly()?.GetName().Name ?? "App";
        var tempBase = Path.GetTempPath();
        WebView2UserDataFolder = Path.Combine(tempBase, appName, $"pid-{Environment.ProcessId}");
        Directory.CreateDirectory(WebView2UserDataFolder);
        Environment.SetEnvironmentVariable("WEBVIEW2_USER_DATA_FOLDER", WebView2UserDataFolder, EnvironmentVariableTarget.Process);
        #endregion

        #region Settings: Font
        GlobalFontSettings.FontResolver = new EmbeddedFontResolver();
        #endregion

        #region Create SerilogBootstrapLogger
        var bootstrapLogger = CreateBootstrapLogger();
        #endregion

        #region Enable selfLogErrors
#if DEBUG
        var selfLogErrors = new ConcurrentBag<string>();
        Serilog.Debugging.SelfLog.Enable(selfLogErrors.Add);
#endif
        #endregion

        #region Create HostBuilder
        var app = new UiApplication();
        var hostBuilder = app.InitializeHostBuilder();
        _host = app.BuildHost(hostBuilder);
        #endregion

        #region Add Project releated services/stores into CollectorCollection
        using var scope = _host.Services.CreateScope();
        _logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(App).FullName!);

        //----------------Get Collector----------------
        var collector = scope.ServiceProvider.GetRequiredService<ICollectorCollection>();

        //---------------- AppOptions ----------------
        AddToCollectorRequired<IAppOptions>(scope.ServiceProvider, collector);

        //---------------- LiveLogs in UI ----------------
        AddToCollectorRequired<IObservableLogSink>(scope.ServiceProvider, collector);
        AddToCollectorRequired<LoggingLevelSwitch>(scope.ServiceProvider, collector);

        // --------------- Stores ----------------
        AddToCollectorRequired<IInvoicePositionStore>(scope.ServiceProvider, collector);
        AddToCollectorRequired<ISelectedInvoicePositionStore>(scope.ServiceProvider, collector);

        // --------------- services ----------------
        AddToCollectorRequired<ITranslatorUiProvider>(scope.ServiceProvider, collector);
        AddToCollectorRequired<IPdfWatermarkService>(scope.ServiceProvider, collector);

        // --------------- Invoices ----------------
        AddToCollectorRequired<IInvoiceBuilderService>(scope.ServiceProvider, collector);
        AddToCollectorRequired<IInvoicePositionService>(scope.ServiceProvider, collector);
        AddToCollectorRequired<IInvoicePositionLookupService>(scope.ServiceProvider, collector);
        AddToCollectorRequired<IZugferdPdfA3ConverterService>(scope.ServiceProvider, collector);
        AddToCollectorRequired<IPdfGeneratorFromInvoice>(scope.ServiceProvider, collector);
        AddToCollectorRequired<IXmlCiiExporter>(scope.ServiceProvider, collector);
        AddToCollectorRequired<ICiiMapper>(scope.ServiceProvider, collector);
        AddToCollectorRequired<ISnapShotService>(scope.ServiceProvider, collector);
        #endregion

        #region App Process Name
        //var _systemConfiguration = scope.ServiceProvider.GetRequiredService<ISystemConfiguration>();
        //var parentProcessId = _systemConfiguration.ParentProcessId;
        var parentProcessId = Environment.ProcessId.ToString();
        string mutexName = _projectName + parentProcessId;
        #endregion

        #region Startup App
        _host.Start();
        _logger.LogInformation($"'{nameof(OnStartup)}' : App is started");

        Window window = scope.ServiceProvider.GetRequiredService<MainWindow>();
        SettingsPropertyUpdateUtility settingsPropertyUpdateHelper = scope.ServiceProvider.GetRequiredService<SettingsPropertyUpdateUtility>();

#if DEBUG
        // Check for SelfLog errors before starting app
        if (!selfLogErrors.IsEmpty)
        {
            _logger.LogError("Serilog internal errors detected, application will not start.");
            foreach (var msg in selfLogErrors) { _logger.LogError("{Message}", msg); }

            // Throw exception to prevent app startup
            throw new StartupException("Serilog internal errors detected. Startup aborted.");
        }
#endif
        #endregion

        #region Check if process is running
        _mutex = new Mutex(true, mutexName, out bool createdNew);
        if (!createdNew)
        {
            Process currentProcess = Process.GetCurrentProcess();
            Process[] processes = Process.GetProcessesByName(currentProcess.ProcessName);

            foreach (Process process in processes)
            {
                // Ensure we are not checking the current process
                if (process.Id != currentProcess.Id)
                {
                    // Bring the main window of the running instance to the foreground
                    IntPtr mainWindowHandle = process.MainWindowHandle;
                    if (mainWindowHandle != IntPtr.Zero)
                    {
                        //if win is minimized
                        NativeMethods.ShowWindow(mainWindowHandle, NativeMethods.SW_RESTORE);
                        //bring win to the foreground
                        NativeMethods.SetForegroundWindow(mainWindowHandle);
                        break;
                    }
                }
            }

            Current.Shutdown();
            return;
        }
        #endregion

        #region Show App
        window.Show();
        _logger.LogInformation($"'{nameof(OnStartup)}' : UI is thrown");
        base.OnStartup(e);
        #endregion
    }
    /// <summary>
    /// Handles application exit logic, including:
    /// - Saving user settings
    /// - Logging shutdown
    /// - Stopping and disposing the host
    /// </summary>
    /// <param name="e">Exit event arguments.</param>
    protected override async void OnExit(ExitEventArgs e)
    {
        Settings.Default.Save();
        _logger!.LogInformation($"'{nameof(OnExit)}' : UI is closed");
        await _host!.StopAsync();
        _host.Dispose();

        // Clean shutdown of Serilog
        Log.CloseAndFlush();

        base.OnExit(e);
    }

    #region Native methods
    /// <summary>
    /// Provides native methods for window manipulation using user32.dll.
    /// </summary>
    private static class NativeMethods
    {
        /// <summary>
        /// Command to restore a minimized window.
        /// </summary>
        public const int SW_RESTORE = 9;

        /// <summary>
        /// Restores the specified window.
        /// </summary>
        /// <param name="hWnd">Handle to the window.</param>
        /// <param name="nCmdShow">Show command.</param>
        /// <returns>True if successful; otherwise, false.</returns>
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        /// <summary>
        /// Brings the specified window to the foreground.
        /// </summary>
        /// <param name="hWnd">Handle to the window.</param>
        /// <returns>True if successful; otherwise, false.</returns>
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
    }
    #endregion

    #region serilog Bootstrap Logger
    static Serilog.ILogger CreateBootstrapLogger()
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.WithThreadId()
            .Enrich.WithProcessId()
            .Enrich.With<UsernameEnrichment>()
            .WriteTo.Console(outputTemplate: "{Timestamp:yyyy.MM.dd-HH:mm:ss.fff} [{Username}] [Thread:{ThreadId}] [ProcessID:{ProcessId}] [{Level:u2}] ({SourceContext}) {Message:lj}{NewLine}{Exception}")
            .CreateBootstrapLogger();

        var bootstrapLogger = Log.Logger.ForContext<App>();
        bootstrapLogger.Information("Bootstrap logger initialized");
        return bootstrapLogger;
    }
    #endregion

    #region Add To Collector
    static void AddToCollectorRequired<T>(IServiceProvider sp, ICollectorCollection collector) where T : notnull
    {
        try
        {
            var service = sp.GetRequiredService<T>();
            collector.AddService(service);
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "Failed to add required service {ServiceType} to collector.", typeof(T).FullName);
            throw; // Optional: if the app should not run without this service
        }
    }
    #endregion
}
