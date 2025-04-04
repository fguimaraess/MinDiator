namespace MinDiator.Configuration;

/// <summary>
/// Atributo que define a ordem de execução dos behaviors no pipeline
/// Quanto menor o valor da ordem, mais cedo o behavior será executado
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class PipelineOrderAttribute : Attribute
{
    /// <summary>
    /// Valor que determina a ordem de execução do behavior
    /// </summary>
    public int Order { get; }

    /// <summary>
    /// Construtor que define a ordem de execução do behavior
    /// </summary>
    /// <param name="order">Valor de ordem (menor = executa primeiro)</param>
    public PipelineOrderAttribute(int order)
    {
        Order = order;
    }
}