using Microsoft.AspNetCore.Mvc;
using SDV.Application.Results;

namespace SDV.Api.Middlewares;

[ApiController]
public class BaseController : ControllerBase
{
    // Cria ApiResponse genérico a partir de OperationResult
    protected static ApiResponse<TReturn> CreateResponseObjectFromOperationResult<TReturn>(
        int statusCode,
        OperationResult<TReturn> operationResult)
    {
        return ApiResponse<TReturn>.Create(statusCode, operationResult);
    }

    // Cria ApiResponse de falha sem dados
    protected ApiResponse<T> CreateFailureResponse<T>(string message, int statusCode = 400)
    {
        return ApiResponse<T>.Failure(message, statusCode);
    }

    // Cria FailureOperationResult simples
    protected FailureOperationResult CreateSimpleFailure(string message, int statusCode = 400)
    {
        return new FailureOperationResult
        {
            Message = message,
            StatusCode = statusCode
        };
    }
}
