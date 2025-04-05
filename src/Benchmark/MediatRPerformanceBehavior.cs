using MediatR;
using System.Diagnostics;

namespace Benchmark;

public class MediatRPerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await next();
        stopwatch.Stop();

        Console.WriteLine($"[MediatR] {typeof(TRequest).Name} executed in {stopwatch.ElapsedMilliseconds}ms");
        return response;
    }
}
