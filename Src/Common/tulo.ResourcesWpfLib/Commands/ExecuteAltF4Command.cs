using System.Windows;

namespace tulo.ResourcesWpfLib.Commands;

public class ExecuteAltF4Command : BaseCommand
{
    private const int WindowIndex = 0;
    private const int IsMainWindowIndex = 1;
    private const int CloseMainWindowCommandIndex = 5;

    public override void Execute(object parameter)
    {
        if (parameter is not object[] args) return;
        if (args.Length <= CloseMainWindowCommandIndex) return;

        if (args[WindowIndex] is not Window window) return;
        if (args[IsMainWindowIndex] is not bool isMainWindow) return;
        if (args[CloseMainWindowCommandIndex] is not BaseCommand closeMainWindowCommand) return;

        if (!isMainWindow) return;

        // CommandArgs for CloseMainWindowCommand
        var closeWindowArgs = new object[] { window, isMainWindow };
        closeMainWindowCommand.Execute(closeWindowArgs);
    }
}
