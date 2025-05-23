﻿using MinDiator.Interfaces;

namespace MinDiator;

/// <summary>
/// Interface principal do Mediator, responsável por enviar requisições aos handlers apropriados
/// </summary>
public interface IMediator
{
    /// <summary>
    /// Envia uma requisição fortemente tipada para o handler apropriado
    /// </summary>
    /// <typeparam name="TResponse">Tipo da resposta esperada</typeparam>
    /// <param name="request">A requisição a ser processada</param>
    /// <param name="cancellationToken">Token de cancelamento para operações assíncronas</param>
    /// <returns>A resposta do handler</returns>
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Envia uma requisição como objeto para o handler apropriado (usado para reflexão)
    /// </summary>
    /// <param name="request">A requisição a ser processada como objeto</param>
    /// <param name="cancellationToken">Token de cancelamento para operações assíncronas</param>
    /// <returns>A resposta do handler como objeto</returns>
    Task<object> Send(object request, CancellationToken cancellationToken = default);
}

public interface ISender
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}