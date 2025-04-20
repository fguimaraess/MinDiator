using Microsoft.Extensions.DependencyInjection;
using MinDiator.Configuration;
using MinDiator.Interfaces;

namespace MinDiator.Handlers.Base
{
    /// <summary>
    /// Base class to all request handlers.
    /// </summary>
    public abstract class RequestHandlerBase
    {
        /// <summary>
        /// Handler method that will be called by the mediator.
        /// </summary>
        /// <param name="request">Request object.</param>
        /// <param name="serviceProvider">Service provider.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing an operation that will return an object.</returns>
        public abstract Task<object> Handle(object request, IServiceProvider serviceProvider,
            CancellationToken cancellationToken);
    }

    /// <summary>
    /// Wrapper for request handlers, used to define a class with concrete type (which will be used for caching).
    /// </summary>
    /// <typeparam name="TResponse">Response type for this handler.</typeparam>
    public abstract class RequestHandlerWrapper<TResponse> : RequestHandlerBase
    {
        /// <summary>
        /// Handler method that will be called by the mediator.
        /// </summary>
        /// <param name="request">Request object.</param>
        /// <param name="serviceProvider">Service provider.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing an operation that will return an object of type <typeparamref name="TResponse"/>.</returns>
        public abstract Task<TResponse> Handle(IRequest<TResponse> request, IServiceProvider serviceProvider,
            CancellationToken cancellationToken);
    }

    /// <summary>
    /// Implementation of a request handler wrapper that uses static caching for handler and behavior types.
    /// </summary>
    /// <typeparam name="TRequest">Request type.</typeparam>
    /// <typeparam name="TResponse">Response type.</typeparam>
    public class StaticCachedHandlerWrapper<TRequest, TResponse> : RequestHandlerWrapper<TResponse>
        where TRequest : IRequest<TResponse>
    {
        // Cache for handler and behavior types, to avoid reflection on every request.
        private static readonly Type _handlerType = typeof(IRequestHandler<TRequest, TResponse>);
        private static readonly Type _behaviorType = typeof(IPipelineBehavior<TRequest, TResponse>);

        /// <inheritdoc/>
        public override async Task<object> Handle(object request, IServiceProvider serviceProvider,
            CancellationToken cancellationToken)
        {
            return await Handle((IRequest<TResponse>)request, serviceProvider, cancellationToken);
        }

        /// <inheritdoc/>
        public override Task<TResponse> Handle(IRequest<TResponse> request, IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            var handler = (IRequestHandler<TRequest, TResponse>)serviceProvider.GetService(_handlerType);
            if (handler == null)
            {
                throw new InvalidOperationException(
                    $"Handler of type {_handlerType.Name} not found for request {typeof(TRequest).Name}");
            }

            RequestHandlerDelegate<TResponse> pipeline = (ct) => handler.Handle((TRequest)request, ct);

            var behaviors = serviceProvider.GetServices(_behaviorType)
                .Cast<IPipelineBehavior<TRequest, TResponse>>()
                .OrderBy(b =>
                {
                    var attr = b.GetType().GetCustomAttributes(typeof(PipelineOrderAttribute), true)
                                .FirstOrDefault() as PipelineOrderAttribute;
                    return attr?.Order ?? int.MaxValue; // Defaults to last if no attribute is found
                })
                .ToArray();

            for (int i = behaviors.Length - 1; i >= 0; i--)
            {
                var behavior = behaviors[i];
                var next = pipeline;
                pipeline = (ct) => behavior.Handle((TRequest)request, next, ct);
            }

            return pipeline(cancellationToken);
        }
    }
}
