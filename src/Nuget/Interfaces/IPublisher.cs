namespace MinDiator.Interfaces
{
    /// <summary>
    /// Defines a publisher interface for notifications
    /// </summary>
    public interface IPublisher
    {
        /// <summary>
        /// Publishes a notification to multiple handlers
        /// </summary>
        /// <param name="notification">The notification to be published</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A task representing the publication process</returns>
        Task Publish(INotification notification, CancellationToken cancellationToken = default);

        /// <summary>
        /// Publishes a notification to multiple handlers
        /// </summary>
        /// <typeparam name="TNotification">The notification type</typeparam>
        /// <param name="notification">The notification to be published</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A task representing the publication process</returns>
        Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification;
    }
}
