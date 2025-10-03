using System.Net.Mail;

namespace SDV.Domain.Entities.Clients.ValueObjects;

public readonly record struct Email(string Address)
{
    public override string ToString() => Address;

    public static Email Create(string address)
{
    if (string.IsNullOrWhiteSpace(address))
        throw new ArgumentException("Email cannot be empty", nameof(address));

    if (!MailAddress.TryCreate(address, out _))
        throw new ArgumentException("Invalid email format", nameof(address));

    return new Email(address.ToLowerInvariant());
}
}