namespace SampleMinimalAPI.Behavior.Services;

public interface IServiceBehavior
{
    Task<string> GetString();
}
