using System;
using FluentValidation;

namespace SDV.Domain.Specification.Validations;

public class EnumerableHasValueValidation : AbstractValidator<string>
{
    public EnumerableHasValueValidation(string field)
    {
        RuleFor(x => x)
            .NotEmpty().WithMessage(x => $"O campo {field}, não pode ser vazio");
    }

}