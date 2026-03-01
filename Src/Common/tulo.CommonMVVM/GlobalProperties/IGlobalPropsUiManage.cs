namespace tulo.CommonMVVM.GlobalProperties;
public interface IGlobalPropsUiManage
{
    /// <summary>
    /// Gets or sets a general-purpose UI message (e.g., status/info/error text).
    /// </summary>
    string Message { get; set; }

    /// <summary>
    /// Raised whenever <see cref="Message"/> changes.
    /// </summary>
    event Action MessageChanged;


    /// <summary>
    /// Gets or sets whether the UI is currently processing (typically used to show/hide a spinner or block input).
    /// </summary>
    bool IsProcessing { get; set; }

    /// <summary>
    /// Raised whenever <see cref="IsProcessing"/> changes.
    /// </summary>
    event Action IsProcessingChanged;


    /// <summary>
    /// Gets or sets whether the UI is in "display view mode" (read-only / view-only mode).
    /// </summary>
    bool IsDisplayViewMode { get; set; }

    /// <summary>
    /// Raised whenever <see cref="IsDisplayViewMode"/> changes.
    /// </summary>
    event Action IsDisplayViewModeChanged;


    /// <summary>
    /// Gets or sets whether the ALT key is currently pressed.
    /// </summary>
    bool IsAltShortcutKeyPressed { get; set; }

    /// <summary>
    /// Raised whenever <see cref="IsAltSchortcutKeyPressed"/> changes.
    /// </summary>
    event Action IsAltShortcutKeyPressedChanged;


    /// <summary>
    /// Gets or sets whether the live log output is currently active/visible.
    /// </summary>
    bool IsLiveLogActive { get; set; }

    /// <summary>
    /// Raised whenever <see cref="IsLiveLogActive"/> changes.
    /// </summary>
    event Action IsLiveLogActiveChanged;


    /// <summary>
    /// Gets or sets whether a "blinked" (highlighted) message has been shown / is present.
    /// </summary>
    bool HasBlinkedMessage { get; set; }

    /// <summary>
    /// Raised whenever <see cref="HasBlinkedMessage"/> changes.
    /// </summary>
    event Action HasBlinkedMessageChanged;


    /// <summary>
    /// Gets or sets whether saving data is currently allowed/enabled.
    /// </summary>
    bool IsEnableToSaveData { get; set; }

    /// <summary>
    /// Raised whenever <see cref="IsEnableToSaveData"/> changes.
    /// </summary>
    event Action IsEnableToSaveDataChanged;


    /// <summary>
    /// Gets or sets whether a required field validation state is currently active.
    /// </summary>
    bool IsRequiredField { get; set; }

    /// <summary>
    /// Raised whenever <see cref="IsRequiredField"/> changes.
    /// </summary>
    event Action IsRequiredFieldChanged;


    /// <summary>
    /// Gets or sets whether there are unsaved changes in the current UI/workflow.
    /// </summary>
    bool HasUnsavedChanges { get; set; }

    /// <summary>
    /// Raised whenever <see cref="HasUnsavedChanges"/> changes.
    /// </summary>
    event Action HasUnsavedChangesChanged;


    /// <summary>
    /// Gets or sets whether the "save request" message should be visible in the UI.
    /// </summary>
    bool IsSaveRequestMessageVisible { get; set; }

    /// <summary>
    /// Raised whenever <see cref="IsSaveRequestMessageVisible"/> changes.
    /// </summary>
    event Action IsSaveRequestMessageVisibleChanged;
   
    /// <summary>
    /// Gets or sets whether the "is loading" message spinner should be visible in the UI.
    /// </summary>
    bool IsLoading { get; set; }

    /// <summary>
    /// Raised whenever <see cref="IsLoading"/> changes.
    /// </summary>
    event Action IsLoadingChanged;
}