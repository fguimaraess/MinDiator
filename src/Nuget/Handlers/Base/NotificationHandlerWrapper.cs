using Microsoft.Extensions.DependencyInjection;
using MinDiator.Interfaces;

namespace MinDiator.Handlers.Base
{
    /// <summary>
    /// Base class for notification handlers
    /// </summary>
    public abstract class NotificationHandlerBase
    {
        /// <summary>
        /// Handles the notification with the appropriate handler
        /// </summary>
        /// <param name="notification">The notification to be handled</param>
        /// <param name="serviceProvider">The service provider for resolving handlers</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the notification handling</returns>
        public abstract Task Handle(object notification, IServiceProvider serviceProvider, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Wrapper for notification handlers that handles resolving and executing handlers
    /// </summary>
    /// <typeparam name="TNotification">The notification type</typeparam>
    public abstract class NotificationHandlerWrapper<TNotification> : NotificationHandlerBase
        where TNotification : INotification
    {
        /// <summary>
        /// Handles the notification with all registered handlers
        /// </summary>
        /// <param name="notification">The notification to be handled</param>
        /// <param name="serviceProvider">The service provider for resolving handlers</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the notification handling</returns>
        public override Task Handle(object notification, IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            return Handle((TNotification)notification, serviceProvider, cancellationToken);
        }

        /// <summary>
        /// Handles the notification with all registered handlers
        /// </summary>
        /// <param name="notification">The notification to be handled</param>
        /// <param name="serviceProvider">The service provider for resolving handlers</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the notification handling</returns>
        public abstract Task Handle(TNotification notification, IServiceProvider serviceProvider, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Static cached wrapper for notification handlers to improve performance
    /// </summary>
    /// <typeparam name="TNotification">The notification type</typeparam>
    public class StaticCachedNotificationHandlerWrapper<TNotification> : NotificationHandlerWrapper<TNotification>
        where TNotification : INotification
    {
        private static readonly Type _handlerType = typeof(INotificationHandler<TNotification>);

        /// <summary>
        /// Handles the notification with all registered handlers
        /// </summary>
        /// <param name="notification">The notification to be handled</param>
        /// <param name="serviceProvider">The service provider for resolving handlers</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the notification handling</returns>
        public override async Task Handle(TNotification notification, IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            // Usar GetServices em vez de GetService para obter todos os handlers
            var handlers = serviceProvider.GetServices(typeof(INotificationHandler<TNotification>))
                .Cast<INotificationHandler<TNotification>>()
                .ToList();

            if (handlers.Count == 0)
            {
                return; // Sem handlers para esta notificação
            }

            var tasks = new List<Task>();
            foreach (var handler in handlers)
            {
                tasks.Add(handler.Handle(notification, cancellationToken));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }
}
