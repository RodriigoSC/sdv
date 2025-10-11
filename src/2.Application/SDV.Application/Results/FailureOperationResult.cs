using System;

namespace SDV.Application.Results;

public class FailureOperationResult
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
}
