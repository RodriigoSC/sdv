using FluentValidation;
using SDV.Domain.Entities.Agendas;
using SDV.Domain.Exceptions;
using SDV.Domain.Specification.Interfaces;
using SDV.Domain.Specification.Validations;

namespace SDV.Domain.Specification;

public class AgendaValidationSpecification : IValidationSpecification<Agenda>
{
    public void IsValid(Agenda entity)
    {
        var validator = new AgendaValidator();
        var result = validator.Validate(entity);

        if (!result.IsValid)
            throw new EntityValidationException(
                nameof(Agenda),
                string.Join(" e ", result.Errors.Select(x => x.ErrorMessage))
            );
    }

    private class AgendaValidator : AbstractValidator<Agenda>
    {
        public AgendaValidator()
        {
            // ClientId
            RuleFor(x => x.ClientId)
                .NotEmpty().WithMessage("O campo 'ClientId' é obrigatório.");

            // Title
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("O campo 'Title' não pode estar vazio.")
                .MinimumLength(3).WithMessage("O campo 'Title' deve ter no mínimo 3 caracteres.")
                .MaximumLength(100).WithMessage("O campo 'Title' deve ter no máximo 100 caracteres.");

            // AgendaType
            RuleFor(x => x.AgendaType)
                .IsInEnum().WithMessage("O campo 'AgendaType' está inválido.");

            // Configuration
            RuleFor(x => x.Configuration)
                .NotNull().WithMessage("A 'Configuration' não pode ser nula.");

            RuleFor(x => x.Configuration.Culture)
                .NotEmpty().WithMessage("O campo 'Culture' é obrigatório.")
                .IsValidCulture();

            // Season
            RuleFor(x => x.Season)
                .NotNull().WithMessage("A 'Season' não pode ser nula.");

            // Templates (opcionais, mas se vierem, não podem ser Guid.Empty)
            RuleFor(x => x.MessageTemplateId)
                .Must(t => t == null || t != Guid.Empty)
                .WithMessage("O campo 'MessageTemplateId' é inválido.");

            RuleFor(x => x.CalendarTemplateId)
                .Must(t => t == null || t != Guid.Empty)
                .WithMessage("O campo 'CalendarTemplateId' é inválido.");
        }        
    }   
    
}

