using Moq;

namespace MinDiator.Tests.Handlers
{
    public class SenderTests
    {
        [Fact]
        public async Task Send_DelegatesCallToMediator()
        {
            // Arrange
            var mockMediator = new Mock<IMediator>();
            var request = new TestRequest();
            var expectedResponse = new TestResponse { Value = "Test Response" };

            mockMediator.Setup(m => m.Send(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var sender = new Sender(mockMediator.Object);

            // Act
            var response = await sender.Send(request);

            // Assert
            Assert.Equal(expectedResponse, response);
            mockMediator.Verify(m => m.Send(request, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Send_WithCancellationToken_DelegatesCallToMediator()
        {
            // Arrange
            var mockMediator = new Mock<IMediator>();
            var request = new TestRequest();
            var expectedResponse = new TestResponse { Value = "Test Response" };
            var cancellationToken = new CancellationToken();

            mockMediator.Setup(m => m.Send(request, cancellationToken))
                .ReturnsAsync(expectedResponse);

            var sender = new Sender(mockMediator.Object);

            // Act
            var response = await sender.Send(request, cancellationToken);

            // Assert
            Assert.Equal(expectedResponse, response);
            mockMediator.Verify(m => m.Send(request, cancellationToken), Times.Once);
        }
    }
}
