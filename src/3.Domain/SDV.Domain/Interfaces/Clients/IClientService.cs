using SDV.Domain.Entities.Clients;
using SDV.Domain.Entities.Clients.ValueObjects;
using SDV.Domain.Entities.Commons;

namespace SDV.Domain.Interfaces.Clients;

public interface IClientService
{
    #region Consultas
    Task<Result<IEnumerable<Client>>> GetAllClientsAsync();
    Task<Result<IEnumerable<Client>>> GetActiveClientsAsync();
    Task<Result<Client>> GetClientAsync(Guid id);
    Task<Result<Client>> FindByEmailAsync(string email);
    #endregion

    #region Criação
    Task<Result<Client>> CreateClientAsync(Client client);
    #endregion

    #region Atualizações
    Task<Result<Client>> UpdateClientAsync(Client client);
    Task<Result<Client>> UpdateClientEmailAsync(Guid clientId, Email newEmail);
    Task<Result<Client>> ChangeClientPasswordAsync(Guid clientId, string newPasswordHash);
    Task<Result<Client>> ResetClientPasswordAsync(Guid clientId, string tempPasswordHash);
    #endregion

    #region Ativação / Desativação
    Task<Result<Client>> ActivateClientAsync(Guid clientId);
    Task<Result<bool>> DeactivateClientAsync(Guid clientId);
    #endregion

    #region Verificação de Email
    Task<Result<Client>> GenerateEmailVerificationTokenAsync(Guid clientId);
    Task<Result<Client>> VerifyClientEmailAsync(Guid clientId, string token);
    #endregion
}
