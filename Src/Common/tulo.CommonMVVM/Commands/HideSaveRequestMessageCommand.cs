using tulo.CommonMVVM.Collector;
using tulo.CommonMVVM.GlobalProperties;

namespace tulo.CommonMVVM.Commands;
public class HideSaveRequestMessageCommand(ICollectorCollection collectorCollection) : BaseCommand
{
    private readonly IGlobalPropsUiManage _globalPropsUiManage = collectorCollection.GetService<IGlobalPropsUiManage>();
   
    public override void Execute(object parameter)
    {
        _globalPropsUiManage.IsSaveRequestMessageVisible = false;
    }
}
