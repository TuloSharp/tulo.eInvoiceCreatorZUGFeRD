using Microsoft.Xaml.Behaviors;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace tulo.ResourcesWpfLib.Utilities;

public class ListViewItemDoubleClick : Behavior<ListView>
{
    private MouseButtonEventHandler _handler;

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.Loaded += OnListViewLoaded;
        AssociatedObject.PreviewMouseDoubleClick += _handler = OnMouseDoubleClick;
        AssociatedObject.ItemContainerGenerator.StatusChanged += OnItemContainerGeneratorStatusChanged;
    }

    private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
        if (e.ChangedButton != MouseButton.Left) return;
        if (e.OriginalSource is not DependencyObject source) return;

        var sourceItem = source is ListViewItem item ? item : source.FindParent<ListViewItem>();
        if (sourceItem == null) return;

        foreach (var binding in AssociatedObject.InputBindings.OfType<MouseBinding>().Where(b => b.MouseAction == MouseAction.LeftDoubleClick))
        {
            var command = binding.Command;
            var parameter = binding.CommandParameter;

            if (command.CanExecute(parameter))
                command.Execute(parameter);
        }
    }

    private void OnListViewLoaded(object sender, RoutedEventArgs e)
    {
        SetFocusOnFirstItem();
    }

    private void OnItemContainerGeneratorStatusChanged(object sender, EventArgs e)
    {
        if (AssociatedObject.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
        {
            SetFocusOnFirstItem();
            AssociatedObject.ItemContainerGenerator.StatusChanged -= OnItemContainerGeneratorStatusChanged;
        }
    }

    private void SetFocusOnFirstItem()
    {
        if (AssociatedObject.Items.Count > 0)
        {
            if (AssociatedObject.SelectedItem == null || AssociatedObject.SelectedIndex == -1)
            {
                AssociatedObject.SelectedIndex = 0;
            }

            AssociatedObject.Dispatcher.BeginInvoke(new Action(() =>
            {
                AssociatedObject.Focus(); 

                if (AssociatedObject.ItemContainerGenerator.ContainerFromIndex(AssociatedObject.SelectedIndex) is ListViewItem selectedItem)
                {
                    selectedItem.Focus();
                }
            }), System.Windows.Threading.DispatcherPriority.Input);
        }
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.Loaded -= OnListViewLoaded;
        AssociatedObject.PreviewMouseDoubleClick -= _handler;
        AssociatedObject.ItemContainerGenerator.StatusChanged -= OnItemContainerGeneratorStatusChanged;
    }
}
