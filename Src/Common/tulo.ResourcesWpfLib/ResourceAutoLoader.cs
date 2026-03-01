using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace tulo.ResourcesWpfLib;
public static class ResourceAutoLoader
{
    [SuppressMessage("Usage", "CA2255:Module initializers should not be used in libraries")]
    [ModuleInitializer]
    internal static void Init()
    {
        if (Application.Current == null) return;

        var uri = new Uri(
            "pack://application:,,,/tulo.ResourcesWpfLib;component/Resources/AllResources.xaml",
            UriKind.Absolute);

        if (Application.Current.Resources.MergedDictionaries.Any(d => d.Source == uri))
            return;

        Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = uri });
    }
}
