using Microsoft.Extensions.DependencyInjection;
using MinDiator.Handlers;
using MinDiator.Interfaces;
using System.Reflection;

namespace MinDiator.Configuration;

/// <summary>
/// Configuration class for registering Mediator pattern services in the DI container.
/// </summary>
public class MinDiatorConfiguration
{
    private readonly List<Assembly> _assemblies = new();
    private readonly List<(Type InterfaceType, Type ImplementationType, ServiceLifetime Lifetime)> _behaviors = new();
    private readonly List<(Type InterfaceType, Type ImplementationType, ServiceLifetime Lifetime)> _exceptionHandlers = new();
    private readonly IServiceCollection _services;
    private bool _hasRegistered = false;

    /// <summary>
    /// Internal constructor used by the AddMediatorPattern extension.
    /// </summary>
    /// <param name="services">DI service collection.</param>
    internal MinDiatorConfiguration(IServiceCollection services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services), "IServiceCollection cannot be null.");
    }

    /// <summary>
    /// Registers handlers and behaviors from a specific assembly.
    /// </summary>
    /// <param name="assembly">Assembly to be scanned.</param>
    /// <returns>The configuration instance for method chaining.</returns>
    public MinDiatorConfiguration RegisterServicesFromAssembly(Assembly assembly)
    {
        if (assembly == null)
            throw new ArgumentNullException(nameof(assembly), "Provided assembly cannot be null.");

        if (!_assemblies.Contains(assembly))
            _assemblies.Add(assembly);

        return this;
    }

    /// <summary>
    /// Registers handlers and behaviors from the assembly that contains the specified type.
    /// </summary>
    /// <typeparam name="T">Type whose assembly will be scanned.</typeparam>
    /// <returns>The configuration instance for method chaining.</returns>
    public MinDiatorConfiguration RegisterServicesFromAssemblyContaining<T>()
    {
        var assembly = typeof(T).Assembly;
        return assembly == null
            ? throw new InvalidOperationException($"Could not locate the assembly for type {typeof(T).FullName}.")
            : RegisterServicesFromAssembly(assembly);
    }

    /// <summary>
    /// Registers handlers and behaviors from multiple assemblies.
    /// </summary>
    /// <param name="assemblies">Assemblies to be scanned.</param>
    /// <returns>The configuration instance for method chaining.</returns>
    public MinDiatorConfiguration RegisterServicesFromAssemblies(params Assembly[] assemblies)
    {
        if (assemblies == null || assemblies.Length == 0)
            throw new ArgumentException("At least one assembly must be provided.", nameof(assemblies));

        foreach (var assembly in assemblies)
        {
            RegisterServicesFromAssembly(assembly);
        }
        return this;
    }

    internal void EnsureRegistered()
    {
        if (_hasRegistered) return;
        Register();
    }

    /// <summary>
    /// Adds a specific behavior to the pipeline.
    /// </summary>
    /// <param name="behaviorInterface">Type of the behavior interface.</param>
    /// <param name="behaviorImplementation">Type of the behavior implementation.</param>
    /// <param name="lifetime">Lifetime of the service in the container.</param>
    /// <returns>The configuration instance for method chaining.</returns>
    public MinDiatorConfiguration AddBehavior(Type behaviorInterface, Type behaviorImplementation, ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        if (behaviorInterface == null)
            throw new ArgumentNullException(nameof(behaviorInterface), "Behavior interface cannot be null.");

        if (behaviorImplementation == null)
            throw new ArgumentNullException(nameof(behaviorImplementation), "Behavior implementation cannot be null.");

        _behaviors.Add((behaviorInterface, behaviorImplementation, lifetime));
        return this;
    }

    /// <summary>
    /// Adds a specific exception handler.
    /// </summary>
    /// <param name="handlerInterface">Type of the exception handler interface.</param>
    /// <param name="handlerImplementation">Type of the exception handler implementation.</param>
    /// <returns>The configuration instance for method chaining.</returns>
    public MinDiatorConfiguration AddExceptionHandler(Type handlerInterface, Type handlerImplementation, ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        if (handlerInterface == null)
            throw new ArgumentNullException(nameof(handlerInterface), "Exception handler interface cannot be null.");

        if (handlerImplementation == null)
            throw new ArgumentNullException(nameof(handlerImplementation), "Exception handler implementation cannot be null.");

        _exceptionHandlers.Add((handlerInterface, handlerImplementation, lifetime));
        return this;
    }

    /// <summary>
    /// Registers all configured services in the DI container.
    /// Internal method called by the AddMediatorPattern extension.
    /// </summary>
    internal void Register()
    {
        if (!_assemblies.Any())
            throw new InvalidOperationException("No assemblies registered. Use 'RegisterServicesFromAssembly' or similar methods to add assemblies before registering.");

        foreach (var assembly in _assemblies.Distinct())
        {
            var types = assembly.GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .ToArray();

            RegisterGenericHandlers(types, typeof(IRequestHandler<>));
            RegisterGenericHandlers(types, typeof(IRequestHandler<,>));
            RegisterGenericHandlers(types, typeof(IRequestExceptionHandler<,>));
            RegisterGenericHandlers(types, typeof(IRequestExceptionHandler<,,>));

            // Adiciona registro para notification handlers
            RegisterGenericHandlers(types, typeof(INotificationHandler<>));
        }

        // Register explicitly configured behaviors.
        RegisterConfiguredBehaviors();

        // Register explicitly configured exception handlers.
        RegisterConfiguredExceptionHandlers();

        // Register the mediator components.
        _services.AddSingleton<IPublisher, Publisher>();
        _services.AddSingleton<IMediator, Mediator>();
        _services.AddSingleton<ISender, Sender>();

        _hasRegistered = true;
    }

    /// <summary>
    /// Registers request handlers from a specific assembly.
    /// </summary>
    /// <param name="assembly">Assembly to be scanned.</param>
    private void RegisterGenericHandlers(Type[] types, Type interfaceDefinition, ServiceLifetime lifetime = ServiceLifetime.Transient, bool skipOpenGenericImplementations = false)
    {
        foreach (var type in types)
        {
            if (skipOpenGenericImplementations && type.ContainsGenericParameters)
                continue;

            var interfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceDefinition);

            foreach (var @interface in interfaces)
            {
                _services.Add(new ServiceDescriptor(@interface, type, lifetime));
            }
        }
    }

    /// <summary>
    /// Registers explicitly configured behaviors.
    /// </summary>
    private void RegisterConfiguredBehaviors()
    {
        foreach (var (interfaceType, implementationType, lifetime) in _behaviors)
        {
            _services.Add(new ServiceDescriptor(interfaceType, implementationType, lifetime));
        }
    }

    /// <summary>
    /// Registers explicitly configured exception handlers.
    /// </summary>
    private void RegisterConfiguredExceptionHandlers()
    {
        foreach (var (interfaceType, implementationType, lifetime) in _exceptionHandlers)
        {
            _services.Add(new ServiceDescriptor(interfaceType, implementationType, lifetime));
        }
    }
}