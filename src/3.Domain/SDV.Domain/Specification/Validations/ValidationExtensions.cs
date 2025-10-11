using System;
using System.Globalization;
using FluentValidation;

namespace SDV.Domain.Specification.Validations;

public static class ValidationExtensions
{
    /// <summary>
    /// Valida se a string fornecida é um código de cultura válido no .NET.
    /// </summary>
    public static IRuleBuilderOptions<T, string> IsValidCulture<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder.Must(cultureCode =>
        {
            if (string.IsNullOrEmpty(cultureCode))
            {
                // A regra NotEmpty() deve tratar isso, mas é uma segurança adicional.
                return false;
            }
            try
            {
                // Tenta criar um CultureInfo. Se falhar, lança exceção.
                _ = new CultureInfo(cultureCode);
                return true;
            }
            catch (CultureNotFoundException)
            {
                return false;
            }
        }).WithMessage("O código de cultura '{PropertyValue}' não é válido.");
    }
}
