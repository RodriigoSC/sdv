using System;

namespace SDV.Domain.Exceptions;

public abstract class DomainException : Exception
{
    public string Model { get; }
    public string? Code { get; }

    protected DomainException(string model, string message, string? code = null, Exception? innerException = null)
        : base(message, innerException)
    {
        Model = model;
        Code = code;
    }

}
