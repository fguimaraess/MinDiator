using Microsoft.Extensions.DependencyInjection;
using MinDiator.Configuration;

namespace MinDiator.Extensions;

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
    public static IServiceCollection AddMinDiator(this IServiceCollection services, Action<MinDiatorConfiguration> configAction)
    {
        var configuration = new MinDiatorConfiguration(services);
        configAction(configuration);
        configuration.Register();
        return services;
    }

}