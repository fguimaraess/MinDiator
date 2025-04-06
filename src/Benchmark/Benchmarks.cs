using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace Benchmark;
[MemoryDiagnoser]
public class Benchmarks
{
    private MediatR.ISender _senderMediatR;
    private MediatR.IMediator _mediatR;

    private MinDiator.ISender _senderMinDiator;
    private MinDiator.IMediator _minDiator;

    private MediatR.IRequest<string> _requestMediatR = new SampleRequest();
    private MinDiator.Interfaces.IRequest<string> _requestMinDiator = new SampleRequest();

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();

        // MediatR setup
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(SampleRequest).Assembly);
            cfg.AddBehavior(typeof(MediatR.IPipelineBehavior<,>), typeof(MediatRPerformanceBehavior<,>));
        });

        // MinDiator setup
        services.AddMinDiator(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(SampleRequest).Assembly);
            cfg.AddBehavior(typeof(MinDiator.Interfaces.IPipelineBehavior<,>), typeof(MinDiatorPerformanceBehavior<,>));
        });

        var provider = services.BuildServiceProvider();
        _mediatR = provider.GetRequiredService<MediatR.IMediator>();
        _minDiator = provider.GetRequiredService<MinDiator.IMediator>();

        _senderMediatR = provider.GetRequiredService<MediatR.ISender>();
        _senderMinDiator = provider.GetRequiredService<MinDiator.ISender>();
    }

    [Benchmark]
    public async Task<string> MediatR_IMediator_Send()
    {
        return await _mediatR.Send(_requestMediatR);
    }

    [Benchmark]
    public async Task<string> MinDiator_IMediator_Send()
    {
        return await _minDiator.Send(_requestMinDiator);
    }

    [Benchmark]
    public async Task<string> MediatR_ISender_Send()
    {
        return await _senderMediatR.Send(_requestMediatR);
    }

    [Benchmark]
    public async Task<string> MinDiator_ISender_Send()
    {
        return await _senderMinDiator.Send(_requestMinDiator);
    }
}

// Sample request/handler for both
public class SampleRequest : MinDiator.Interfaces.IRequest<string>, MediatR.IRequest<string>
{
}

public class SampleHandler : MinDiator.Interfaces.IRequestHandler<SampleRequest, string>, MediatR.IRequestHandler<SampleRequest, string>
{
    public Task<string> Handle(SampleRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult("ok");
    }
}
