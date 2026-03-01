using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using Microsoft.Xaml.Behaviors;
using System;
using System.Linq;
using System.Runtime.Versioning;
using System.Windows;

namespace tulo.ResourcesWpfLib.Utilities;

[SupportedOSPlatform("windows")]
public class WebView2SettingsBehavior : Behavior<WebView2>
{
    public static readonly DependencyProperty HidePdfToolbarProperty =
        DependencyProperty.Register(nameof(HidePdfToolbar), typeof(bool), typeof(WebView2SettingsBehavior),
            new PropertyMetadata(true));

    public bool HidePdfToolbar
    {
        get => (bool)GetValue(HidePdfToolbarProperty);
        set => SetValue(HidePdfToolbarProperty, value);
    }

    public static readonly DependencyProperty HiddenToolbarItemsProperty =
        DependencyProperty.Register(nameof(HiddenToolbarItems), typeof(string), typeof(WebView2SettingsBehavior),
            new PropertyMetadata(null));

    public string HiddenToolbarItems
    {
        get => (string)GetValue(HiddenToolbarItemsProperty);
        set => SetValue(HiddenToolbarItemsProperty, value);
    }

    public static readonly DependencyProperty DisableContextMenuProperty =
        DependencyProperty.Register(nameof(DisableContextMenu), typeof(bool), typeof(WebView2SettingsBehavior),
            new PropertyMetadata(true));

    public bool DisableContextMenu
    {
        get => (bool)GetValue(DisableContextMenuProperty);
        set => SetValue(DisableContextMenuProperty, value);
    }

    public static readonly DependencyProperty DisableAcceleratorsProperty =
        DependencyProperty.Register(nameof(DisableAccelerators), typeof(bool), typeof(WebView2SettingsBehavior),
            new PropertyMetadata(true));

    public bool DisableAccelerators
    {
        get => (bool)GetValue(DisableAcceleratorsProperty);
        set => SetValue(DisableAcceleratorsProperty, value);
    }

    public static readonly DependencyProperty DisableZoomUIProperty =
        DependencyProperty.Register(nameof(DisableZoomUI), typeof(bool), typeof(WebView2SettingsBehavior),
            new PropertyMetadata(true));

    public bool DisableZoomUI
    {
        get => (bool)GetValue(DisableZoomUIProperty);
        set => SetValue(DisableZoomUIProperty, value);
    }

    protected override async void OnAttached()
    {
        base.OnAttached();
        if (AssociatedObject == null) return;

        if (AssociatedObject.CoreWebView2 == null)
            await AssociatedObject.EnsureCoreWebView2Async();

        var s = AssociatedObject.CoreWebView2.Settings;

        // Context menus, shortcuts, zoom
        s.AreDefaultContextMenusEnabled = !DisableContextMenu;
        s.AreBrowserAcceleratorKeysEnabled = !DisableAccelerators;
        s.IsZoomControlEnabled = !DisableZoomUI;
        s.IsPinchZoomEnabled = !DisableZoomUI;

        // Hide individual PDF toolbar buttons (effective from the next navigation)
        if (!string.IsNullOrWhiteSpace(HiddenToolbarItems))
        {
            s.HiddenPdfToolbarItems = ParseToolbarFlags(HiddenToolbarItems);
        }

        // Hide complete PDF toolbar: via URL fragment “#toolbar=0”
        AssociatedObject.CoreWebView2.NavigationStarting -= CoreWebView2_NavigationStarting;
        if (HidePdfToolbar)
            AssociatedObject.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;
    }

    private void CoreWebView2_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
    {
        if (TryMakePdfToolbarHiddenUrl(e.Uri, out var newUri))
        {
            e.Cancel = true;
            AssociatedObject.CoreWebView2.Navigate(newUri);
        }
    }

    private static bool TryMakePdfToolbarHiddenUrl(string uri, out string newUri)
    {
        newUri = uri ?? string.Empty;
        if (string.IsNullOrWhiteSpace(uri)) return false;

        if (!Uri.TryCreate(uri, UriKind.Absolute, out var u)) return false;
        var isPdf = u.AbsolutePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
        if (!isPdf) return false;

        var hasFragment = !string.IsNullOrEmpty(u.Fragment);
        if (hasFragment && u.Fragment.Contains("toolbar=0", StringComparison.OrdinalIgnoreCase))
            return false;

        var baseUrl = uri.Split('#')[0];
        var fragment = u.Fragment.TrimStart('#');
        fragment = string.IsNullOrEmpty(fragment) ? "toolbar=0" : $"{fragment}&toolbar=0";
        newUri = $"{baseUrl}#{fragment}";
        return true;
    }

    private static CoreWebView2PdfToolbarItems ParseToolbarFlags(string csv)
    {
        return csv.Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries)
                  .Select(p => p.Trim())
                  .Select(Map)
                  .Aggregate(CoreWebView2PdfToolbarItems.None, (acc, cur) => acc | cur);
    }

    private static CoreWebView2PdfToolbarItems Map(string name) => name.ToLowerInvariant() switch
    {
        "save" => CoreWebView2PdfToolbarItems.Save,
        "saveas" => CoreWebView2PdfToolbarItems.SaveAs,
        "print" => CoreWebView2PdfToolbarItems.Print,
        "zoomin" => CoreWebView2PdfToolbarItems.ZoomIn,
        "zoomout" => CoreWebView2PdfToolbarItems.ZoomOut,
        "fitpage" => CoreWebView2PdfToolbarItems.FitPage,
        "pagelayout" => CoreWebView2PdfToolbarItems.PageLayout,
        "rotate" => CoreWebView2PdfToolbarItems.Rotate,
        _ => CoreWebView2PdfToolbarItems.None
    };
}