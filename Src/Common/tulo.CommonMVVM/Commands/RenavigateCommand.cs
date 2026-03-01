using tulo.CommonMVVM.Services;

namespace tulo.CommonMVVM.Commands;

public class RenavigateCommand : BaseCommand
{
    private readonly IRenavigationService _renavigationService;

    public RenavigateCommand(IRenavigationService renavigationService)
    {
        _renavigationService = renavigationService;
    }

    public override void Execute(object parameter)
    {
        _renavigationService.Renavigate();
    }
}