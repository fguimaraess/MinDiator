using MinDiator.Interfaces;

namespace MinDiator.Tests.Handlers
{
    #region Test Classes

    public class TestRequest : IRequest<TestResponse>
    {
    }

    public class TestResponse
    {
        public string Value { get; set; } = "Test Response";
    }

    public class InvalidRequest
    {
        // Doesn't implement IRequest<>
    }

    public class TestNotification : INotification
    {
    }

    public class InvalidNotification : INotification
    {
        // Implementation that would cause issues in the real code
    }

    #endregion
}
