using Microsoft.Extensions.DependencyInjection;
using MinDiator.Interfaces;
using System.Reflection;

namespace MinDiator.Configuration;

/// <summary>
/// Classe de configuração para registrar serviços do padrão Mediator no container de DI
/// </summary>
public class MinDiatorConfiguration
{
    private readonly List<Assembly> _assemblies = new();
    private readonly List<(Type InterfaceType, Type ImplementationType, ServiceLifetime Lifetime)> _behaviors = new();
    private readonly List<(Type InterfaceType, Type ImplementationType, ServiceLifetime Lifetime)> _exceptionHandlers = new();
    private readonly IServiceCollection _services;
    private bool _hasRegistered = false;

    /// <summary>
    /// Construtor interno usado pela extensão AddMediatorPattern
    /// </summary>
    /// <param name="services">Coleção de serviços do DI</param>
    internal MinDiatorConfiguration(IServiceCollection services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services), "IServiceCollection não pode ser nulo.");
    }

    /// <summary>
    /// Registra handlers e behaviors de um assembly específico
    /// </summary>
    /// <param name="assembly">Assembly a ser escaneado</param>
    /// <returns>A instância de configuração para chamadas em cadeia</returns>
    public MinDiatorConfiguration RegisterServicesFromAssembly(Assembly assembly)
    {
        if (assembly == null)
            throw new ArgumentNullException(nameof(assembly), "Assembly fornecido não pode ser nulo.");

        if (!_assemblies.Contains(assembly))
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
        var assembly = typeof(T).Assembly;
        return assembly == null
            ? throw new InvalidOperationException($"Não foi possível localizar o assembly para o tipo {typeof(T).FullName}.")
            : RegisterServicesFromAssembly(assembly);
    }


    /// <summary>
    /// Registra handlers e behaviors de múltiplos assemblies
    /// </summary>
    /// <param name="assemblies">Assemblies a serem escaneados</param>
    /// <returns>A instância de configuração para chamadas em cadeia</returns>
    public MinDiatorConfiguration RegisterServicesFromAssemblies(params Assembly[] assemblies)
    {
        if (assemblies == null || assemblies.Length == 0)
            throw new ArgumentException("Ao menos um assembly deve ser fornecido.", nameof(assemblies));

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
    /// Adiciona um behavior específico ao pipeline
    /// </summary>
    /// <param name="behaviorInterface">Tipo da interface do behavior</param>
    /// <param name="behaviorImplementation">Tipo da implementação do behavior</param>
    /// <param name="lifetime">Tempo de vida do serviço no container</param>
    /// <returns>A instância de configuração para chamadas em cadeia</returns>
    public MinDiatorConfiguration AddBehavior(Type behaviorInterface, Type behaviorImplementation, ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        if (behaviorInterface == null)
            throw new ArgumentNullException(nameof(behaviorInterface), "Interface do behavior não pode ser nula.");

        if (behaviorImplementation == null)
            throw new ArgumentNullException(nameof(behaviorImplementation), "Implementação do behavior não pode ser nula.");

        _behaviors.Add((behaviorInterface, behaviorImplementation, lifetime));
        return this;
    }

    /// <summary>
    /// Adiciona um handler de exceção específico
    /// </summary>
    /// <param name="handlerInterface">Tipo da interface do handler de exceção</param>
    /// <param name="handlerImplementation">Tipo da implementação do handler de exceção</param>
    /// <returns>A instância de configuração para chamadas em cadeia</returns>
    public MinDiatorConfiguration AddExceptionHandler(Type handlerInterface, Type handlerImplementation, ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        if (handlerInterface == null)
            throw new ArgumentNullException(nameof(handlerInterface), "Interface do exception handler não pode ser nula.");

        if (handlerImplementation == null)
            throw new ArgumentNullException(nameof(handlerImplementation), "Implementação do exception handler não pode ser nula.");

        _exceptionHandlers.Add((handlerInterface, handlerImplementation, lifetime));
        return this;
    }

    /// <summary>
    /// Registra todos os serviços configurados no container de DI
    /// Método interno chamado pela extensão AddMediatorPattern
    /// </summary>
    internal void Register()
    {
        if (!_assemblies.Any())
            throw new InvalidOperationException("Nenhum assembly registrado. Utilize 'RegisterServicesFromAssembly' ou métodos similares para adicionar assemblies antes de registrar.");


        foreach (var assembly in _assemblies.Distinct())
        {
            var types = assembly.GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .ToArray();

            RegisterGenericHandlers(types, typeof(IRequestHandler<>));
            RegisterGenericHandlers(types, typeof(IRequestHandler<,>));
            RegisterGenericHandlers(types, typeof(IRequestExceptionHandler<,>));
            RegisterGenericHandlers(types, typeof(IRequestExceptionHandler<,,>));
        }

        // Registrar behaviors configurados explicitamente
        RegisterConfiguredBehaviors();

        // Registrar exception handlers configurados explicitamente
        RegisterConfiguredExceptionHandlers();

        //// Registrar o mediador
        _services.AddSingleton<IMediator>(provider =>
        {
            // Instancia a classe concreta
            return new Mediator(provider, _assemblies);
        });




        _hasRegistered = true;
    }

    /// <summary>
    /// Registra handlers de requisições de um assembly específico
    /// </summary>
    /// <param name="assembly">Assembly a ser escaneado</param>
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
    /// Registra behaviors configurados explicitamente
    /// </summary>
    private void RegisterConfiguredBehaviors()
    {
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
        foreach (var (interfaceType, implementationType, lifetime) in _exceptionHandlers)
        {
            _services.Add(new ServiceDescriptor(interfaceType, implementationType));
        }
    }
}