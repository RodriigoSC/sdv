using SDV.Application.Dtos.Clients;
using SDV.Domain.Entities.Clients;
using SDV.Domain.Entities.Clients.ValueObjects;


namespace SDV.Application.Mappers;

public static class ClientMapper
{
    // Entidade -> DTO
    public static ClientDto ToClientDto(this Client client)
    {
        if (client == null) return null!;

        return new ClientDto
        {
            Id = client.Id.ToString(),
            Name = client.Name,
            Email = client.Email.Address,
            PasswordHash = client.PasswordHash,
            IsActive = client.IsActive,
            EmailVerified = client.EmailVerified,
            Profile = new UserProfileDto
            {
                JobTitle = client.JobTitle,
                Bio = client.Bio,
                ProfileImageUrl = client.ProfileImageUrl,
                PhoneNumber = client.PhoneNumber
            }
        };
    }

    public static IEnumerable<ClientDto> ToClientDtoList(this IEnumerable<Client> clients)
    {
        if (clients == null) yield break;

        foreach (var client in clients)
            yield return client.ToClientDto();
    }

    // DTO -> Entidade (para criação)
    public static Client ToClient(this ClientDto dto)
    {
        if (dto == null) return null!;

        var email = new Email(dto.Email ?? throw new ArgumentNullException(nameof(dto.Email)));
        var passwordHash = dto.PasswordHash ?? throw new ArgumentNullException(nameof(dto.PasswordHash));

        return new Client(
            name: dto.Name ?? throw new ArgumentNullException(nameof(dto.Name)),
            email: email,
            passwordHash: passwordHash
        );
    }

    public static IEnumerable<Client> ToClientList(this IEnumerable<ClientDto> dtos)
    {
        if (dtos == null) yield break;

        foreach (var dto in dtos)
            yield return dto.ToClient();
    }

    // Atualiza entidade existente a partir de DTO
    public static void UpdateFromDto(this Client client, ClientDto dto)
    {
        if (client == null || dto == null) return;

        // Atualizando Name
        if (!string.IsNullOrWhiteSpace(dto.Name) && dto.Name != client.Name)
        {
            client.ChangeName(dto.Name);
        }

        // Atualizando Profile
        client.UpdateProfile(
            jobTitle: dto.Profile?.JobTitle,
            bio: dto.Profile?.Bio,
            profileImageUrl: dto.Profile?.ProfileImageUrl,
            phoneNumber: string.IsNullOrWhiteSpace(dto.Profile?.PhoneNumber)
                ? null
                : dto.Profile.PhoneNumber
        );

        // Atualizando Password se fornecido
        if (!string.IsNullOrWhiteSpace(dto.PasswordHash) && dto.PasswordHash != client.PasswordHash)
        {
            client.ChangePassword(dto.PasswordHash);
        }
    }
}
