namespace Investment.Domain.Common;

/// <summary>
/// Representa o resultado de uma operação com retorno de dados
/// </summary>
public class Result<T> : Result
{
    public T? Data { get; private set; }

    private Result(bool isSuccess, T? data, List<string> errors, Dictionary<string, List<string>> validationErrors)
        : base(isSuccess, errors, validationErrors)
    {
        Data = data;
    }

    /// <summary>
    /// Cria um resultado de sucesso com dados
    /// </summary>
    public static Result<T> Success(T data)
    {
        return new Result<T>(true, data, new List<string>(), new Dictionary<string, List<string>>());
    }

    /// <summary>
    /// Cria um resultado de falha com uma mensagem de erro
    /// </summary>
    public new static Result<T> Failure(string error)
    {
        return new Result<T>(false, default, new List<string> { error }, new Dictionary<string, List<string>>());
    }

    /// <summary>
    /// Cria um resultado de falha com múltiplas mensagens de erro
    /// </summary>
    public new static Result<T> Failure(List<string> errors)
    {
        return new Result<T>(false, default, errors, new Dictionary<string, List<string>>());
    }

    /// <summary>
    /// Cria um resultado de falha com erros de validação
    /// </summary>
    public new static Result<T> Failure(Dictionary<string, List<string>> validationErrors)
    {
        return new Result<T>(false, default, new List<string>(), validationErrors);
    }

    /// <summary>
    /// Cria um resultado de falha com mensagens de erro e erros de validação
    /// </summary>
    public new static Result<T> Failure(List<string> errors, Dictionary<string, List<string>> validationErrors)
    {
        return new Result<T>(false, default, errors, validationErrors);
    }

    /// <summary>
    /// Adiciona um erro ao resultado
    /// </summary>
    public new Result<T> AddError(string error)
    {
        Errors.Add(error);
        IsSuccess = false;
        return this;
    }

    /// <summary>
    /// Adiciona um erro de validação ao resultado
    /// </summary>
    public new Result<T> AddValidationError(string field, string error)
    {
        if (!ValidationErrors.ContainsKey(field))
        {
            ValidationErrors[field] = new List<string>();
        }

        ValidationErrors[field].Add(error);
        IsSuccess = false;
        return this;
    }
}
