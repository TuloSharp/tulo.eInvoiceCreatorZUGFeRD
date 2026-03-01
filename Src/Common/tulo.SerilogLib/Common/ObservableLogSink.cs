using Serilog.Core;
using Serilog.Events;

namespace tulo.SerilogLib.Common;

public class ObservableLogSink : ILogEventSink, IObservableLogSink
{
    public event Action<string>? MessageReceived;

    const string Separator = "+_#_+";
    public void Emit(LogEvent logEvent)
    {
        var text = $"{logEvent.Timestamp:HH:mm:ss.fff} {Separator} [{logEvent.Level}] {Separator}  {logEvent.RenderMessage()}";
        MessageReceived?.Invoke(text);
    }
}
