using System.ComponentModel.DataAnnotations;

namespace SDV.Domain.Enums.Commons;

/// <summary>
/// Define os tipos de arquivo suportados.
/// </summary>
public enum FileType
{
    /// <summary>
    /// Arquivo do tipo CSV (Comma-Separated Values).
    /// </summary>
    [Display(Name = "Arquivo CSV")]
    CSV,

    /// <summary>
    /// Arquivo do tipo TXT (texto plano).
    /// </summary>
    [Display(Name = "Arquivo TXT")]
    TXT
}