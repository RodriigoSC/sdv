using System;

namespace SDV.Domain.Exceptions;

public class EmptyEntityException : DomainException
{
    public EmptyEntityException(string model, string message, string? code = null, Exception? innerException = null)
        : base(model, message, code, innerException) { }
}
