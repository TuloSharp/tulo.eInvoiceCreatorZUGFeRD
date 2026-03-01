using tulo.CommonMVVM.Commands;
using tulo.CommonMVVM.Stores;

namespace tulo.ResourcesWpfLib.Commands;

public class CloseCommonMessageCommand(IModalNavigationStore modalNavigationStore) : BaseCommand
{
    private readonly IModalNavigationStore _modalNavigationStore = modalNavigationStore;

    public override void Execute(object parameter)
    {
        _modalNavigationStore.Close();
    }
}
