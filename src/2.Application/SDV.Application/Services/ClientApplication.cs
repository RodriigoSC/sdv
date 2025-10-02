using SDV.Application.Dtos.Clients;
using SDV.Application.Interfaces;
using SDV.Application.Mappers;
using SDV.Application.Results;
using SDV.Domain.Entities.Clients.ValueObjects;
using SDV.Domain.Interfaces.Clients;

namespace SDV.Application.Services;

public class ClientApplication : IClientApplication
{
    private readonly IClientService _clientService;

    public ClientApplication(IClientService clientService)
    {
        _clientService = clientService;
    }

    #region Consultas

    public async Task<OperationResult<IEnumerable<ClientDto>>> GetAllClients()
    {
        var result = await _clientService.GetAllClientsAsync();
        if (!result.IsSuccess)
            return OperationResult<IEnumerable<ClientDto>>.Failed(null, result.Error ?? "Clients not retrieved", 400);

        var dtos = result.Value?.Select(c => c.ToClientDto()) ?? Enumerable.Empty<ClientDto>();
        return OperationResult<IEnumerable<ClientDto>>.Succeeded(dtos, "Clients retrieved", 200);
    }

    public async Task<OperationResult<IEnumerable<ClientDto>>> GetActiveClients()
    {
        var result = await _clientService.GetActiveClientsAsync();
        if (!result.IsSuccess)
            return OperationResult<IEnumerable<ClientDto>>.Failed(null, result.Error ?? "Active clients not retrieved", 400);

        var dtos = result.Value?.Select(c => c.ToClientDto()) ?? Enumerable.Empty<ClientDto>();
        return OperationResult<IEnumerable<ClientDto>>.Succeeded(dtos, "Active clients retrieved", 200);
    }

    public async Task<OperationResult<ClientDto>> GetOneClient(string id)
    {
        if (!Guid.TryParse(id, out var guid))
            return OperationResult<ClientDto>.Failed(null, "Invalid ID format", 400);

        var result = await _clientService.GetClientAsync(guid);
        if (!result.IsSuccess)
            return OperationResult<ClientDto>.Failed(null, result.Error ?? "Client not found", 404);

        return OperationResult<ClientDto>.Succeeded(result.Value!.ToClientDto(), "Client retrieved", 200);
    }

    public async Task<OperationResult<ClientDto>> FindClientByEmail(string email)
    {
        var result = await _clientService.FindByEmailAsync(email);
        if (!result.IsSuccess)
            return OperationResult<ClientDto>.Failed(null, result.Error ?? "Client not found", 404);

        return OperationResult<ClientDto>.Succeeded(result.Value!.ToClientDto(), "Client retrieved", 200);
    }

    #endregion

    #region Criação

    public async Task<OperationResult<ClientDto>> CreateClient(ClientDto dto)
    {
        try
        {
            var client = dto.ToClient();
            var result = await _clientService.CreateClientAsync(client);

            if (!result.IsSuccess)
                return OperationResult<ClientDto>.Failed(null, result.Error ?? "Client not created", 406);

            return OperationResult<ClientDto>.Succeeded(client.ToClientDto(), "Client created", 201);
        }
        catch (Exception ex)
        {
            return OperationResult<ClientDto>.Failed(null, ex.Message, 400);
        }
    }

    #endregion

    #region Atualizações

    public async Task<OperationResult<ClientDto>> UpdateClient(string id, ClientDto dto)
    {
        if (!Guid.TryParse(id, out var guid))
            return OperationResult<ClientDto>.Failed(null, "Invalid ID format", 400);

        var existing = await _clientService.GetClientAsync(guid);
        if (!existing.IsSuccess)
            return OperationResult<ClientDto>.Failed(null, existing.Error!, 404);

        var client = existing.Value!;

        client.ChangeName(dto.Name);
        client.UpdateProfile(
            dto.Profile?.JobTitle,
            dto.Profile?.Bio,
            dto.Profile?.ProfileImageUrl,
            dto.Profile?.PhoneNumber
        );

        var result = await _clientService.UpdateClientAsync(client);
        if (!result.IsSuccess)
            return OperationResult<ClientDto>.Failed(null, result.Error ?? "Client not updated", 400);

        return OperationResult<ClientDto>.Succeeded(client.ToClientDto(), "Client updated", 200);
    }

    public async Task<OperationResult<ClientDto>> UpdateClientEmail(string id, string newEmail)
    {
        if (!Guid.TryParse(id, out var guid))
            return OperationResult<ClientDto>.Failed(null, "Invalid ID format", 400);

        var result = await _clientService.UpdateClientEmailAsync(guid, new Email(newEmail));
        if (!result.IsSuccess)
            return OperationResult<ClientDto>.Failed(null, result.Error ?? "Email not updated", 400);

        return OperationResult<ClientDto>.Succeeded(result.Value!.ToClientDto(), "Email updated", 200);
    }

    public async Task<OperationResult<ClientDto>> ChangeClientPassword(string id, string newPassword)
    {
        if (!Guid.TryParse(id, out var guid))
            return OperationResult<ClientDto>.Failed(null, "Invalid ID format", 400);

        var result = await _clientService.ChangeClientPasswordAsync(guid, newPassword);
        if (!result.IsSuccess)
            return OperationResult<ClientDto>.Failed(null, result.Error ?? "Password not changed", 400);

        return OperationResult<ClientDto>.Succeeded(result.Value!.ToClientDto(), "Password changed", 200);
    }

    public async Task<OperationResult<ClientDto>> ResetClientPassword(string id, string tempPassword)
    {
        if (!Guid.TryParse(id, out var guid))
            return OperationResult<ClientDto>.Failed(null, "Invalid ID format", 400);

        var result = await _clientService.ResetClientPasswordAsync(guid, tempPassword);
        if (!result.IsSuccess)
            return OperationResult<ClientDto>.Failed(null, result.Error ?? "Password not reset", 400);

        return OperationResult<ClientDto>.Succeeded(result.Value!.ToClientDto(), "Password reset", 200);
    }

    #endregion

    #region Ativação / Desativação

    public async Task<OperationResult<ClientDto>> ActivateClient(string id)
    {
        if (!Guid.TryParse(id, out var guid))
            return OperationResult<ClientDto>.Failed(null, "Invalid ID format", 400);

        var result = await _clientService.ActivateClientAsync(guid);
        if (!result.IsSuccess)
            return OperationResult<ClientDto>.Failed(null, result.Error ?? "Client not activated", 400);

        return OperationResult<ClientDto>.Succeeded(result.Value!.ToClientDto(), "Client activated", 200);
    }

    public async Task<OperationResult<bool>> DeactivateClient(string id)
    {
        if (!Guid.TryParse(id, out var guid))
            return OperationResult<bool>.Failed(false, "Invalid ID format", 400);

        var result = await _clientService.DeactivateClientAsync(guid);
        if (!result.IsSuccess)
            return OperationResult<bool>.Failed(false, result.Error ?? "Client not deactivated", 400);

        return OperationResult<bool>.Succeeded(true, "Client deactivated", 200);
    }

    #endregion

    #region Verificação de Email

    public async Task<OperationResult<ClientDto>> GenerateEmailVerificationToken(string id)
    {
        if (!Guid.TryParse(id, out var guid))
            return OperationResult<ClientDto>.Failed(null, "Invalid ID format", 400);

        var result = await _clientService.GenerateEmailVerificationTokenAsync(guid);
        if (!result.IsSuccess)
            return OperationResult<ClientDto>.Failed(null, result.Error ?? "Token not generated", 400);

        return OperationResult<ClientDto>.Succeeded(result.Value!.ToClientDto(), "Token generated", 200);
    }

    public async Task<OperationResult<ClientDto>> VerifyClientEmail(string id, string token)
    {
        if (!Guid.TryParse(id, out var guid))
            return OperationResult<ClientDto>.Failed(null, "Invalid ID format", 400);

        var result = await _clientService.VerifyClientEmailAsync(guid, token);
        if (!result.IsSuccess)
            return OperationResult<ClientDto>.Failed(null, result.Error ?? "Email not verified", 400);

        return OperationResult<ClientDto>.Succeeded(result.Value!.ToClientDto(), "Email verified", 200);
    }

    #endregion
}
