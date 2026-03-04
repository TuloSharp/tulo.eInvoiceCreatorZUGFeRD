using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using tulo.CommonMVVM.Collector;
using tulo.CommonMVVM.GlobalProperties;

namespace tulo.CommonMVVM.Commands;
public class HideSaveRequestMessageCommand(ICollectorCollection collectorCollection) : BaseCommand
{
    private readonly IGlobalPropsUiManage _globalPropsUiManage = collectorCollection.GetService<IGlobalPropsUiManage>();
   
    public override void Execute(object parameter)
    {
        _globalPropsUiManage.IsSaveRequestMessageVisible = false;

        // 1) Determine a focus target:
        // - preferred: the UI root passed via CommandParameter (modal root)
        // - fallback: the currently focused element
        var target = parameter as FrameworkElement
                     ?? Keyboard.FocusedElement as FrameworkElement;

        if (target is null)
            return;

        // 2) Restore focus AFTER the UI has updated (overlay removed), using the UI dispatcher
        try
        {
            target.Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
            {
                try
                {
                    // Ensure the element can receive focus
                    if (!target.Focusable)
                        target.Focusable = true;

                    // Set keyboard focus back to the modal area
                    target.Focus();

                    // Optionally move focus to the first focusable child within the modal
                    target.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
                }
                catch
                {
                    // Ignore focus-related exceptions (can happen if the visual tree is changing)
                }
            }));
        }
        catch
        {
            // Ignore dispatcher-related exceptions (rare, but can happen in edge cases)
        }
    }
}
