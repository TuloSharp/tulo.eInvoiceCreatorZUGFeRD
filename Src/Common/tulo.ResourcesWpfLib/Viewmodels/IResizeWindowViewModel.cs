namespace tulo.ResourcesWpfLib.Viewmodels;

public interface IResizeWindowViewModel
{
    bool IsWindowMaximized { get; set; }
    /// <summary>
    /// set state when window has a custom size
    /// </summary>
    bool IsWindowCustomResized { get; set; }
}
