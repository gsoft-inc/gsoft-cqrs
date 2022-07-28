namespace GSoft.Cqrs;

public interface IRequest<out TResponse> : IRequest
{
}

public interface IRequest
{
}