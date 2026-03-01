namespace tulo.SerilogLib.Common;

public interface IObservableLogSink
{
    event Action<string>? MessageReceived;
}
