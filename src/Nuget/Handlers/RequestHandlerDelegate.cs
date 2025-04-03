namespace MinDiator.Handlers;
/// <summary>
/// Delegate que encapsula a chamada para o próximo handler ou behavior na cadeia
/// </summary>
/// <typeparam name="TResponse">Tipo de resposta esperada</returns>
/// <returns>Task contendo a resposta</returns>
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();
