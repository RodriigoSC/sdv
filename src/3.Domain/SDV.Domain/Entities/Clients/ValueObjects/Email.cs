using System;
using System.Text.RegularExpressions;

namespace SDV.Domain.Entities.Clients.ValueObjects;

public readonly record struct Email(string Address)
{
    public override string ToString() => Address;

    public static Email Create(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Email cannot be empty", nameof(address));

        if (!Regex.IsMatch(address, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            throw new ArgumentException("Invalid email format", nameof(address));

        return new Email(address.ToLowerInvariant());
    }
}