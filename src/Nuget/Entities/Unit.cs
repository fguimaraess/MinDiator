﻿namespace MinDiator.Entities;

/// <summary>
/// Estrutura que representa um tipo de retorno vazio (similar ao void para tarefas assíncronas)
/// Usado para requests que não retornam valor
/// </summary>
public readonly struct Unit : IEquatable<Unit>, IComparable<Unit>, IComparable
{
    private static readonly Unit _value = new();

    /// <summary>
    /// Valor padrão estático para Unit
    /// </summary>
    public static ref readonly Unit Value => ref _value;

    /// <summary>
    /// Task pré-criada que retorna Unit para otimização de performance
    /// </summary>
    public static Task<Unit> Task { get; } = System.Threading.Tasks.Task.FromResult(_value);

    public int CompareTo(Unit other) => 0;
    int IComparable.CompareTo(object? obj) => 0;
    public override int GetHashCode() => 0;
    public bool Equals(Unit other) => true;
    public override bool Equals(object? obj) => obj is Unit;
    public static bool operator ==(Unit first, Unit second) => true;
    public static bool operator !=(Unit first, Unit second) => false;
    public override string ToString() => "()";
}
