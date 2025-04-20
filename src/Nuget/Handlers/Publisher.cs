using MinDiator.Handlers.Base;
using MinDiator.Interfaces;
using System.Collections.Concurrent;

namespace MinDiator.Handlers
{
    public class Publisher : IPublisher
    {
        private readonly IServiceProvider _serviceProvider;
        private static readonly ConcurrentDictionary<Type, NotificationHandlerBase> _notificationHandlers = new();

        public Publisher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification
        {
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            var notificationType = notification.GetType();
            var handler = _notificationHandlers.GetOrAdd(notificationType, type =>
            {
                var wrapperType = typeof(StaticCachedNotificationHandlerWrapper<>).MakeGenericType(type);

                var wrapper = Activator.CreateInstance(wrapperType)
                    ?? throw new InvalidOperationException($"Could not create notification wrapper type for {type}");

                return (NotificationHandlerBase)wrapper;
            });

            return handler.Handle(notification, _serviceProvider, cancellationToken);
        }

        public Task Publish(INotification notification, CancellationToken cancellationToken = default)
        {
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            var notificationType = notification.GetType();
            var handler = _notificationHandlers.GetOrAdd(notificationType, type =>
            {
                if (!typeof(INotification).IsAssignableFrom(type))
                {
                    throw new InvalidOperationException($"Notification type {type.Name} does not implement INotification");
                }

                var wrapperType = typeof(StaticCachedNotificationHandlerWrapper<>).MakeGenericType(type);

                var wrapper = Activator.CreateInstance(wrapperType)
                    ?? throw new InvalidOperationException($"Could not create notification wrapper type for {type}");

                return (NotificationHandlerBase)wrapper;
            });

            return handler.Handle(notification, _serviceProvider, cancellationToken);
        }
    }
}
