namespace WebPasteleria.Common;

/// <summary>Resultado de una operación de negocio que puede fallar por una regla (no por una excepción).</summary>
public sealed class OperationResult
{
    private OperationResult(bool succeeded, string? error, string? field)
    {
        Succeeded = succeeded;
        Error = error;
        Field = field;
    }

    public bool Succeeded { get; }
    public string? Error { get; }

    /// <summary>Nombre de la propiedad del formulario a la que corresponde el error (opcional).</summary>
    public string? Field { get; }

    public static OperationResult Ok() => new(true, null, null);

    public static OperationResult Fail(string error, string? field = null) => new(false, error, field);
}

/// <inheritdoc cref="OperationResult"/>
public sealed class OperationResult<T>
{
    private OperationResult(bool succeeded, T? value, string? error, string? field)
    {
        Succeeded = succeeded;
        Value = value;
        Error = error;
        Field = field;
    }

    public bool Succeeded { get; }
    public T? Value { get; }
    public string? Error { get; }
    public string? Field { get; }

    public static OperationResult<T> Ok(T value) => new(true, value, null, null);

    public static OperationResult<T> Fail(string error, string? field = null) => new(false, default, error, field);
}
