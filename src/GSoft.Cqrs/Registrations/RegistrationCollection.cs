using System.Diagnostics.CodeAnalysis;

namespace GSoft.Cqrs.Registrations;

internal class RegistrationCollection
{
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration", Justification = "Duplicate enumeration is only done in exception.")]
    public RegistrationCollection(
        IEnumerable<QueryRegistration> queryRegistrations,
        IEnumerable<StreamQueryRegistration> streamQueryRegistrations,
        IEnumerable<CommandRegistration> commandRegistrations,
        IEnumerable<EventRegistration> eventRegistrations)
    {
        try
        {
            this.QueryRegistrations = queryRegistrations.ToDictionary(x => x.QueryType);
            this.QueryStreamRegistrations = streamQueryRegistrations.ToDictionary(x => x.QueryType);
            this.CommandRegistrations = commandRegistrations.ToDictionary(x => x.CommandType);

            // Multiple EventHandlers can handle the same event type. As the wrapper is dependent on the EventType, all will point to the same wrapper.
            this.EventRegistrations = eventRegistrations.GroupBy(x => x.EventType).ToDictionary(x => x.Key, x => x.First());
        }
        catch (Exception ex) when (ValidationUtils.TryGetDuplicateRegistrations(queryRegistrations, streamQueryRegistrations, commandRegistrations, out var duplicates))
        {
            throw new ArgumentException($"The same entity has been registered multiple times. [{string.Join(",", duplicates!)}]", ex);
        }
    }

    public IReadOnlyDictionary<Type, QueryRegistration> QueryRegistrations { get; }

    public IReadOnlyDictionary<Type, StreamQueryRegistration> QueryStreamRegistrations { get; }

    public IReadOnlyDictionary<Type, CommandRegistration> CommandRegistrations { get; }

    public IReadOnlyDictionary<Type, EventRegistration> EventRegistrations { get; }
}