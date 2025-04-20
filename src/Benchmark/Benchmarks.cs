using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace Benchmark;
[MemoryDiagnoser]
public class Benchmarks
{
    private MediatR.ISender _senderMediatR;
    private MediatR.IMediator _mediatR;
    private MediatR.IPublisher _publisherMediatR;

    private MinDiator.ISender _senderMinDiator;
    private MinDiator.IMediator _minDiator;
    private MinDiator.Interfaces.IPublisher _publisherMinDiator;

    private MediatR.IRequest<string> _requestMediatR = new SampleRequest();
    private MinDiator.Interfaces.IRequest<string> _requestMinDiator = new SampleRequest();

    private MediatR.INotification _notificationMediatR = new SampleNotification();
    private MinDiator.Interfaces.INotification _notificationMinDiator = new SampleNotification();

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

        _publisherMediatR = provider.GetRequiredService<MediatR.IPublisher>();
        _publisherMinDiator = provider.GetRequiredService<MinDiator.Interfaces.IPublisher>();
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

    //[Benchmark]
    //public async Task<string> MediatR_ISender_Send()
    //{
    //    return await _senderMediatR.Send(_requestMediatR);
    //}

    //[Benchmark]
    //public async Task<string> MinDiator_ISender_Send()
    //{
    //    return await _senderMinDiator.Send(_requestMinDiator);
    //}

    //[Benchmark]
    //public async Task MediatR_IPublisher_Publish()
    //{
    //    await _publisherMediatR.Publish(_notificationMediatR);
    //}

    //[Benchmark]
    //public async Task MinDiator_IPublisher_Publish()
    //{
    //    await _publisherMinDiator.Publish(_notificationMinDiator);
    //}
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

// Sample notification/handlers for both
public class SampleNotification : MinDiator.Interfaces.INotification, MediatR.INotification
{
}

public class SampleNotificationHandler1 : MinDiator.Interfaces.INotificationHandler<SampleNotification>, MediatR.INotificationHandler<SampleNotification>
{
    public Task Handle(SampleNotification notification, CancellationToken cancellationToken)
    {
        // Just a simple handler that does nothing
        return Task.CompletedTask;
    }
}

public class SampleNotificationHandler2 : MinDiator.Interfaces.INotificationHandler<SampleNotification>, MediatR.INotificationHandler<SampleNotification>
{
    public Task Handle(SampleNotification notification, CancellationToken cancellationToken)
    {
        // Second handler to test multiple handlers scenario
        return Task.CompletedTask;
    }
}