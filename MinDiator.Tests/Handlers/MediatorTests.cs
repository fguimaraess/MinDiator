using Microsoft.Extensions.DependencyInjection;
using MinDiator.Handlers;
using MinDiator.Interfaces;
using Moq;

namespace MinDiator.Tests.Handlers
{
    public class MediatorTests
    {
        [Fact]
        public async Task Send_Generic_WithValidRequest_CallsHandler()
        {
            // Arrange
            var serviceProvider = CreateServiceProviderWithHandler<TestRequest, TestResponse>();
            var mediator = new Mediator(serviceProvider);
            var request = new TestRequest();

            // Act
            var response = await mediator.Send(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal("Test Response", response.Value);
        }

        [Fact]
        public async Task Send_Object_WithValidRequest_CallsHandler()
        {
            // Arrange
            var serviceProvider = CreateServiceProviderWithHandler<TestRequest, TestResponse>();
            var mediator = new Mediator(serviceProvider);
            object request = new TestRequest();

            // Act
            var response = await mediator.Send(request);

            // Assert
            Assert.NotNull(response);
            Assert.IsType<TestResponse>(response);
            Assert.Equal("Test Response", ((TestResponse)response).Value);
        }

        [Fact]
        public async Task Send_Generic_WithCachedHandler_ReusesHandler()
        {
            // Arrange
            var serviceProvider = CreateServiceProviderWithHandler<TestRequest, TestResponse>();
            var mediator = new Mediator(serviceProvider);
            var request = new TestRequest();

            // Act
            // Call twice to test caching
            var response1 = await mediator.Send(request);
            var response2 = await mediator.Send(request);

            // Assert
            Assert.NotNull(response1);
            Assert.NotNull(response2);
            Assert.Equal("Test Response", response1.Value);
            Assert.Equal("Test Response", response2.Value);
        }

        [Fact]
        public void Send_Generic_WithNullRequest_ThrowsArgumentNullException()
        {
            // Arrange
            var serviceProvider = CreateServiceProviderWithHandler<TestRequest, TestResponse>();
            var mediator = new Mediator(serviceProvider);

            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(() => mediator.Send<TestResponse>(null));
        }

        [Fact]
        public void Send_Object_WithNullRequest_ThrowsArgumentNullException()
        {
            // Arrange
            var serviceProvider = CreateServiceProviderWithHandler<TestRequest, TestResponse>();
            var mediator = new Mediator(serviceProvider);

            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(() => mediator.Send(null));
        }

        [Fact]
        public void Send_Object_WithInvalidRequest_ThrowsInvalidOperationException()
        {
            // Arrange
            var serviceProvider = CreateServiceProviderWithHandler<TestRequest, TestResponse>();
            var mediator = new Mediator(serviceProvider);
            object request = new InvalidRequest(); // Doesn't implement IRequest<>

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(() => mediator.Send(request));
        }

        private IServiceProvider CreateServiceProviderWithHandler<TRequest, TResponse>()
            where TRequest : IRequest<TResponse>
        {
            var services = new ServiceCollection();

            // Register request handler
            services.AddTransient<IRequestHandler<TRequest, TResponse>>(sp =>
            {
                var mock = new Mock<IRequestHandler<TRequest, TResponse>>();
                mock.Setup(h => h.Handle(It.IsAny<TRequest>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(Activator.CreateInstance<TResponse>());
                return mock.Object;
            });

            return services.BuildServiceProvider();
        }
    }
}
