using tulo.CommonMVVM.Commands;
using tulo.CommonMVVM.Stores;

namespace tulo.CommonMVVM.UiCommands;

public class CloseCommonMessageCommand(IModalNavigationStore modalNavigationStore) : BaseCommand
{
    private readonly IModalNavigationStore _modalNavigationStore = modalNavigationStore;

    public override void Execute(object parameter)
    {
        _modalNavigationStore.Close();
    }
}
