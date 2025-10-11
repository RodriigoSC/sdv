namespace SDV.Domain.Enums.Clients;

/// <summary>
/// Representa o resultado da verificação de um e-mail.
/// </summary>
public enum EmailVerificationResult
{
    /// <summary>
    /// O e-mail foi verificado com sucesso.
    /// </summary>
    Success,

    /// <summary>
    /// O token de verificação é inválido.
    /// </summary>
    InvalidToken,

    /// <summary>
    /// O token de verificação expirou.
    /// </summary>
    Expired,

    /// <summary>
    /// O e-mail já foi verificado anteriormente.
    /// </summary>
    AlreadyVerified
}