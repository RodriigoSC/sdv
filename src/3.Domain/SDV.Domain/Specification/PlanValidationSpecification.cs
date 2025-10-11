using FluentValidation;
using SDV.Domain.Entities.Plans;
using SDV.Domain.Exceptions;
using SDV.Domain.Specification.Interfaces;

namespace SDV.Domain.Specification;

public class PlanValidationSpecification : IValidationSpecification<Plan>
{
    public void IsValid(Plan entity)
    {
        var validator = new PlanValidator();
        var result = validator.Validate(entity);

        if (!result.IsValid)
            throw new EntityValidationException(
                nameof(Plan),
                string.Join(" e ", result.Errors.Select(x => x.ErrorMessage))
            );
    }

    private class PlanValidator : AbstractValidator<Plan>
    {
        public PlanValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("O campo 'Name' não pode estar vazio.")
                .MinimumLength(3).WithMessage("O campo 'Name' deve ter no mínimo 3 caracteres.")
                .MaximumLength(100).WithMessage("O campo 'Name' deve ter no máximo 100 caracteres.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("O campo 'Description' não pode estar vazio.")
                .MaximumLength(200).WithMessage("O campo 'Description' deve ter no máximo 200 caracteres.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("O campo 'Price' deve ser maior que zero.");

            RuleFor(x => x.PlanType)
                .IsInEnum().WithMessage("O campo 'PlanType' é inválido.");
        }
    }
}