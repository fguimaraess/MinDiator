using MinDiator.Interfaces;
using System.Diagnostics;

namespace Benchmark;

public class MinDiatorPerformanceBehavior<TRequest, TResponse> : MinDiator.Interfaces.IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, MinDiator.Handlers.RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await next();
        stopwatch.Stop();
        var requestName = request.GetType().Name;

        Console.WriteLine($"[MinDiator] {requestName} executed in {stopwatch.ElapsedMilliseconds}ms");
        return response;
    }
}