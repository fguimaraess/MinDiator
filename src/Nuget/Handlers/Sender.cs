using MinDiator.Interfaces;

namespace MinDiator;
public class Sender : ISender
{
    private readonly IMediator _mediator;
    public Sender(IMediator mediator)
    {
        _mediator = mediator;
    }

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        return _mediator.Send(request, cancellationToken);
    }
}
