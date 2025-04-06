using Microsoft.Extensions.DependencyInjection;
using MinDiator.Configuration;
using MinDiator.Entities;
using MinDiator.Handlers;
using MinDiator.Interfaces;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace MinDiator;

/// <summary>
/// Implementação principal do Mediator, responsável por enviar requisições aos handlers apropriados
/// e coordenar a execução dos behaviors e tratamento de exceções
/// </summary>
public class Mediator : IMediator, ISender
{
    private readonly IServiceProvider _serviceProvider;
    private static readonly ConcurrentDictionary<Type, Type> _handlerTypeCache = new();
    private static readonly ConcurrentDictionary<Type, Func<object, object, CancellationToken, Task<object>>> _handlerDelegates = new();
    private static readonly ConcurrentDictionary<Type, (IReadOnlyList<object> Behaviors, Delegate HandleDelegate)> _pipelineCache = new();

    /// <summary>
    /// Construtor que recebe o provedor de serviços para resolução de handlers e behaviors
    /// </summary>
    /// <param name="serviceProvider">Provedor de serviços para resolução de dependências</param>
    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Envia uma requisição fortemente tipada para o handler apropriado
    /// </summary>
    /// <typeparam name="TResponse">Tipo da resposta esperada</typeparam>
    /// <param name="request">A requisição a ser processada</param>
    /// <param name="cancellationToken">Token de cancelamento para operações assíncronas</param>
    /// <returns>A resposta do handler</returns>
    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();

        try
        {
            // Criar o pipeline com os behaviors e executar a requisição
            return await CreateRequestPipeline<TResponse>(request, cancellationToken);
        }
        catch (Exception ex)
        {
            // Lidar com a exceção usando IRequestExceptionHandler, se disponível
            var actualException = ex.InnerException ?? ex;
            var exceptionType = actualException.GetType();
            var responseType = typeof(TResponse);

            // Tentar encontrar os handlers de exceção específicos para este request e tipo de exceção
            var exceptionHandlerType = typeof(IRequestExceptionHandler<,,>).MakeGenericType(requestType, responseType, exceptionType);
            var exceptionHandlers = _serviceProvider.GetServices(exceptionHandlerType).ToList();

            if (exceptionHandlers.Any())
            {
                var exceptionHandlerState = new RequestExceptionHandlerState<TResponse>();

                // Tentar cada handler de exceção registrado até que um deles trate a exceção
                foreach (var exceptionHandler in exceptionHandlers)
                {
                    var handleMethod = exceptionHandlerType.GetMethod("Handle");
                    var handleTask = (Task<Unit>)handleMethod.Invoke(exceptionHandler, new object[] { request, actualException, exceptionHandlerState, cancellationToken });
                    await handleTask;

                    // Se o handler tratou a exceção, retornar a resposta fornecida
                    if (exceptionHandlerState.Handled)
                    {
                        return exceptionHandlerState.Response;
                    }
                }
            }

            // Se nenhum handler de exceção tratou, relançar a exceção
            throw;
        }
    }

    /// <summary>
    /// Cria o pipeline de requisição com todos os behaviors registrados
    /// </summary>
    /// <typeparam name="TResponse">Tipo da resposta esperada</typeparam>
    /// <param name="request">A requisição a ser processada</param>
    /// <param name="cancellationToken">Token de cancelamento para operações assíncronas</param>
    /// <returns>A resposta após a execução do pipeline</returns>
    private async Task<TResponse> CreateRequestPipeline<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
    {
        var requestType = request.GetType();
        var pipelineInfo = _pipelineCache.GetOrAdd(
            requestType,
            _ => BuildPipeline<TResponse>(requestType));

        if (pipelineInfo.Behaviors.Count == 0)
            return await ExecuteHandler(request, cancellationToken);

        //Inicializa o pipeline com o handler final
        RequestHandlerDelegate<TResponse> pipeline = () => ExecuteHandler(request, cancellationToken);

        foreach (var behavior in pipelineInfo.Behaviors.Cast<dynamic>().Reverse())
        {
            var currentPipeline = pipeline;
            pipeline = () => behavior.Handle((dynamic)request, currentPipeline, cancellationToken);
        }

        return await pipeline();
    }

    private (IReadOnlyList<object> Behaviors, Delegate HandleDelegate) BuildPipeline<TResponse>(Type requestType)
    {
        var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, typeof(TResponse));
        var behaviors = _serviceProvider.GetServices(behaviorType)
                                        .OrderBy(b => b.GetType().GetCustomAttribute<PipelineOrderAttribute>()?.Order ?? int.MaxValue)
                                        .ToList();

        return (behaviors, null);
    }

    /// <summary>
    /// Executa o handler apropriado para a requisição
    /// </summary>
    /// <typeparam name="TResponse">Tipo da resposta esperada</typeparam>
    /// <param name="request">A requisição a ser processada</param>
    /// <param name="cancellationToken">Token de cancelamento para operações assíncronas</param>
    /// <returns>A resposta do handler</returns>
    private async Task<TResponse> ExecuteHandler<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
    {
        var requestType = request.GetType();
        var responseType = typeof(TResponse);

        // Obter o handler
        var handlerType = GetHandlerType(requestType);
        var handler = GetHandler(_serviceProvider, handlerType);

        if (handler == null)
        {
            throw new InvalidOperationException($"Não foi encontrado um handler para {requestType.Name}");
        }

        // Executar o handler
        var handlerDelegate = GetHandlerDelegate(handler.GetType());
        var result = await handlerDelegate(handler, request, cancellationToken);

        return (TResponse)result;
    }


    private static Type GetHandlerType(Type requestType)
    {
        return _handlerTypeCache.GetOrAdd(requestType, static type =>
        {
            //Encontrar a interface
            var requestInterface = type
                                    .GetInterfaces()
                                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>))
                                    ?? throw new InvalidOperationException($"Tipo {type} não implementa IRequest<>");

            var responseType = requestInterface.GetGenericArguments()[0];

            return typeof(IRequestHandler<,>).MakeGenericType(type, responseType);
        });
    }

    private static object GetHandler(IServiceProvider serviceProvider, Type handlerType)
    {
        return serviceProvider.GetRequiredService(handlerType);
    }

    private static Func<object, object, CancellationToken, Task<object>> GetHandlerDelegate(Type handlerType)
    {
        return _handlerDelegates.GetOrAdd(handlerType, type =>
        {
            //Encontrar a interface
            var interfaceType = type.GetInterfaces()
                                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
                                    ?? throw new InvalidOperationException($"Tipo {type} não implementa IRequestHandler");

            var requestType = interfaceType.GetGenericArguments()[0];
            var responseType = interfaceType.GetGenericArguments()[1];

            var method = interfaceType.GetMethod("Handle");

            var handlerParam = Expression.Parameter(typeof(object), "handler");
            var requestParam = Expression.Parameter(typeof(object), "request");
            var cancellationParam = Expression.Parameter(typeof(CancellationToken), "ct");

            var handlerCast = Expression.Convert(handlerParam, type);
            var requestCast = Expression.Convert(requestParam, requestType);

            var call = Expression.Call(handlerCast, method, requestCast, cancellationParam);

            //Auxilia na conversão de Task<T> em Task<object>
            var wrapperMethod = typeof(Mediator)
            .GetMethod(nameof(ConvertTaskToObjectAsync), BindingFlags.NonPublic | BindingFlags.Static)
            .MakeGenericMethod(responseType);

            var wrappedCall = Expression.Call(wrapperMethod, call);

            return Expression.
                    Lambda<Func<object, object, CancellationToken, Task<object>>>(
                        wrappedCall,
                        handlerParam,
                        requestParam,
                        cancellationParam)
                    .Compile();
        });
    }

    private static async Task<object> ConvertTaskToObjectAsync<T>(Task<T> task)
    {
        return await task.ConfigureAwait(false);
    }

    /// <summary>
    /// Envia uma requisição como objeto para o handler apropriado
    /// Útil para chamadas via reflexão ou quando o tipo exato não é conhecido em tempo de compilação
    /// </summary>
    /// <param name="request">A requisição a ser processada como objeto</param>
    /// <param name="cancellationToken">Token de cancelamento para operações assíncronas</param>
    /// <returns>A resposta do handler como objeto</returns>
    public async Task<object> Send(object request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var requestType = request.GetType();

        try
        {
            // Encontrar o tipo de resposta apropriado
            var responseType = GetResponseType(requestType);

            // Criar o método genérico Send<TResponse>
            var method = typeof(Mediator)
                .GetMethods()
                .First(m => m.Name == nameof(Send) && m.IsGenericMethod);

            var genericMethod = method.MakeGenericMethod(responseType);

            // Invocar o método genérico
            return await (Task<object>)genericMethod.Invoke(this, new[] { request, cancellationToken });
        }
        catch (Exception ex)
        {
            // Preservar a stack trace original
            ExceptionDispatchInfo.Capture(ex.InnerException ?? ex).Throw();
            // Linha abaixo nunca é executada, apenas para compilação
            throw;
        }
    }

    /// <summary>
    /// Obtém o tipo de resposta esperado para um tipo de requisição
    /// </summary>
    /// <param name="requestType">O tipo da requisição</param>
    /// <returns>O tipo da resposta esperada</returns>
    private Type GetResponseType(Type requestType)
    {
        // Encontrar a interface IRequest<TResponse> que o tipo implementa
        var requestInterfaceType = requestType
            .GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType &&
                (i.GetGenericTypeDefinition() == typeof(IRequest<>) || i.GetGenericTypeDefinition() == typeof(IRequest)));

        if (requestInterfaceType == null)
        {
            throw new InvalidOperationException($"O tipo {requestType.Name} não implementa IRequest<> ou IRequest");
        }

        // Se for IRequest, retorna Unit
        if (requestInterfaceType.GetGenericTypeDefinition() == typeof(IRequest))
        {
            return typeof(Unit);
        }

        // Senão, retorna o tipo genérico da resposta
        return requestInterfaceType.GetGenericArguments()[0];
    }
}