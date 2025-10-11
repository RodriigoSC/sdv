using FluentValidation;
using SDV.Domain.Entities.Messages;
using SDV.Domain.Exceptions;
using SDV.Domain.Specification.Interfaces;

namespace SDV.Domain.Specification;

public class MessageValidationSpecification : IValidationSpecification<Message>
{
    public void IsValid(Message entity)
    {
        var validator = new MessageValidator();
        var result = validator.Validate(entity);

        if (!result.IsValid)
            throw new EntityValidationException(
                nameof(Message),
                string.Join(" e ", result.Errors.Select(x => x.ErrorMessage))
            );
    }

    private class MessageValidator : AbstractValidator<Message>
    {
        public MessageValidator()
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
            RuleForEach(x => x.Messages)
                .Must(h => !string.IsNullOrWhiteSpace(h.Content))
                .WithMessage("O conteúdo da mensagem não pode ser vazio.")
                .Must(h => h.Date != default)
                .WithMessage("A data da mensagem deve ser válida.");

            // Opcional: verificar duplicidade de datas
            RuleFor(x => x.Messages)
                .Must(h => h.Select(f => f.Date.Date).Distinct().Count() == h.Count)
                .WithMessage("Não pode haver mensagens duplicadas na mesma data.");
        }
    }
}
