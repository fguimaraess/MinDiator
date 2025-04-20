using Microsoft.Extensions.DependencyInjection;
using MinDiator.Handlers.Base;
using MinDiator.Interfaces;
using Moq;

namespace MinDiator.Tests.Handlers.Base
{
    public class NotificationHandlerWrapperTests
    {
        public class SampleNotification : INotification { }

        [Fact]
        public async Task Handle_ShouldInvokeAllRegisteredHandlers()
        {
            // Arrange
            var mockHandler1 = new Mock<INotificationHandler<SampleNotification>>();
            var mockHandler2 = new Mock<INotificationHandler<SampleNotification>>();

            var services = new ServiceCollection()
                .AddSingleton(mockHandler1.Object)
                .AddSingleton(mockHandler2.Object)
                .BuildServiceProvider();

            var wrapper = new StaticCachedNotificationHandlerWrapper<SampleNotification>();
            var notification = new SampleNotification();

            // Act
            await wrapper.Handle(notification, services, CancellationToken.None);

            // Assert
            mockHandler1.Verify(h => h.Handle(notification, CancellationToken.None), Times.Once);
            mockHandler2.Verify(h => h.Handle(notification, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldNotFailWhenNoHandlersRegistered()
        {
            // Arrange
            var services = new ServiceCollection().BuildServiceProvider();
            var wrapper = new StaticCachedNotificationHandlerWrapper<SampleNotification>();

            // Act & Assert
            await wrapper.Handle(new SampleNotification(), services, CancellationToken.None);
        }
    }
}
