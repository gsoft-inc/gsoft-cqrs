namespace GSoft.Cqrs.Registrations;

internal class EventRegistration
{
    public EventRegistration(Type eventType, Type wrapperType)
    {
        this.EventType = eventType;
        this.WrapperType = wrapperType;
    }

    public Type EventType { get; }

    public Type WrapperType { get; }
}
