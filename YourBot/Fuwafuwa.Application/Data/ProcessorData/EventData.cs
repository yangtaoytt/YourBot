using Fuwafuwa.Core.Data.ServiceData.Level1;
using Lagrange.Core.Event;

namespace YourBot.Fuwafuwa.Application.Data.ProcessorData;

public class EventData : IProcessorData {
    public EventData(EventBase @event) {
        Event = @event;
    }

    public EventBase Event { get; set; }
}