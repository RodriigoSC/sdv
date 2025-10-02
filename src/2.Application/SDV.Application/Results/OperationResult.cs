using System;
using SDV.Application.Dtos.Clients;

namespace SDV.Application.Results;

public class OperationResult<T>
{
    public bool IsSuccess { get; private set; }
    public int OperationCode { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public T? Data { get; private set; }

    private OperationResult() { }

    // Cria um resultado de sucesso
    public static OperationResult<T> Succeeded(T? data, string message = "", int operationCode = 200)
    {
        return new OperationResult<T>
        {
            IsSuccess = true,
            Data = data,
            Message = message,
            OperationCode = operationCode
        };
    }

    // Cria um resultado de falha
    public static OperationResult<T> Failed(T? data, string message, int operationCode = 400)
    {
        return new OperationResult<T>
        {
            IsSuccess = false,
            Data = data,
            Message = message,
            OperationCode = operationCode
        };
    }
    
}
