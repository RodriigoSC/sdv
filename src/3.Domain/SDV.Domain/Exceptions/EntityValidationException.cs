using System;

namespace SDV.Domain.Exceptions;

public class EntityValidationException : DomainException
{
    public EntityValidationException(string model, string message, string? code = null, Exception? innerException = null)
        : base(model, message, code, innerException) { }
}
