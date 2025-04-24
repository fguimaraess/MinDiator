using MinDiator.Handlers;
using MinDiator.Interfaces;
using SampleMinimalAPI.Behavior.Services;
using System.Diagnostics;

namespace SampleAPI.Behavior;

public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
    private readonly IServiceBehavior _serviceBehavior;
    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger, IServiceBehavior serviceBehavior)
    {
        _logger = logger;
        _serviceBehavior = serviceBehavior;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var timer = Stopwatch.StartNew();
        var okMessage = await _serviceBehavior.GetString();

        var response = await next();
        timer.Stop();
        var elapsedMilliseconds = timer.ElapsedMilliseconds;
        if (elapsedMilliseconds > 500)
        {
            var requestName = typeof(TRequest).Name;
            _logger.LogInformation($"Long running request: {requestName} ({elapsedMilliseconds} milliseconds) {request}");
        }

        return response;
    }
}
