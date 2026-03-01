namespace tulo.ResourcesWpfLib.Viewmodels;

public interface IUiControlPropsViewModel
{
    /// <summary>
    /// This property control, if the alt key is pressed
    /// </summary>
    bool IsAltShortcutKeyPressed { get; set; }
    /// <summary>
    /// This property control, if the alt key is alredy pressed
    /// </summary>
    bool IsAltShortcutKeyAlreadyPressed { get; set; }
    /// <summary>
    /// This property show a message if the dataset is duplicated
    /// </summary>
    //bool IsDuplicate { get; set; }
    /// <summary>
    /// This property show the request status message
    /// </summary>
    //string StatusMessage { set; }
    /// <summary>
    /// set the current view model name to search the MapId for help text
    /// </summary>
    //string CurrentViewModelName { get; }
}
