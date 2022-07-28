namespace GSoft.Cqrs.Registrations;

internal sealed class QueryRegistration
{
    public QueryRegistration(Type queryType, Type wrapperType)
    {
        this.QueryType = queryType;
        this.WrapperType = wrapperType;
    }

    public Type QueryType { get; }

    public Type WrapperType { get; }
}