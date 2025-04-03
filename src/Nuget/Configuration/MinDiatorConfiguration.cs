using Microsoft.Extensions.DependencyInjection;
using MinDiator.Interfaces;
using System.Reflection;

namespace MinDiator.Configuration;

/// <summary>
/// Classe de configuração para registrar serviços do padrão Mediator no container de DI
/// </summary>
public class MinDiatorConfiguration
{
    private readonly List<Assembly> _assemblies = new List<Assembly>();
    private readonly List<(Type, Type, ServiceLifetime)> _behaviors = new();
    private readonly List<(Type, Type)> _exceptionHandlers = new();
    private readonly IServiceCollection _services;

    /// <summary>
    /// Construtor interno usado pela extensão AddMediatorPattern
    /// </summary>
    /// <param name="services">Coleção de serviços do DI</param>
    internal MinDiatorConfiguration(IServiceCollection services)
    {
        _services = services;
    }

    /// <summary>
    /// Registra handlers e behaviors de um assembly específico
    /// </summary>
    /// <param name="assembly">Assembly a ser escaneado</param>
    /// <returns>A instância de configuração para chamadas em cadeia</returns>
    public MinDiatorConfiguration RegisterServicesFromAssembly(Assembly assembly)
    {
        _assemblies.Add(assembly);
        return this;
    }

    /// <summary>
    /// Registra handlers e behaviors do assembly que contém o tipo especificado
    /// </summary>
    /// <typeparam name="T">Tipo cujo assembly será escaneado</typeparam>
    /// <returns>A instância de configuração para chamadas em cadeia</returns>
    public MinDiatorConfiguration RegisterServicesFromAssemblyContaining<T>()
    {
        return RegisterServicesFromAssembly(typeof(T).Assembly);
    }

    /// <summary>
    /// Adiciona um behavior específico ao pipeline
    /// </summary>
    /// <param name="behaviorInterface">Tipo da interface do behavior</param>
    /// <param name="behaviorImplementation">Tipo da implementação do behavior</param>
    /// <param name="lifetime">Tempo de vida do serviço no container</param>
    /// <returns>A instância de configuração para chamadas em cadeia</returns>
    public MinDiatorConfiguration AddBehavior(Type behaviorInterface, Type behaviorImplementation, ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        _behaviors.Add((behaviorInterface, behaviorImplementation, lifetime));
        return this;
    }

    /// <summary>
    /// Adiciona um handler de exceção específico
    /// </summary>
    /// <param name="handlerInterface">Tipo da interface do handler de exceção</param>
    /// <param name="handlerImplementation">Tipo da implementação do handler de exceção</param>
    /// <returns>A instância de configuração para chamadas em cadeia</returns>
    public MinDiatorConfiguration AddExceptionHandler(Type handlerInterface, Type handlerImplementation)
    {
        _exceptionHandlers.Add((handlerInterface, handlerImplementation));
        return this;
    }

    /// <summary>
    /// Registra handlers e behaviors de múltiplos assemblies
    /// </summary>
    /// <param name="assemblies">Assemblies a serem escaneados</param>
    /// <returns>A instância de configuração para chamadas em cadeia</returns>
    public MinDiatorConfiguration RegisterServicesFromAssemblies(params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            _assemblies.Add(assembly);
        }
        return this;
    }

    /// <summary>
    /// Registra todos os serviços configurados no container de DI
    /// Método interno chamado pela extensão AddMediatorPattern
    /// </summary>
    internal void Register()
    {
        // Registrar o mediador
        _services.AddScoped<IMinDiator, Handlers.MinDiator>();

        foreach (var assembly in _assemblies)
        {
            RegisterRequestHandlers(assembly);
            RegisterExceptionHandlers(assembly);
        }

        // Registrar behaviors configurados explicitamente
        RegisterConfiguredBehaviors();

        // Registrar exception handlers configurados explicitamente
        RegisterConfiguredExceptionHandlers();
    }

    /// <summary>
    /// Registra handlers de requisições de um assembly específico
    /// </summary>
    /// <param name="assembly">Assembly a ser escaneado</param>
    private void RegisterRequestHandlers(Assembly assembly)
    {
        // Registrar handlers com resposta
        var handlerTypesWithResponse = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(t => t.GetInterfaces(), (t, i) => new { Type = t, Interface = i })
            .Where(x => x.Interface.IsGenericType &&
                       (x.Interface.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)))
            .ToList();

        foreach (var handler in handlerTypesWithResponse)
        {
            _services.AddTransient(handler.Interface, handler.Type);
        }

        // Registrar handlers sem resposta
        var handlerTypesWithoutResponse = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(t => t.GetInterfaces(), (t, i) => new { Type = t, Interface = i })
            .Where(x => x.Interface.IsGenericType &&
                       (x.Interface.GetGenericTypeDefinition() == typeof(IRequestHandler<>)))
            .ToList();

        foreach (var handler in handlerTypesWithoutResponse)
        {
            _services.AddTransient(handler.Interface, handler.Type);
        }
    }

    /// <summary>
    /// Registra handlers de exceção de um assembly específico
    /// </summary>
    /// <param name="assembly">Assembly a ser escaneado</param>
    private void RegisterExceptionHandlers(Assembly assembly)
    {
        // Registrar handlers de exceção com resposta
        var exceptionHandlersWithResponse = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(t => t.GetInterfaces(), (t, i) => new { Type = t, Interface = i })
            .Where(x => x.Interface.IsGenericType &&
                       (x.Interface.GetGenericTypeDefinition() == typeof(IRequestExceptionHandler<,,>)))
            .ToList();

        foreach (var handler in exceptionHandlersWithResponse)
        {
            _services.AddTransient(handler.Interface, handler.Type);
        }

        // Registrar handlers de exceção sem resposta
        var exceptionHandlersWithoutResponse = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(t => t.GetInterfaces(), (t, i) => new { Type = t, Interface = i })
            .Where(x => x.Interface.IsGenericType &&
                       (x.Interface.GetGenericTypeDefinition() == typeof(IRequestExceptionHandler<,>)))
            .ToList();

        foreach (var handler in exceptionHandlersWithoutResponse)
        {
            _services.AddTransient(handler.Interface, handler.Type);
        }
    }

    /// <summary>
    /// Registra behaviors configurados explicitamente
    /// </summary>
    private void RegisterConfiguredBehaviors()
    {
        // Register behaviors
        foreach (var (interfaceType, implementationType, lifetime) in _behaviors)
        {
            _services.Add(new ServiceDescriptor(interfaceType, implementationType, lifetime));
        }
    }

    /// <summary>
    /// Registra handlers de exceção configurados explicitamente
    /// </summary>
    private void RegisterConfiguredExceptionHandlers()
    {
        // Register exception handlers
        foreach (var (interfaceType, implementationType) in _exceptionHandlers)
        {
            _services.AddTransient(interfaceType, implementationType);
        }
    }
}