namespace tulo.CommonMVVM.ViewModels;
/// <summary>
/// Marks a ViewModel as having a UI status/message area (e.g., a banner, toast, or inline message).
/// Commands or services can use this interface to read or clear the message in a generic way,
/// without knowing the concrete ViewModel type.
/// </summary>
public interface IHasStatusMessage
{
    /// <summary>
    /// Gets the ViewModel that holds the current status message shown in the UI.
    /// Set <see cref="MessageViewModel.Message"/> to an empty string to hide/clear the message.
    /// </summary>
    MessageViewModel StatusMessageViewModel { get; }
}
