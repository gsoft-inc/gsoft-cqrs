using GSoft.Cqrs;
using GSoft.Cqrs.Abstractions.Events;
using GSoft.Cqrs.Handlers;
using GSoft.Cqrs.Registrations;

namespace Microsoft.Extensions.DependencyInjection;

public static class CqrsBuilderExtensions
{
    public static void AddMediator(this IServiceCollection services)
    {
        services.AddTransient<IMediator, Mediator>();
        services.AddSingleton<RegistrationCollection>();
    }

    public static void AddHandler<THandler>(this IServiceCollection services, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where THandler : IHandler
    {
        services.AddHandler(typeof(THandler), serviceLifetime: serviceLifetime);
    }

    public static void AddHandler<THandler>(this IServiceCollection services, Func<IServiceProvider, object> implementationFactory, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where THandler : IHandler
    {
        services.AddHandler(typeof(THandler), implementationFactory, serviceLifetime);
    }

    public static void AddHandler<THandler, TEvent>(this IServiceCollection services, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where THandler : IEventHandler<TEvent>
        where TEvent : IEvent
    {
        services.AddHandler<THandler, TEvent>(p => ActivatorUtilities.CreateInstance<THandler>(p), serviceLifetime);
    }

    public static void AddHandler<THandler, TEvent>(this IServiceCollection services, Func<IServiceProvider, object> implementationFactory, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where THandler : IEventHandler<TEvent>
        where TEvent : IEvent
    {
        AddHandler<THandler>(services, implementationFactory);
        switch (serviceLifetime)
        {
            case ServiceLifetime.Singleton:
                services.AddSingleton(typeof(IEventHandler<TEvent>), implementationFactory);
                break;
            case ServiceLifetime.Scoped:
                services.AddScoped(typeof(IEventHandler<TEvent>), implementationFactory);
                break;
            case ServiceLifetime.Transient:
                services.AddTransient(typeof(IEventHandler<TEvent>), implementationFactory);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(serviceLifetime), serviceLifetime, null);
        }
    }

    public static void AddHandler(this IServiceCollection services, Type handlerType, Func<IServiceProvider, object>? implementationFactory = null, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        foreach (var @interface in handlerType.GetInterfaces().Where(x => x.IsGenericType))
        {
            RegisterHandler(services, handlerType, @interface, typeof(IQueryHandler<,>), typeof(QueryHandlerWrapper<,,>), typeof(QueryRegistration));
            RegisterHandler(services, handlerType, @interface, typeof(IStreamHandler<,>), typeof(StreamHandlerWrapper<,,>), typeof(StreamQueryRegistration));
            RegisterHandler(services, handlerType, @interface, typeof(ICommandHandler<,>), typeof(CommandHandlerWrapper<,,>), typeof(CommandRegistration));
            RegisterHandler(services, handlerType, @interface, typeof(ICommandHandler<>), typeof(CommandHandlerWrapper<,>), typeof(CommandRegistration));
            RegisterHandler(services, handlerType, @interface, typeof(IEventHandler<>), typeof(EventHandlerWrapper<>), typeof(EventRegistration));
        }

        if (implementationFactory != null)
        {
            services.Add(new ServiceDescriptor(handlerType, implementationFactory, serviceLifetime));
        }
        else
        {
            services.Add(new ServiceDescriptor(handlerType, handlerType, serviceLifetime));
        }
    }

    private static void RegisterHandler(IServiceCollection services, Type type, Type @interface, Type handlerType, Type wrapperType, Type registrationType)
    {
        if (@interface.IsGenericType && @interface.GetGenericTypeDefinition() == handlerType)
        {
            var arguments = @interface.GetGenericArguments();
            var request = arguments[0];

            Type genericWrapperType;
            if (arguments.Length == 1)
            {
                if (wrapperType.GetGenericArguments().Length == 1)
                {
                    genericWrapperType = wrapperType.MakeGenericType(request);
                }
                else
                {
                    genericWrapperType = wrapperType.MakeGenericType(type, request);
                }
            }
            else if (arguments.Length == 2)
            {
                var result = arguments[1];
                genericWrapperType = wrapperType.MakeGenericType(type, request, result);
            }
            else
            {
                throw new ArgumentException(null, nameof(@interface));
            }

            services.AddTransient(genericWrapperType);

            var registration = Activator.CreateInstance(registrationType, request, genericWrapperType)!;

            services.AddSingleton(registrationType, registration);
        }
    }
}