using System;

namespace SDV.Api.Models;

/// <summary>
/// Modelo padronizado para respostas de erro
/// </summary>
public class ErrorResponseDto
{
    public string Message { get; set; } = string.Empty;
    public string? Code { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
