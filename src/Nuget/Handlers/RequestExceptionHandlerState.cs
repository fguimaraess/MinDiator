namespace MinDiator.Handlers;

/// <summary>
/// Classe que mantém o estado de tratamento de exceção, indicando se foi tratada e qual resposta retornar
/// </summary>
/// <typeparam name="TResponse">Tipo da resposta</typeparam>
public class RequestExceptionHandlerState<TResponse>
{
    private TResponse _response;
    private bool _handled;

    /// <summary>
    /// Indica se a exceção foi tratada e uma resposta alternativa definida
    /// </summary>
    public bool Handled => _handled;

    /// <summary>
    /// Resposta a ser retornada em vez de propagar a exceção
    /// </summary>
    public TResponse Response
    {
        get => _response;
        set
        {
            _response = value;
            _handled = true;
        }
    }

    /// <summary>
    /// Define que a exceção foi tratada e define a resposta a ser retornada
    /// </summary>
    /// <param name="response">Resposta a ser retornada</param>
    public void SetHandled(TResponse response)
    {
        Response = response;
    }
}