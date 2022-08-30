using System.Reflection;
using GSoft.Cqrs;
using GSoft.Cqrs.Abstractions.Events;
using GSoft.Cqrs.Handlers;
using GSoft.Cqrs.Registrations;

namespace Microsoft.Extensions.DependencyInjection;

public static class CqrsBuilderExtensions
{
    public static IServiceCollection AddMediator(this IServiceCollection services)
    {
        services.AddTransient<IMediator, Mediator>();
        services.AddSingleton<RegistrationCollection>();

        return services;
    }

    public static IServiceCollection AddMediator(this IServiceCollection services, params Assembly[] assembliesToScan)
    {
        services.AddMediator();
        services.AddHandlers(assembliesToScan);

        return services;
    }

    public static IServiceCollection AddHandlers(this IServiceCollection services, params Assembly[] assembliesToScan)
    {
        return services.AddHandlers(t => true, assembliesToScan);
    }

    public static IServiceCollection AddHandlers(this IServiceCollection services, Func<Type, bool> typeFilter, params Assembly[] assembliesToScan)
    {
        var supportedHandlers = new[]
        {
            typeof(IQueryHandler<,>),
            typeof(IStreamHandler<,>),
            typeof(ICommandHandler<,>),
            typeof(ICommandHandler<>),
            typeof(IEventHandler<>),
        };

        var typesToRegister =
            from assembly in assembliesToScan
            from type in assembly.GetTypes()
            where typeFilter(type)
            where type.GetInterfaces().Any(i => i.IsGenericType && supportedHandlers.Contains(i.GetGenericTypeDefinition()))
            select type;

        typesToRegister.Distinct().ToList().ForEach(s => services.AddHandler(s));

        return services;
    }

    public static IServiceCollection AddHandler(this IServiceCollection services, Type handlerType, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        return services.AddHandler(handlerType, (Func<IServiceProvider, object>)null!, serviceLifetime: serviceLifetime);
    }

    public static IServiceCollection AddHandler<THandler>(this IServiceCollection services, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where THandler : IHandler
    {
        return services.AddHandler(typeof(THandler), (Func<IServiceProvider, THandler>)null!, serviceLifetime: serviceLifetime);
    }

    public static IServiceCollection AddHandler<THandler>(this IServiceCollection services, Func<IServiceProvider, THandler> implementationFactory, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where THandler : IHandler
    {
        return services.AddHandler(typeof(THandler), implementationFactory, serviceLifetime);
    }

    public static IServiceCollection AddHandler<THandler, TEvent>(this IServiceCollection services, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where THandler : IEventHandler<TEvent>
        where TEvent : IEvent
    {
        return services.AddHandler<THandler, TEvent>(p => ActivatorUtilities.CreateInstance<THandler>(p), serviceLifetime);
    }

    public static IServiceCollection AddHandler<THandler, TEvent>(this IServiceCollection services, Func<IServiceProvider, THandler> implementationFactory, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where THandler : IEventHandler<TEvent>
        where TEvent : IEvent
    {
        AddHandler(services, implementationFactory);
        switch (serviceLifetime)
        {
            case ServiceLifetime.Singleton:
                services.AddSingleton(typeof(IEventHandler<TEvent>), (implementationFactory as Func<IServiceProvider, object>)!);
                break;
            case ServiceLifetime.Scoped:
                services.AddScoped(typeof(IEventHandler<TEvent>), (implementationFactory as Func<IServiceProvider, object>)!);
                break;
            case ServiceLifetime.Transient:
                services.AddTransient(typeof(IEventHandler<TEvent>), (implementationFactory as Func<IServiceProvider, object>)!);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(serviceLifetime), serviceLifetime, null);
        }

        return services;
    }

    public static IServiceCollection AddHandler<THandler>(this IServiceCollection services, Type handlerType, Func<IServiceProvider, THandler>? implementationFactory = null, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
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
            services.Add(new ServiceDescriptor(handlerType, (implementationFactory as Func<IServiceProvider, object>)!, serviceLifetime));
        }
        else
        {
            services.Add(new ServiceDescriptor(handlerType, handlerType, serviceLifetime));
        }

        return services;
    }

    private static IServiceCollection RegisterHandler(IServiceCollection services, Type type, Type @interface, Type handlerType, Type wrapperType, Type registrationType)
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

        return services;
    }
}