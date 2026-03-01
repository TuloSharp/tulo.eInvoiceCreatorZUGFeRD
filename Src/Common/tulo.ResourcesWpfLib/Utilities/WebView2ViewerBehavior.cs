using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using Microsoft.Xaml.Behaviors;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace tulo.ResourcesWpfLib.Utilities;

[SupportedOSPlatform("windows")]
public class WebView2ViewerBehavior : Behavior<WebView2>
{
    // -------------------- HTML navigate (your existing viewer binding) --------------------
    public static readonly DependencyProperty HtmlContentProperty =
        DependencyProperty.Register(nameof(HtmlContent), typeof(string), typeof(WebView2ViewerBehavior),
            new PropertyMetadata(null, OnHtmlContentChanged));

    public string HtmlContent
    {
        get => (string)GetValue(HtmlContentProperty);
        set => SetValue(HtmlContentProperty, value);
    }

    private static async void OnHtmlContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not WebView2ViewerBehavior b) return;

        var wv = b.AssociatedObject;
        if (wv == null) return;

        int myVersion = Interlocked.Increment(ref b._htmlUpdateVersion);

        await b._htmlNavGate.WaitAsync();
        try
        {
            if (myVersion != b._htmlUpdateVersion) return;

            var op = wv.Dispatcher.InvokeAsync(() => b.OnHtmlNavigateUiAsync(e.NewValue));
            await op.Task;
        }
        finally
        {
            b._htmlNavGate.Release();
        }
    }

    private async Task OnHtmlNavigateUiAsync(object newValue)
    {
        if (AssociatedObject == null) return;

        await InitializeWebViewSafeAsync();

        if (AssociatedObject?.CoreWebView2 == null)
            return;

        if (newValue is string html)
        {
            AssociatedObject.Visibility = Visibility.Visible;
            AssociatedObject.NavigateToString(html);
        }
    }

    private async Task InitializeWebViewSafeAsync()
    {
        var wv = AssociatedObject;
        if (wv == null) return;
        if (wv.CoreWebView2 != null) return;

        try
        {
            var ver = Microsoft.Web.WebView2.Core.CoreWebView2Environment.GetAvailableBrowserVersionString();
            Debug.WriteLine("WebView2 Runtime Version: " + ver);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("WebView2 Runtime Version query failed: " + ex);
        }

        await _initGate.WaitAsync();
        try
        {
            wv = AssociatedObject;
            if (wv == null) return;
            if (wv.CoreWebView2 != null) return;

            if (!wv.IsLoaded)
            {
                var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                RoutedEventHandler loaded = null;
                loaded = (_, __) => { wv.Loaded -= loaded; tcs.TrySetResult(true); };
                wv.Loaded += loaded;
                await tcs.Task;

                wv = AssociatedObject;
                if (wv == null) return;
                if (wv.CoreWebView2 != null) return;
            }

            try
            {
                // 1) Wenn Prozess-Env gesetzt ist, benutze die
                var envFolder = Environment.GetEnvironmentVariable("WEBVIEW2_USER_DATA_FOLDER");

                // 2) sonst eigener Prozess-Ordner
                string userDataFolder;
                if (!string.IsNullOrWhiteSpace(envFolder))
                {
                    userDataFolder = envFolder.Trim();
                }
                else
                {
                    var appName = Assembly.GetEntryAssembly()?.GetName().Name ?? "App";
                    userDataFolder = Path.Combine(Path.GetTempPath(), appName, $"pid-{Environment.ProcessId}");
                }

                Directory.CreateDirectory(userDataFolder);

                wv.CreationProperties ??= new CoreWebView2CreationProperties();
                wv.CreationProperties.UserDataFolder = userDataFolder;

                Debug.WriteLine("WebView2 UserDataFolder: " + userDataFolder);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Setting WebView2 UserDataFolder failed: " + ex);
            }

            if (!wv.Dispatcher.CheckAccess())
            {
                var op = wv.Dispatcher.InvokeAsync(() => wv.EnsureCoreWebView2Async());
                var inner = await op.Task; 
                await inner;               
            }
            else
            {
                await wv.EnsureCoreWebView2Async();
            }
        }
        catch (COMException ex) when ((uint)ex.HResult == 0x80010108)
        {
            PrintError = "WebView2 was disconnected during initialization (RPC_E_DISCONNECTED).";
            PrintState = PrintJobState.Failed;
        }
        finally
        {
            _initGate.Release();
        }
    }


    // -------------------- Silent print / export trigger --------------------
    public static readonly DependencyProperty SilentPrintPdfBytesProperty =
        DependencyProperty.Register(nameof(SilentPrintPdfBytes), typeof(byte[]), typeof(WebView2ViewerBehavior),
            new PropertyMetadata(null, OnSilentPrintPdfBytesChanged));

    public byte[] SilentPrintPdfBytes
    {
        get => (byte[])GetValue(SilentPrintPdfBytesProperty);
        set => SetValue(SilentPrintPdfBytesProperty, value);
    }

    // Default FALSE - never Hidden/Collapsed (breaks rendering/printing)
    public static readonly DependencyProperty HideControlDuringSilentPrintProperty =
        DependencyProperty.Register(nameof(HideControlDuringSilentPrint), typeof(bool),
            typeof(WebView2ViewerBehavior),
            new PropertyMetadata(false));

    public bool HideControlDuringSilentPrint
    {
        get => (bool)GetValue(HideControlDuringSilentPrintProperty);
        set => SetValue(HideControlDuringSilentPrintProperty, value);
    }

    // Compatibility with your XAML (even if not used in base64-mode)
    public static readonly DependencyProperty VirtualPdfPathProperty =
        DependencyProperty.Register(nameof(VirtualPdfPath), typeof(string), typeof(WebView2ViewerBehavior),
            new PropertyMetadata("document.pdf"));

    public string VirtualPdfPath
    {
        get => (string)GetValue(VirtualPdfPathProperty);
        set => SetValue(VirtualPdfPathProperty, value);
    }

    // Optional: if you really want it "hidden", move offscreen but KEEP renderable.
    private Thickness _savedMargin;
    private HorizontalAlignment _savedHAlign;
    private VerticalAlignment _savedVAlign;
    private double _savedWidth, _savedHeight;
    private bool _savedSizeSet;

    // -------------------- Print options (bindable) --------------------
    public static readonly DependencyProperty PrinterNameProperty =
        DependencyProperty.Register(nameof(PrinterName), typeof(string), typeof(WebView2ViewerBehavior),
            new PropertyMetadata(string.Empty));
    public string PrinterName { get => (string)GetValue(PrinterNameProperty); set => SetValue(PrinterNameProperty, value); }

    public static readonly DependencyProperty CopiesProperty =
        DependencyProperty.Register(nameof(Copies), typeof(int), typeof(WebView2ViewerBehavior),
            new PropertyMetadata(1));
    public int Copies { get => (int)GetValue(CopiesProperty); set => SetValue(CopiesProperty, value); }

    public static readonly DependencyProperty CollateProperty =
        DependencyProperty.Register(nameof(Collate), typeof(bool), typeof(WebView2ViewerBehavior),
            new PropertyMetadata(true));
    public bool Collate { get => (bool)GetValue(CollateProperty); set => SetValue(CollateProperty, value); }

    public static readonly DependencyProperty DuplexProperty =
        DependencyProperty.Register(nameof(Duplex), typeof(CoreWebView2PrintDuplex), typeof(WebView2ViewerBehavior),
            new PropertyMetadata(CoreWebView2PrintDuplex.Default));
    public CoreWebView2PrintDuplex Duplex { get => (CoreWebView2PrintDuplex)GetValue(DuplexProperty); set => SetValue(DuplexProperty, value); }

    public static readonly DependencyProperty LandscapeProperty =
        DependencyProperty.Register(nameof(Landscape), typeof(bool), typeof(WebView2ViewerBehavior),
            new PropertyMetadata(false));
    public bool Landscape { get => (bool)GetValue(LandscapeProperty); set => SetValue(LandscapeProperty, value); }

    public static readonly DependencyProperty FromPageProperty =
        DependencyProperty.Register(nameof(FromPage), typeof(int?), typeof(WebView2ViewerBehavior),
            new PropertyMetadata(null));
    public int? FromPage { get => (int?)GetValue(FromPageProperty); set => SetValue(FromPageProperty, value); }

    public static readonly DependencyProperty ToPageProperty =
        DependencyProperty.Register(nameof(ToPage), typeof(int?), typeof(WebView2ViewerBehavior),
            new PropertyMetadata(null));
    public int? ToPage { get => (int?)GetValue(ToPageProperty); set => SetValue(ToPageProperty, value); }

    public static readonly DependencyProperty MarginTopInchesProperty =
        DependencyProperty.Register(nameof(MarginTopInches), typeof(double), typeof(WebView2ViewerBehavior),
            new PropertyMetadata(0d));
    public double MarginTopInches { get => (double)GetValue(MarginTopInchesProperty); set => SetValue(MarginTopInchesProperty, value); }

    public static readonly DependencyProperty MarginBottomInchesProperty =
        DependencyProperty.Register(nameof(MarginBottomInches), typeof(double), typeof(WebView2ViewerBehavior),
            new PropertyMetadata(0d));
    public double MarginBottomInches { get => (double)GetValue(MarginBottomInchesProperty); set => SetValue(MarginBottomInchesProperty, value); }

    public static readonly DependencyProperty MarginLeftInchesProperty =
        DependencyProperty.Register(nameof(MarginLeftInches), typeof(double), typeof(WebView2ViewerBehavior),
            new PropertyMetadata(0d));
    public double MarginLeftInches { get => (double)GetValue(MarginLeftInchesProperty); set => SetValue(MarginLeftInchesProperty, value); }

    public static readonly DependencyProperty MarginRightInchesProperty =
        DependencyProperty.Register(nameof(MarginRightInches), typeof(double), typeof(WebView2ViewerBehavior),
            new PropertyMetadata(0d));
    public double MarginRightInches { get => (double)GetValue(MarginRightInchesProperty); set => SetValue(MarginRightInchesProperty, value); }

    // Export path (no printer): writes bytes 1:1
    public static readonly DependencyProperty ExportPdfPathProperty =
        DependencyProperty.Register(nameof(ExportPdfPath), typeof(string), typeof(WebView2ViewerBehavior),
            new PropertyMetadata(string.Empty));
    public string ExportPdfPath
    {
        get => (string)GetValue(ExportPdfPathProperty);
        set => SetValue(ExportPdfPathProperty, value);
    }

    /// <summary>
    /// Base delay in ms after iframe.onload before PrintAsync. Extra delay is added based on PDF size.
    /// </summary>
    public static readonly DependencyProperty BaseRenderDelayMsProperty =
        DependencyProperty.Register(nameof(BaseRenderDelayMs), typeof(int), typeof(WebView2ViewerBehavior),
            new PropertyMetadata(500));
    public int BaseRenderDelayMs
    {
        get => (int)GetValue(BaseRenderDelayMsProperty);
        set => SetValue(BaseRenderDelayMsProperty, value);
    }

    public enum PrintJobState { Idle, Starting, Navigating, Rendering, Printing, Succeeded, Failed }

    public static readonly DependencyProperty PrintStateProperty =
        DependencyProperty.Register(nameof(PrintState), typeof(PrintJobState),
            typeof(WebView2ViewerBehavior), new PropertyMetadata(PrintJobState.Idle));
    public PrintJobState PrintState
    {
        get => (PrintJobState)GetValue(PrintStateProperty);
        set => SetValue(PrintStateProperty, value);
    }

    public static readonly DependencyProperty PrintErrorProperty =
        DependencyProperty.Register(nameof(PrintError), typeof(string),
            typeof(WebView2ViewerBehavior), new PropertyMetadata(string.Empty));
    public string PrintError
    {
        get => (string)GetValue(PrintErrorProperty);
        set => SetValue(PrintErrorProperty, value);
    }

    private static async void OnSilentPrintPdfBytesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var b = (WebView2ViewerBehavior)d;
        if (b.AssociatedObject is null) return;

        var bytes = e.NewValue as byte[];
        if (bytes == null || bytes.Length == 0) return;

        await b.SilentPrintInternalAsync(bytes);
    }

    // -------------------- Internals --------------------
    private EventHandler<CoreWebView2WebMessageReceivedEventArgs> _msgHandler;
    private int _htmlUpdateVersion = 0;
    private readonly SemaphoreSlim _htmlNavGate = new(1, 1);
    private readonly SemaphoreSlim _initGate = new(1, 1);

    private async Task SilentPrintInternalAsync(byte[] pdfBytes)
    {
        await InitializeWebViewSafeAsync();
        if (AssociatedObject == null || AssociatedObject.CoreWebView2 == null) return;

        try
        {
            PrintError = string.Empty;
            PrintState = PrintJobState.Starting;

            EnsureVisibleAndSized();

            if (HideControlDuringSilentPrint)
                MoveOffscreen();

            // 1) Export mode (no printer)
            var exportPath = (ExportPdfPath ?? string.Empty).Trim();
            if (!string.IsNullOrEmpty(exportPath))
            {
                var dir = Path.GetDirectoryName(exportPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                File.WriteAllBytes(exportPath, pdfBytes);
                PrintState = PrintJobState.Succeeded;
                return;
            }

            // 2) Print mode (HTML wrapper, no viewer-controls)
            var core = AssociatedObject.CoreWebView2;

            var html = CreateHtmlForPrint(pdfBytes);

            var navTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            void NavDone(object s, CoreWebView2NavigationCompletedEventArgs e)
            {
                core.NavigationCompleted -= NavDone;
                navTcs.TrySetResult(true);
            }
            core.NavigationCompleted += NavDone;

            var iframeTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            if (_msgHandler != null) core.WebMessageReceived -= _msgHandler;
            _msgHandler = (s, e) =>
            {
                try
                {
                    var msg = e.TryGetWebMessageAsString();
                    if (msg == "IFRAME_LOADED")
                        iframeTcs.TrySetResult(true);
                }
                catch { }
            };
            core.WebMessageReceived += _msgHandler;

            PrintState = PrintJobState.Navigating;
            AssociatedObject.NavigateToString(html);

            //await navTcs.Task;
            //await iframeTcs.Task;

            var navTask = navTcs.Task;
            var iframeTask = iframeTcs.Task;

            var timeoutTask = Task.Delay(15000);

            var all = Task.WhenAll(navTask, iframeTask);
            var finished = await Task.WhenAny(all, timeoutTask);

            if (finished == timeoutTask)
            {
                PrintError = "PDF did not load in time (IFRAME_LOADED Timeout).";
                PrintState = PrintJobState.Failed;
                return;
            }
            await all;

            PrintState = PrintJobState.Rendering;

            await AssociatedObject.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);

            int extraBySize =
                pdfBytes.Length > 10_000_000 ? 2500 :
                pdfBytes.Length > 3_000_000 ? 1500 :
                800;

            int totalDelay = Math.Max(0, BaseRenderDelayMs) + extraBySize;
            await Task.Delay(totalDelay);

            await AssociatedObject.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);

            // 3) Print
            PrintState = PrintJobState.Printing;
            var ps = BuildSettings(core);
            await core.PrintAsync(ps);

            PrintState = PrintJobState.Succeeded;
        }
        catch (Exception ex)
        {
            PrintError = ex.Message;
            PrintState = PrintJobState.Failed;
            Debug.WriteLine(ex);
        }
        finally
        {
            try
            {
                var core = AssociatedObject?.CoreWebView2;
                if (core != null && _msgHandler != null)
                    core.WebMessageReceived -= _msgHandler;
            }
            catch { }

            _msgHandler = null;

            if (HideControlDuringSilentPrint)
                RestoreFromOffscreen();
        }
    }

    private void EnsureVisibleAndSized()
    {
        if (AssociatedObject == null) return;

        AssociatedObject.Visibility = Visibility.Visible;

        if (AssociatedObject.ActualWidth < 10 || AssociatedObject.ActualHeight < 10)
        {
            _savedWidth = AssociatedObject.Width;
            _savedHeight = AssociatedObject.Height;
            _savedSizeSet = true;

            if (double.IsNaN(AssociatedObject.Width) || AssociatedObject.Width < 10) AssociatedObject.Width = 900;
            if (double.IsNaN(AssociatedObject.Height) || AssociatedObject.Height < 10) AssociatedObject.Height = 1200;
        }

        AssociatedObject.UpdateLayout();
    }

    private void MoveOffscreen()
    {
        if (AssociatedObject == null) return;

        _savedMargin = AssociatedObject.Margin;
        _savedHAlign = AssociatedObject.HorizontalAlignment;
        _savedVAlign = AssociatedObject.VerticalAlignment;

        AssociatedObject.HorizontalAlignment = HorizontalAlignment.Left;
        AssociatedObject.VerticalAlignment = VerticalAlignment.Top;
        AssociatedObject.Margin = new Thickness(-20000, -20000, 0, 0);

        AssociatedObject.UpdateLayout();
    }

    private void RestoreFromOffscreen()
    {
        if (AssociatedObject == null) return;

        AssociatedObject.Margin = _savedMargin;
        AssociatedObject.HorizontalAlignment = _savedHAlign;
        AssociatedObject.VerticalAlignment = _savedVAlign;

        if (_savedSizeSet)
        {
            AssociatedObject.Width = _savedWidth;
            AssociatedObject.Height = _savedHeight;
            _savedSizeSet = false;
        }

        AssociatedObject.UpdateLayout();
    }

    // *** THIS is the method you wanted to keep (your working HTML for printing, with toolbar=0) ***
    private static string CreateHtmlForPrint(byte[] pdfBytes)
    {
        if (pdfBytes == null || pdfBytes.Length == 0)
            return "<html><body><h1>PDF content is empty.</h1></body></html>";

        string base64Pdf = Convert.ToBase64String(pdfBytes);

        return $@"
<!DOCTYPE html>
<html>
<head>
  <meta charset='utf-8'/>
  <title>Print</title>
  <style>
    @page {{
      size: auto;
      margin: 0;
    }}
    html, body {{
      margin: 0;
      padding: 0;
      width: 100%;
      height: 100%;
      overflow: hidden;
      background: #fff;
    }}
    iframe {{
      position: fixed;
      inset: 0;
      width: 100%;
      height: 100%;
      border: 0;
    }}
    @media print {{
      html, body {{ overflow: hidden; }}
    }}
  </style>
</head>
<body>
  <iframe id='pdfFrame' src='data:application/pdf;base64,{base64Pdf}#toolbar=0&navpanes=0&scrollbar=0'></iframe>
  <script>
    (function() {{
      var f = document.getElementById('pdfFrame');
      f.onload = function() {{
        try {{
          if (window.chrome && window.chrome.webview) {{
            window.chrome.webview.postMessage('IFRAME_LOADED');
          }}
        }} catch(e) {{}}
      }};
      setTimeout(function() {{
        try {{
          if (window.chrome && window.chrome.webview) {{
            window.chrome.webview.postMessage('IFRAME_LOADED');
          }}
        }} catch(e) {{}}
      }}, 1500);
    }})();
  </script>
</body>
</html>";
    }

    private CoreWebView2PrintSettings BuildSettings(CoreWebView2 core)
    {
        var ps = core.Environment.CreatePrintSettings();

        var name = (PrinterName ?? string.Empty).Trim();
        if (!string.IsNullOrEmpty(name))
            ps.PrinterName = name;
        // otherwise do not set => default printer

        ps.Copies = Math.Max(1, Copies);
        ps.Collation = Collate ? CoreWebView2PrintCollation.Collated : CoreWebView2PrintCollation.Uncollated;
        ps.Duplex = Duplex;
        ps.Orientation = Landscape ? CoreWebView2PrintOrientation.Landscape : CoreWebView2PrintOrientation.Portrait;

        if (FromPage.HasValue || ToPage.HasValue)
        {
            if (FromPage.HasValue && ToPage.HasValue) ps.PageRanges = $"{FromPage}-{ToPage}";
            else if (FromPage.HasValue) ps.PageRanges = $"{FromPage}-";
            else ps.PageRanges = $"-{ToPage}";
        }

        ps.MarginTop = MarginTopInches;
        ps.MarginBottom = MarginBottomInches;
        ps.MarginLeft = MarginLeftInches;
        ps.MarginRight = MarginRightInches;

        ps.ShouldPrintHeaderAndFooter = false;
        ps.ShouldPrintBackgrounds = true;

        return ps;
    }

    protected override void OnDetaching()
    {
        try
        {
            var core = AssociatedObject?.CoreWebView2;
            if (core != null && _msgHandler != null)
                core.WebMessageReceived -= _msgHandler;
        }
        catch { }
        finally
        {
            _msgHandler = null;
        }

        base.OnDetaching();
    }
}