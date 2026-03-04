using System.Windows.Input;
using tulo.CommonMVVM.Collector;
using tulo.CommonMVVM.Commands;
using tulo.CommonMVVM.GlobalProperties;

namespace tulo.CommonMVVM.UiCommands;

public class IsAltShortcutKeyPressedCommand(ICollectorCollection collectorCollection) : BaseCommand
{
    private readonly IGlobalPropsUiManage _globalPropsUiManage = collectorCollection.GetService<IGlobalPropsUiManage>();

    public override void Execute(object parameter)
    {
        // Expecting a KeyEventArgs coming from a WPF key event binding.
        if (parameter is not KeyEventArgs e)
            return;

        // Use e.SystemKey when e.Key == Key.System (Alt/Windows key combos).
        // SystemKey cannot be created programmatically in some scenarios, so normalize it here.
        var key = e.Key == Key.System ? e.SystemKey : e.Key;

        // Only react to LeftAlt, and only once per physical key press (debounce).
        // We don't use e.IsRepeat because it introduces a delay before the same key
        // can be detected again.
        var isAltKey = key == Key.LeftAlt || key == Key.RightAlt;
        if (!isAltKey)
            return;

        // Debounce: nur einmal pro physischem Alt-Drücken
        if (_globalPropsUiManage.IsAltShortcutKeyPressed)
            return;

        // IMPORTANT: DO NOT toggle, but set to true
        _globalPropsUiManage.IsAltShortcutKeyPressed = true;

        // Optional: mark event as handled if you want to stop further processing.
        // e.Handled = true;
    }
}
