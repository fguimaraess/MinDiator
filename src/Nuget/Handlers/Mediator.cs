using MinDiator.Handlers.Base;
using MinDiator.Interfaces;
using System.Collections.Concurrent;

namespace MinDiator.Handlers
{
    /// <summary>
    /// Mediator implementation that handles requests, responses and notifications with caching wrappers for reduced reflection.
    /// </summary>
    public class Mediator : IMediator
    {
        /// <summary>
        /// The service provider used to resolve dependencies.
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Cache of request handlers.
        /// </summary>
        private static readonly ConcurrentDictionary<Type, RequestHandlerBase> _requestHandlers = new();

        /// <summary>
        /// Cache of notification handlers.
        /// </summary>
        private static readonly ConcurrentDictionary<Type, NotificationHandlerBase> _notificationHandlers = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="Mediator"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
        public Mediator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Sends a request and returns the response of type TResponse.
        /// </summary>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="request">The request to be sent.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The response of type TResponse, wrapped into a Task.</returns>
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var requestType = request.GetType();
            var handler = (RequestHandlerWrapper<TResponse>)_requestHandlers.GetOrAdd(requestType, type =>
            {
                var responseType = typeof(TResponse);
                var wrapperType = typeof(StaticCachedHandlerWrapper<,>).MakeGenericType(type, responseType);

                var wrapper = Activator.CreateInstance(wrapperType)
                    ?? throw new InvalidOperationException($"Could not create wrapper type for {type}");

                return (RequestHandlerBase)wrapper;
            });

            return handler.Handle(request, _serviceProvider, cancellationToken);
        }

        /// <summary>
        /// Sends a request and returns the response as an object.
        /// </summary>
        /// <param name="request">The request to be sent.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The response as an object, wrapped into a Task.</returns>
        public Task<object> Send(object request, CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var requestType = request.GetType();
            var handler = _requestHandlers.GetOrAdd(requestType, type =>
            {
                var requestInterface = type.GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>));

                if (requestInterface == null)
                {
                    throw new InvalidOperationException($"Request type {type.Name} does not implement IRequest<TResponse>");
                }

                var responseType = requestInterface.GetGenericArguments()[0];
                var wrapperType = typeof(StaticCachedHandlerWrapper<,>).MakeGenericType(type, responseType);

                var wrapper = Activator.CreateInstance(wrapperType)
                    ?? throw new InvalidOperationException($"Could not create wrapper type for {type}");

                return (RequestHandlerBase)wrapper;
            });

            return handler.Handle(request, _serviceProvider, cancellationToken);
        }        
    }
}