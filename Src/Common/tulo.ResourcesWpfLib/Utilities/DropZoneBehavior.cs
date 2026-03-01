using Microsoft.Xaml.Behaviors;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace tulo.ResourcesWpfLib.Utilities;

public class DropZoneBehavior : Behavior<UIElement>
{
    public static readonly DependencyProperty DropCommandProperty = DependencyProperty.Register(nameof(DropCommand), typeof(ICommand), typeof(DropZoneBehavior),
            new PropertyMetadata(null));

    public ICommand DropCommand
    {
        get => (ICommand)GetValue(DropCommandProperty);
        set => SetValue(DropCommandProperty, value);
    }

    public static readonly DependencyProperty AllowedExtensionsProperty = DependencyProperty.Register(nameof(AllowedExtensions), typeof(string), typeof(DropZoneBehavior),
            new PropertyMetadata(".xml;.pdf"));

    public string AllowedExtensions
    {
        get => (string)GetValue(AllowedExtensionsProperty);
        set => SetValue(AllowedExtensionsProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        if (AssociatedObject == null) return;

        if (AssociatedObject is UIElement el)
            el.AllowDrop = true;

        AssociatedObject.PreviewDragOver += OnPreviewDragOver;
        AssociatedObject.PreviewDrop += OnPreviewDrop;
    }

    protected override void OnDetaching()
    {
        if (AssociatedObject != null)
        {
            AssociatedObject.PreviewDragOver -= OnPreviewDragOver;
            AssociatedObject.PreviewDrop -= OnPreviewDrop;
        }
        base.OnDetaching();
    }

    private void OnPreviewDragOver(object sender, DragEventArgs e)
    {
        var path = GetFirstAllowedPath(e);
        e.Effects = path != null ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    private void OnPreviewDrop(object sender, DragEventArgs e)
    {
        var path = GetFirstAllowedPath(e);
        if (path == null)
        {
            e.Handled = true;
            return;
        }

        var cmd = DropCommand;
        if (cmd?.CanExecute(path) == true)
            cmd.Execute(path);

        e.Handled = true;
    }

    private string GetFirstAllowedPath(DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return null;

        var files = e.Data.GetData(DataFormats.FileDrop) as string[];
        if (files == null || files.Length == 0) return null;

        var allowed = (AllowedExtensions ?? string.Empty)
            .Split(new[] { ';', ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim().ToLowerInvariant())
            .ToHashSet();

        foreach (var f in files)
        {
            if (string.IsNullOrWhiteSpace(f) || !File.Exists(f)) continue;
            var ext = Path.GetExtension(f).ToLowerInvariant();
            if (allowed.Contains(ext)) return f;
        }

        return null;
    }
}
