using System;
using SDV.Application.Results;

namespace SDV.Api.Middlewares;

public class ApiResponse<T>
{
    public int StatusCode { get; set; }
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    // Criação a partir de OperationResult<T>
    public static ApiResponse<T> Create(int statusCode, OperationResult<T> operationResult)
    {
        if (operationResult == null)
            throw new ArgumentNullException(nameof(operationResult));

        return new ApiResponse<T>
        {
            StatusCode = statusCode,
            IsSuccess = operationResult.IsSuccess,
            Message = operationResult.Message,
            Data = operationResult.Data
        };
    }

    // Criação para falha sem dados (pode ser usado em BaseController)
    public static ApiResponse<T> Failure(string message, int statusCode = 400)
    {
        return new ApiResponse<T>
        {
            StatusCode = statusCode,
            IsSuccess = false,
            Message = message,
            Data = default
        };
    }
}
