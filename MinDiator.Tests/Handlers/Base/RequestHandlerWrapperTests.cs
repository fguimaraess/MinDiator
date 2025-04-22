using Microsoft.Extensions.DependencyInjection;
using MinDiator.Configuration;
using MinDiator.Handlers;
using MinDiator.Handlers.Base;
using MinDiator.Interfaces;
using Moq;

namespace MinDiator.Tests.Handlers.Base
{
    public class RequestHandlerWrapperTests
    {
        public class SampleRequest : IRequest<string> { }

        [Fact]
        public async Task Handle_ShouldReturnResponseWithoutBehaviors()
        {
            // Arrange
            var expectedResponse = "response";
            var mockHandler = new Mock<IRequestHandler<SampleRequest, string>>();
            mockHandler.Setup(h => h.Handle(It.IsAny<SampleRequest>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(expectedResponse);

            var services = new ServiceCollection()
                .AddSingleton(mockHandler.Object)
                .BuildServiceProvider();

            var wrapper = new StaticCachedHandlerWrapper<SampleRequest, string>();
            var request = new SampleRequest();

            // Act
            var result = await wrapper.Handle(request, services, CancellationToken.None);

            // Assert
            Assert.Equal(expectedResponse, result);
        }

        public class OrderedBehaviorBase : IPipelineBehavior<SampleRequest, string>
        {
            private readonly string _id;
            private readonly Action<string> _execution;

            protected OrderedBehaviorBase(string id, Action<string> execution)
            {
                _id = id;
                _execution = execution;
            }

            public Task<string> Handle(SampleRequest request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
            {
                _execution(_id + ">");
                return next(cancellationToken).ContinueWith(t =>
                {
                    _execution("<" + _id);
                    return t.Result;
                });
            }
        }

        [PipelineOrder(2)]
        public class Behavior2 : OrderedBehaviorBase
        {
            public Behavior2(Action<string> execution) : base("B2", execution) { }
        }

        [PipelineOrder(1)]
        public class Behavior1 : OrderedBehaviorBase
        {
            public Behavior1(Action<string> execution) : base("B1", execution) { }
        }

        [Fact]
        public async Task Handle_ShouldUsePipelineBehaviorsInOrder()
        {
            // Arrange
            var executionLog = "";

            var mockHandler = new Moq.Mock<IRequestHandler<SampleRequest, string>>();
            mockHandler.Setup(h => h.Handle(It.IsAny<SampleRequest>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(() =>
                       {
                           executionLog += "H";
                           return "done";
                       });

            var services = new ServiceCollection()
                .AddSingleton<IRequestHandler<SampleRequest, string>>(mockHandler.Object)
                .AddSingleton<IPipelineBehavior<SampleRequest, string>>(new Behavior2(s => executionLog += s))
                .AddSingleton<IPipelineBehavior<SampleRequest, string>>(new Behavior1(s => executionLog += s))
                .BuildServiceProvider();

            var wrapper = new StaticCachedHandlerWrapper<SampleRequest, string>();
            var request = new SampleRequest();

            // Act
            var result = await wrapper.Handle(request, services, CancellationToken.None);

            // Assert
            Assert.Equal("B1>B2>H<B2<B1", executionLog);
            Assert.Equal("done", result);
        }

        [Fact]
        public async Task Handle_ShouldThrowWhenNoHandlerIsRegistered()
        {
            // Arrange
            var services = new ServiceCollection().BuildServiceProvider();
            var wrapper = new StaticCachedHandlerWrapper<SampleRequest, string>();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                wrapper.Handle(new SampleRequest(), services, CancellationToken.None));

            Assert.Contains("Handler of type", exception.Message);
        }
    }
}
