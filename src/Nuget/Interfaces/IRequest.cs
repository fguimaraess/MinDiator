using MinDiator.Entities;
using MinDiator.Handlers;

namespace MinDiator.Interfaces;


/// <summary>
/// Interface que marca uma classe como uma requisição que produz uma resposta
/// </summary>
/// <typeparam name="TResponse">Tipo da resposta esperada</typeparam>
public interface IRequest<TResponse> { }

/// <summary>
/// Interface que marca uma classe como uma requisição que não produz resposta
/// </summary>
public interface IRequest : IRequest<Unit> { }

/// <summary>
/// Interface para handlers que processam requisições e produzem respostas
/// </summary>
/// <typeparam name="TRequest">Tipo da requisição</typeparam>
/// <typeparam name="TResponse">Tipo da resposta</typeparam>
public interface IRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Processa a requisição e retorna uma resposta
    /// </summary>
    /// <param name="request">A requisição a ser processada</param>
    /// <param name="cancellationToken">Token de cancelamento para operações assíncronas</param>
    /// <returns>A resposta do handler</returns>
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// Interface simplificada para handlers que processam requisições sem resposta
/// </summary>
/// <typeparam name="TRequest">Tipo da requisição</typeparam>
public interface IRequestHandler<TRequest> : IRequestHandler<TRequest, Unit> where TRequest : IRequest<Unit> { }

/// <summary>
/// Interface para handlers de exceção que podem tratar exceções específicas durante o processamento de requisições
/// </summary>
/// <typeparam name="TRequest">Tipo da requisição</typeparam>
/// <typeparam name="TResponse">Tipo da resposta</typeparam>
/// <typeparam name="TException">Tipo da exceção a ser tratada</typeparam>
public interface IRequestExceptionHandler<in TRequest, TResponse, in TException>
    where TRequest : IRequest<TResponse>
    where TException : Exception
{
    /// <summary>
    /// Trata uma exceção específica ocorrida durante o processamento de uma requisição
    /// </summary>
    /// <param name="request">A requisição que estava sendo processada</param>
    /// <param name="exception">A exceção que ocorreu</param>
    /// <param name="state">Estado que contém a resposta a ser retornada caso a exceção seja tratada</param>
    /// <param name="cancellationToken">Token de cancelamento para operações assíncronas</param>
    /// <returns>Unit indicando a conclusão do tratamento</returns>
    Task<Unit> Handle(TRequest request, TException exception, RequestExceptionHandlerState<TResponse> state, CancellationToken cancellationToken);
}

/// <summary>
/// Interface para handlers de exceção para requisições sem resposta
/// </summary>
/// <typeparam name="TRequest">Tipo da requisição</typeparam>
/// <typeparam name="TException">Tipo da exceção a ser tratada</typeparam>
public interface IRequestExceptionHandler<in TRequest, in TException>
    where TRequest : IRequest
    where TException : Exception
{
    /// <summary>
    /// Trata uma exceção específica ocorrida durante o processamento de uma requisição sem resposta
    /// </summary>
    /// <param name="request">A requisição que estava sendo processada</param>
    /// <param name="exception">A exceção que ocorreu</param>
    /// <param name="state">Estado que contém a resposta a ser retornada caso a exceção seja tratada</param>
    /// <param name="cancellationToken">Token de cancelamento para operações assíncronas</param>
    /// <returns>Unit indicando a conclusão do tratamento</returns>
    Task<Unit> Handle(TRequest request, TException exception, RequestExceptionHandlerState<Unit> state, CancellationToken cancellationToken);
}