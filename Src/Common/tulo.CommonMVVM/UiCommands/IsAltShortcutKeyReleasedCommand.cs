using System.Windows.Input;
using tulo.CommonMVVM.Collector;
using tulo.CommonMVVM.Commands;
using tulo.CommonMVVM.GlobalProperties;

namespace tulo.CommonMVVM.UiCommands;

public class IsAltShortcutKeyReleasedCommand(ICollectorCollection collectorCollection) : BaseCommand
{
    private readonly IGlobalPropsUiManage _globalPropsUiManage = collectorCollection.GetService<IGlobalPropsUiManage>();

    public override void Execute(object parameter)
    {
        // Expecting a KeyEventArgs coming from a WPF key event binding.
        if (parameter is not KeyEventArgs e)
        {
            _globalPropsUiManage.IsAltShortcutKeyPressed = false;
            return;
        }

        // Use e.SystemKey when e.Key == Key.System (Alt/Windows key combos).
        // SystemKey cannot be created programmatically in some scenarios, so normalize it here.
        var key = e.Key == Key.System ? e.SystemKey : e.Key;

        // ONLY when ALT is released -> hide + debounce reset
        if (key == Key.LeftAlt || key == Key.RightAlt)
        {
            _globalPropsUiManage.IsAltShortcutKeyPressed = false;
            return;
        }

        // Safety: If for some reason Alt is no longer pressed (focus change etc.)
        // then also reset. (Important against "Alt hangs".)
        if (!Keyboard.IsKeyDown(Key.LeftAlt) && !Keyboard.IsKeyDown(Key.RightAlt))
        {
            _globalPropsUiManage.IsAltShortcutKeyPressed = false;
        }

        // IMPORTANT: For KeyUp of O/R/Z/... while Alt is still pressed:
        // -> DO NOT set to false (otherwise it flickers).
    }
}
