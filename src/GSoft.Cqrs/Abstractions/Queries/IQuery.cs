namespace GSoft.Cqrs;

public interface IQuery<out TResponse> : IRequest<TResponse>
{
}