using SDV.Domain.Entities.Clients;
using SDV.Domain.Entities.Clients.ValueObjects;
using SDV.Domain.Entities.Commons;
using SDV.Domain.Enums;
using SDV.Domain.Enums.Clients;
using SDV.Domain.Exceptions;
using SDV.Domain.Interfaces.Clients;
using SDV.Domain.Specification;

namespace SDV.Infra.Data.Service.Clients;

public class ClientService : IClientService
{
    private readonly IClientRepository _repository;

    public ClientService(IClientRepository repository)
    {
        _repository = repository;
    }

    #region Consultas

    public async Task<Result<IEnumerable<Client>>> GetAllClientsAsync()
    {
        var clients = await _repository.GetAllAsync();
        return Result<IEnumerable<Client>>.Success(clients);
    }

    public async Task<Result<IEnumerable<Client>>> GetActiveClientsAsync()
    {
        var clients = await _repository.GetAllAsync();
        var activeClients = clients.Where(c => c.IsActive);
        return Result<IEnumerable<Client>>.Success(activeClients);
    }

    public async Task<Result<Client>> GetClientAsync(Guid id)
    {
        var client = await _repository.GetByIdAsync(id);
        return client is null
            ? Result<Client>.Failure("Cliente não encontrado")
            : Result<Client>.Success(client);
    }

    public async Task<Result<Client>> FindByEmailAsync(string email)
    {
        var clients = await _repository.GetAllAsync();
        var client = clients.FirstOrDefault(c => c.Email.Address.Equals(email, StringComparison.OrdinalIgnoreCase));
        return client is null
            ? Result<Client>.Failure("Cliente não encontrado")
            : Result<Client>.Success(client);
    }

    #endregion

    #region Criação
    public async Task<Result<Client>> CreateClientAsync(Client client)
    {
        try
        {
            new ClientValidationSpecification().IsValid(client);
        }
        catch (EntityValidationException ex)
        {
            return Result<Client>.Failure(ex.Message);
        }

        if (!client.IsActive)
            return Result<Client>.Failure("Cliente inativo não pode ser criado.");

        var emailCheck = await FindByEmailAsync(client.Email.Address);
        if (emailCheck.IsSuccess)
            return Result<Client>.Failure("Já existe um cliente cadastrado com este email.");

        await _repository.AddAsync(client);

        return Result<Client>.Success(client);
    }
    #endregion

    #region Atualizações
    public async Task<Result<Client>> UpdateClientAsync(Client client)
    {
        var existingClient = await _repository.GetByIdAsync(client.Id);
        if (existingClient == null)
            return Result<Client>.Failure("Cliente não encontrado");

        existingClient.ChangeName(client.Name);
        existingClient.UpdateProfile(client.JobTitle, client.Bio, client.ProfileImageUrl, client.PhoneNumber);
        existingClient.ChangePassword(client.PasswordHash);

        try
        {
            new ClientValidationSpecification().IsValid(existingClient);
        }
        catch (EntityValidationException ex)
        {
            return Result<Client>.Failure(ex.Message);
        }

        if (!existingClient.IsActive)
            return Result<Client>.Failure("Cliente inativo não pode ser atualizado.");

        await _repository.UpdateAsync(existingClient);

        return Result<Client>.Success(existingClient);
    }

    public async Task<Result<Client>> UpdateClientEmailAsync(Guid clientId, Email newEmail)
    {
        var client = await _repository.GetByIdAsync(clientId);
        if (client is null)
            return Result<Client>.Failure("Cliente não encontrado");

        var emailCheck = await FindByEmailAsync(newEmail.Address);

        if (emailCheck.IsSuccess && emailCheck.Value is not null && emailCheck.Value.Id != client.Id)
            return Result<Client>.Failure("Já existe um cliente cadastrado com este email.");

        client.UpdateEmail(newEmail);
        await _repository.UpdateAsync(client);

        return Result<Client>.Success(client);
    }

    public async Task<Result<Client>> ChangeClientPasswordAsync(Guid clientId, string newPasswordHash)
    {
        var client = await _repository.GetByIdAsync(clientId);
        if (client is null)
            return Result<Client>.Failure("Cliente não encontrado");

        client.ChangePassword(newPasswordHash);
        await _repository.UpdateAsync(client);

        return Result<Client>.Success(client);
    }

    public async Task<Result<Client>> ResetClientPasswordAsync(Guid clientId, string tempPasswordHash)
    {
        var client = await _repository.GetByIdAsync(clientId);
        if (client is null)
            return Result<Client>.Failure("Cliente não encontrado");

        client.ResetPassword(tempPasswordHash);
        await _repository.UpdateAsync(client);

        return Result<Client>.Success(client);
    }
    #endregion

    #region Ativação / Desativação
    public async Task<Result<Client>> ActivateClientAsync(Guid clientId)
    {
        var client = await _repository.GetByIdAsync(clientId);
        if (client is null)
            return Result<Client>.Failure("Cliente não encontrado");

        client.Activate();
        await _repository.UpdateAsync(client);

        return Result<Client>.Success(client);
    }

    public async Task<Result<bool>> DeactivateClientAsync(Guid clientId)
    {
        var client = await _repository.GetByIdAsync(clientId);
        if (client is null)
            return Result<bool>.Failure("Cliente não encontrado");

        client.Deactivate();
        await _repository.UpdateAsync(client);

        return Result<bool>.Success(true);
    }
    #endregion

    #region Verificação de Email
    public async Task<Result<Client>> GenerateEmailVerificationTokenAsync(Guid clientId)
    {
        var client = await _repository.GetByIdAsync(clientId);
        if (client is null)
            return Result<Client>.Failure("Cliente não encontrado");

        client.GenerateEmailVerificationToken();
        await _repository.UpdateAsync(client);

        return Result<Client>.Success(client);
    }

    public async Task<Result<Client>> VerifyClientEmailAsync(Guid clientId, string token)
    {
        var client = await _repository.GetByIdAsync(clientId);
        if (client is null)
            return Result<Client>.Failure("Cliente não encontrado");

        var result = client.VerifyEmail(token);
        if (result != EmailVerificationResult.Success)
            return Result<Client>.Failure(result.ToString());

        await _repository.UpdateAsync(client);
        return Result<Client>.Success(client);
    }        
    #endregion

}
