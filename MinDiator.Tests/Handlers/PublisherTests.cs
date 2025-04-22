using Microsoft.Extensions.DependencyInjection;
using MinDiator.Handlers;
using MinDiator.Interfaces;
using Moq;

namespace MinDiator.Tests.Handlers
{
    public class PublisherTests
    {
        [Fact]
        public async Task Publish_Generic_WithValidNotification_CallsHandlers()
        {
            // Arrange
            var mockHandler = new Mock<INotificationHandler<TestNotification>>();
            mockHandler.Setup(h => h.Handle(It.IsAny<TestNotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var serviceProvider = CreateServiceProviderWithNotificationHandler(mockHandler.Object);
            var publisher = new Publisher(serviceProvider);
            var notification = new TestNotification();

            // Act
            await publisher.Publish(notification);

            // Assert
            mockHandler.Verify(h => h.Handle(notification, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Publish_Interface_WithValidNotification_CallsHandlers()
        {
            // Arrange
            var mockHandler = new Mock<INotificationHandler<TestNotification>>();
            mockHandler.Setup(h => h.Handle(It.IsAny<TestNotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var serviceProvider = CreateServiceProviderWithNotificationHandler(mockHandler.Object);
            var publisher = new Publisher(serviceProvider);
            INotification notification = new TestNotification();

            // Act
            await publisher.Publish(notification);

            // Assert
            mockHandler.Verify(h => h.Handle(It.IsAny<TestNotification>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Publish_WithMultipleHandlers_CallsAllHandlers()
        {
            // Arrange
            var mockHandler1 = new Mock<INotificationHandler<TestNotification>>();
            var mockHandler2 = new Mock<INotificationHandler<TestNotification>>();

            mockHandler1.Setup(h => h.Handle(It.IsAny<TestNotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            mockHandler2.Setup(h => h.Handle(It.IsAny<TestNotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var services = new ServiceCollection();
            services.AddTransient<INotificationHandler<TestNotification>>(sp => mockHandler1.Object);
            services.AddTransient<INotificationHandler<TestNotification>>(sp => mockHandler2.Object);
            var serviceProvider = services.BuildServiceProvider();

            var publisher = new Publisher(serviceProvider);
            var notification = new TestNotification();

            // Act
            await publisher.Publish(notification);

            // Assert
            mockHandler1.Verify(h => h.Handle(notification, It.IsAny<CancellationToken>()), Times.Once);
            mockHandler2.Verify(h => h.Handle(notification, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void Publish_Generic_WithNullNotification_ThrowsArgumentNullException()
        {
            // Arrange
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            var publisher = new Publisher(serviceProvider);

            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(() => publisher.Publish<TestNotification>(null));
        }

        [Fact]
        public void Publish_Interface_WithNullNotification_ThrowsArgumentNullException()
        {
            // Arrange
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            var publisher = new Publisher(serviceProvider);

            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(() => publisher.Publish(null));
        }

        [Fact]
        public void Publish_Interface_WithInvalidNotification_ThrowsInvalidOperationException()
        {
            // Arrange
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            var publisher = new Publisher(serviceProvider);
            INotification notification = new InvalidNotification();

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(() => publisher.Publish(notification));
        }

        private IServiceProvider CreateServiceProviderWithNotificationHandler<TNotification>(
            INotificationHandler<TNotification> handler)
            where TNotification : INotification
        {
            var services = new ServiceCollection();
            services.AddTransient<INotificationHandler<TNotification>>(sp => handler);
            return services.BuildServiceProvider();
        }
    }    
}
