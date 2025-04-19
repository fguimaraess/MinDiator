namespace MinDiator.Handlers;
/// <summary>
/// Delegate that encapsulates the call to the next handler or behavior in the chain
/// </summary>
/// <typeparam name="TResponse">Expected response type</returns>
/// <param name="cancellationToken">Cancellation token.</param>
/// <returns>Task containing the response</returns>
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>(CancellationToken cancellationToken = default);
