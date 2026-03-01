using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace tulo.CommonMVVM.ViewModels;

public class BaseViewModel : INotifyPropertyChanged
{
    #region OnPropertyChanged
    public event PropertyChangedEventHandler PropertyChanged;

    //protected void OnPropertyChanged(string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    protected void OnPropertyChanged(string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion

    #region CallerMemberName
    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null!)
    {
        if (Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
    #endregion

    public virtual void Dispose() { }
}
