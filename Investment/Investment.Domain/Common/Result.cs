namespace Investment.Domain.Common;

/// <summary>
/// Representa o resultado de uma operação sem retorno de dados
/// </summary>
public class Result
{
    public bool IsSuccess { get; protected set; }
    public bool IsFailure => !IsSuccess;
    public List<string> Errors { get; protected set; }
    public Dictionary<string, List<string>> ValidationErrors { get; protected set; }

    protected Result(bool isSuccess, List<string> errors, Dictionary<string, List<string>> validationErrors)
    {
        IsSuccess = isSuccess;
        Errors = errors ?? new List<string>();
        ValidationErrors = validationErrors ?? new Dictionary<string, List<string>>();
    }

    /// <summary>
    /// Cria um resultado de sucesso
    /// </summary>
    public static Result Success()
    {
        return new Result(true, new List<string>(), new Dictionary<string, List<string>>());
    }

    /// <summary>
    /// Cria um resultado de falha com uma mensagem de erro
    /// </summary>
    public static Result Failure(string error)
    {
        return new Result(false, new List<string> { error }, new Dictionary<string, List<string>>());
    }

    /// <summary>
    /// Cria um resultado de falha com múltiplas mensagens de erro
    /// </summary>
    public static Result Failure(List<string> errors)
    {
        return new Result(false, errors, new Dictionary<string, List<string>>());
    }

    /// <summary>
    /// Cria um resultado de falha com erros de validação
    /// </summary>
    public static Result Failure(Dictionary<string, List<string>> validationErrors)
    {
        return new Result(false, new List<string>(), validationErrors);
    }

    /// <summary>
    /// Cria um resultado de falha com mensagens de erro e erros de validação
    /// </summary>
    public static Result Failure(List<string> errors, Dictionary<string, List<string>> validationErrors)
    {
        return new Result(false, errors, validationErrors);
    }

    /// <summary>
    /// Adiciona um erro ao resultado
    /// </summary>
    public Result AddError(string error)
    {
        Errors.Add(error);
        IsSuccess = false;
        return this;
    }

    /// <summary>
    /// Adiciona um erro de validação ao resultado
    /// </summary>
    public Result AddValidationError(string field, string error)
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
