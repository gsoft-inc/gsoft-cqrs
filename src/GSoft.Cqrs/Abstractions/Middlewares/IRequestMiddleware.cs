namespace GSoft.Cqrs.Abstractions.Middlewares;

public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();
public delegate Task RequestHandlerDelegate();

public interface IRequestMiddleware<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> HandleAsync(RequestHandlerDelegate<TResponse> next, TRequest request, CancellationToken cancellationToken);
}

public interface IRequestMiddleware<in TRequest>
    where TRequest : IRequest
{
    Task HandleAsync(RequestHandlerDelegate next, TRequest request, CancellationToken cancellationToken);
}
