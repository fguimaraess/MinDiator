using MinDiator.Configuration;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;
/// <summary>
/// Extensões para IServiceCollection para registro do padrão Mediator
/// </summary>
public static class MediatorServiceCollectionExtensions
{
    /// <summary>
    /// Adiciona e configura o padrão Mediator ao container de DI
    /// </summary>
    /// <param name="services">Coleção de serviços do DI</param>
    /// <param name="configAction">Ação de configuração para personalizar o registro</param>
    /// <returns>A coleção de serviços para chamadas em cadeia</returns>
    public static IServiceCollection AddMinDiator(this IServiceCollection services, Action<MinDiatorConfiguration> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var configuration = new MinDiatorConfiguration(services);
        configure(configuration);

        configuration.Register();
        return services;
    }

    public static IServiceCollection AddMinDiator(this IServiceCollection services, string assemblyName)
    {
        var assembly = AppDomain.CurrentDomain.Load(assemblyName);

        // Tenta carregar o assembly se ainda não estiver carregado
        assembly ??= Assembly.Load(assemblyName);

        if (assembly is null)
            throw new InvalidOperationException($"Assembly '{assemblyName}' not found.");

        services.AddMinDiator(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
        });

        return services;
    }
}