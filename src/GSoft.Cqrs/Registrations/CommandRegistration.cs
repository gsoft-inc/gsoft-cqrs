namespace GSoft.Cqrs.Registrations;

internal class CommandRegistration
{
    public CommandRegistration(Type commandType, Type wrapperType)
    {
        this.CommandType = commandType;
        this.WrapperType = wrapperType;
    }

    public Type CommandType { get; }

    public Type WrapperType { get; }
}