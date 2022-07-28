namespace GSoft.Cqrs.Registrations;

internal sealed class StreamQueryRegistration
{
    public StreamQueryRegistration(Type queryType, Type wrapperType)
    {
        this.QueryType = queryType;
        this.WrapperType = wrapperType;
    }

    public Type QueryType { get; }

    public Type WrapperType { get; }
}