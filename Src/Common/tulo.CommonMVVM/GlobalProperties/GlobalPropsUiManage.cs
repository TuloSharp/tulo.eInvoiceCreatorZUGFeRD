using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace tulo.CommonMVVM.GlobalProperties;

public class GlobalPropsUiManage : IGlobalPropsUiManage, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    // utilitiy
    private bool SetField<T>(ref T field, T value, Action changed, [CallerMemberName] string propertyName = null)
    {
        if (Equals(field, value)) return false;
        field = value;
        // 1. existing events remain
        changed?.Invoke();
        // 2. WPF Binding Event
        OnPropertyChanged(propertyName);

        return true;
    }

    // ---------- Message ----------
    private string _message = string.Empty;
    public string Message
    {
        get => _message;
        set => SetField(ref _message, value ?? string.Empty, MessageChanged);
    }
    public event Action MessageChanged = delegate { };

    // ---------- Processing ----------
    private bool _isProcessing;
    public bool IsProcessing
    {
        get => _isProcessing;
        set => SetField(ref _isProcessing, value, IsProcessingChanged);
    }
    public event Action IsProcessingChanged = delegate { };

    // ---------- Display/View mode ----------
    private bool _isDisplayViewMode;
    public bool IsDisplayViewMode
    {
        get => _isDisplayViewMode;
        set => SetField(ref _isDisplayViewMode, value, IsDisplayViewModeChanged);
    }
    public event Action IsDisplayViewModeChanged = delegate { };

    // ---------- IsAltShortcutKeyPressed ----------
    private bool _isAltShortcutKeyPressed;
    public bool IsAltShortcutKeyPressed
    {
        get => _isAltShortcutKeyPressed;
        set => SetField(ref _isAltShortcutKeyPressed, value, IsAltShortcutKeyPressedChanged);
    }
    public event Action IsAltShortcutKeyPressedChanged = delegate { };

    // ---------- Live log ----------
    private bool _isLiveLogActive;
    public bool IsLiveLogActive
    {
        get => _isLiveLogActive;
        set => SetField(ref _isLiveLogActive, value, IsLiveLogActiveChanged);
    }
    public event Action IsLiveLogActiveChanged = delegate { };

    // ---------- Blinked message ----------
    private bool _hasBlinkedMessage;
    public bool HasBlinkedMessage
    {
        get => _hasBlinkedMessage;
        set => SetField(ref _hasBlinkedMessage, value, HasBlinkedMessageChanged);
    }
    public event Action HasBlinkedMessageChanged = delegate { };

    // ---------- Enable save ----------
    private bool _isEnableToSaveData;
    public bool IsEnableToSaveData
    {
        get => _isEnableToSaveData;
        set => SetField(ref _isEnableToSaveData, value, IsEnableToSaveDataChanged);
    }
    public event Action IsEnableToSaveDataChanged = delegate { };

    // ---------- Required field ----------
    private bool _isRequiredField;
    public bool IsRequiredField
    {
        get => _isRequiredField;
        set => SetField(ref _isRequiredField, value, IsRequiredFieldChanged);
    }
    public event Action IsRequiredFieldChanged = delegate { };

    // ---------- HasUnsavedChanges ----------
    private bool _hasUnsavedChanges;
    public bool HasUnsavedChanges
    {
        get => _hasUnsavedChanges;
        set => SetField(ref _hasUnsavedChanges, value, HasUnsavedChangesChanged);
    }
    public event Action HasUnsavedChangesChanged = delegate { };

    // ---------- IsSaveRequestMessageVisible ----------
    private bool _isSaveRequestMessageVisible;
    public bool IsSaveRequestMessageVisible
    {
        get => _isSaveRequestMessageVisible;
        set => SetField(ref _isSaveRequestMessageVisible, value, IsSaveRequestMessageVisibleChanged);
    }
    public event Action IsSaveRequestMessageVisibleChanged = delegate { };
 
    // ---------- IsLoading ----------
    private bool _isLoading;
    public bool IsLoading
    {
        get => IsLoading;
        set => SetField(ref _isLoading, value, IsLoadingChanged);
    }
    public event Action IsLoadingChanged = delegate { };
}