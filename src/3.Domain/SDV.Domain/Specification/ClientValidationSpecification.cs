using System;
using System.Net.Mail;
using System.Text.RegularExpressions;
using FluentValidation;
using SDV.Domain.Entities.Clients;
using SDV.Domain.Exceptions;
using SDV.Domain.Specification.Interfaces;

namespace SDV.Domain.Specification
{
    public class ClientValidationSpecification : IValidationSpecification<Client>
    {
        public void IsValid(Client entity)
        {
            var validator = new ClientValidator();
            var result = validator.Validate(entity);

            if (!result.IsValid)
                throw new EntityValidationException(
                    nameof(Client),
                    string.Join(" e ", result.Errors.Select(x => x.ErrorMessage))
                );
        }

        private class ClientValidator : AbstractValidator<Client>
        {
            public ClientValidator()
            {
                // Nome
                RuleFor(x => x.Name)
                    .NotEmpty().WithMessage("O campo 'Name' não pode estar vazio.")
                    .MinimumLength(4).WithMessage("O campo 'Name' deve ter no mínimo 4 caracteres.")
                    .MaximumLength(100).WithMessage("O campo 'Name' deve ter no máximo 100 caracteres.");

                // Email (validação usando MailAddress)
                RuleFor(x => x.Email)
                    .NotNull().WithMessage("O campo 'Email' é obrigatório.")
                    .Must(e => MailAddress.TryCreate(e.ToString(), out _))
                    .WithMessage("O campo 'Email' está em formato inválido.");

                // Senha
                RuleFor(x => x.PasswordHash)
                    .NotEmpty().WithMessage("O campo 'Password' não pode estar vazio.")
                    .MinimumLength(6).WithMessage("A senha deve ter no mínimo 6 caracteres.")
                    .MaximumLength(128).WithMessage("A senha deve ter no máximo 128 caracteres.");

                // Telefone (opcional, regex com timeout)
                RuleFor(x => x.PhoneNumber)
                    .Matches(new Regex(@"^\+?[1-9]\d{1,14}$", RegexOptions.None, TimeSpan.FromMilliseconds(200)))
                    .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
                    .WithMessage("O campo 'PhoneNumber' está em formato inválido.");

                // Cargo (opcional)
                RuleFor(x => x.JobTitle)
                    .MaximumLength(50)
                    .When(x => !string.IsNullOrWhiteSpace(x.JobTitle))
                    .WithMessage("O campo 'JobTitle' deve ter no máximo 50 caracteres.");

                // Bio (opcional)
                RuleFor(x => x.Bio)
                    .MaximumLength(200)
                    .When(x => !string.IsNullOrWhiteSpace(x.Bio))
                    .WithMessage("O campo 'Bio' deve ter no máximo 200 caracteres.");
            }
        }
    }
}
