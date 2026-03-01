using tulo.CommonMVVM.Collector;
using tulo.CommonMVVM.GlobalProperties;
using tulo.CommonMVVM.Stores;
using tulo.CommonMVVM.ViewModels;
using tulo.ResourcesWpfLib.StatusMessages;

namespace tulo.CommonMVVM.Commands;

public class CloseModalStackCommand(ICollectorCollection collectorCollection, Type viewModelTypeToClose, bool confirmUnsavedChangesOnClose = false) : BaseCommand
{
    private readonly IModalStackNavigationStore _modalStackNavigationStore =  collectorCollection.GetService<IModalStackNavigationStore>();
    private readonly IGlobalPropsUiManage _globalPropsUiManage = collectorCollection.GetService<IGlobalPropsUiManage>();

    private readonly Type _defaultViewModelTypeToClose = viewModelTypeToClose;
    private readonly bool _defaultConfirmUnsavedChangesOnClose = confirmUnsavedChangesOnClose;

    public override void Execute(object parameter)
    {
        var (typeToClose, shouldConfirmUnsavedChanges) = ResolveParameters(parameter);

        if (typeToClose == null)
            return;

        // Find the topmost (most recently added) instance of this type in the modals collection
        BaseViewModel target = _modalStackNavigationStore.Modals.LastOrDefault(modal => modal?.GetType() == typeToClose);

        if (target == null)
            return;

        // Only apply the unsaved-changes confirmation logic if enabled.
        // Step 1: HasUnsavedChanges=true + Message not yet visible → show message, abort close
        // Step 2: Message already visible (user reacted) → hide message, proceed to close
        if (shouldConfirmUnsavedChanges)
        {
            // Clear the status message (hides it in the UI) before attempting to close
            // Only does something if the target ViewModel supports status messages and a message is currently set
            if (target is IHasStatusMessage statusMsgViewModel && statusMsgViewModel.StatusMessageViewModel is not null && !string.IsNullOrEmpty(statusMsgViewModel.StatusMessageViewModel.Message))
            {
                statusMsgViewModel.StatusMessageViewModel.Message = string.Empty;
            }

            // If there are unsaved changes and the save request UI is not yet shown,
            // enable the save request UI and prevent closing (first click behavior)
            if (_globalPropsUiManage.HasUnsavedChanges && !_globalPropsUiManage.IsSaveRequestMessageVisible)
            {
                // Do not close the modal on the first click
                _globalPropsUiManage.IsSaveRequestMessageVisible = true;
                return;
            }

            // User confirmed (or no unsaved changes) -> reset the save request UI flag
            _globalPropsUiManage.IsSaveRequestMessageVisible = false;
        }

        _modalStackNavigationStore.Close(target);
    }

    private (Type typeToClose, bool shouldConfirm) ResolveParameters(object parameter)
    {
        Type typeToClose = _defaultViewModelTypeToClose;
        bool shouldConfirm = _defaultConfirmUnsavedChangesOnClose;

        switch (parameter)
        {
            case object[] { Length: > 0 } paramArray:
                if (paramArray[0] is Type typeFromArray)
                    typeToClose = typeFromArray;
                else if (paramArray[0] != null)
                    typeToClose = paramArray[0].GetType();

                if (paramArray.Length > 1 && paramArray[1] is bool confirmFromArray)
                    shouldConfirm = confirmFromArray;
                break;

            case Type typeFromParameter:
                typeToClose = typeFromParameter;
                break;

            case bool confirmFromParameter:
                shouldConfirm = confirmFromParameter;
                break;
        }

        return (typeToClose, shouldConfirm);
    }
}
