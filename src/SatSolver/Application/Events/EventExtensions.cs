using System.Reflection;

namespace Raijin.SatSolver.Application.Events;

public static class EventExtensions
{
    extension<TEvent> (TEvent _) where TEvent : IEvent
    {
        public EventMetadataAttribute Metadata => typeof(TEvent).GetCustomAttribute<EventMetadataAttribute>() ??
                                                  throw new ArgumentException("Event metadata attribute not found");

        public static EventMetadataAttribute GetMetadata() =>
            typeof(TEvent).GetCustomAttribute<EventMetadataAttribute>() ??
            throw new ArgumentException(
                "Event metadata attribute not found");
    }
}