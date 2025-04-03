using MinDiator.Handlers;

namespace MinDiator.Interfaces;


/// <summary>
/// Interface para behaviors de pipeline que podem interceptar e modificar requisições
/// ou respostas durante o processamento
/// </summary>
/// <typeparam name="TRequest">Tipo da requisição</typeparam>
/// <typeparam name="TResponse">Tipo da resposta</typeparam>
public interface IPipelineBehavior<TRequest, TResponse>
{
    /// <summary>
    /// Manipula a requisição, podendo executar lógica antes e depois do handler
    /// </summary>
    /// <param name="request">A requisição a ser processada</param>
    /// <param name="next">Delegado que representa o próximo behavior ou o handler na cadeia</param>
    /// <param name="cancellationToken">Token de cancelamento para operações assíncronas</param>
    /// <returns>A resposta do handler ou do behavior modificada</returns>
    Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
}
