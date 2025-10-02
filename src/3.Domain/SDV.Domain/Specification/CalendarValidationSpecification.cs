using FluentValidation;
using SDV.Domain.Entities.Calendars;
using SDV.Domain.Exceptions;
using SDV.Domain.Specification.Interfaces;

namespace SDV.Domain.Specification;

public class CalendarValidationSpecification : IValidationSpecification<Calendar>
{
    public void IsValid(Calendar entity)
    {
        var validator = new CalendarValidator();
        var result = validator.Validate(entity);

        if (!result.IsValid)
            throw new EntityValidationException(
                nameof(Calendar),
                string.Join(" e ", result.Errors.Select(x => x.ErrorMessage))
            );
    }

    private class CalendarValidator : AbstractValidator<Calendar>
    {
        public CalendarValidator()
        {
            // UserId
            RuleFor(x => x.ClientId)
                .NotEmpty().WithMessage("O campo 'UserId' é obrigatório.");

            // Title
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("O campo 'Title' não pode estar vazio.")
                .MinimumLength(3).WithMessage("O campo 'Title' deve ter no mínimo 3 caracteres.")
                .MaximumLength(100).WithMessage("O campo 'Title' deve ter no máximo 100 caracteres.");

            // Holidays (opcional)
            RuleForEach(x => x.Calendars)
                .Must(h => !string.IsNullOrWhiteSpace(h.Content))
                .WithMessage("O conteúdo do feriado não pode ser vazio.")
                .Must(h => h.Date != default)
                .WithMessage("A data do feriado deve ser válida.");

            // Opcional: verificar duplicidade de datas
            RuleFor(x => x.Calendars)
                .Must(h => h.Select(f => f.Date.Date).Distinct().Count() == h.Count)
                .WithMessage("Não pode haver feriados duplicados na mesma data.");
        }
    }
}
