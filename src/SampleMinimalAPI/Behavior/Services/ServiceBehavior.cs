
namespace SampleMinimalAPI.Behavior.Services;

public class ServiceBehavior : IServiceBehavior
{
    public async Task<string> GetString()
    {
        return "OK";
    }
}
