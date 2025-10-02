using SDV.Application.Dtos.Clients;
using SDV.Application.Results;

namespace SDV.Application.Interfaces;

public interface IClientApplication
{
    #region Consultas
    Task<OperationResult<IEnumerable<ClientDto>>> GetAllClients();
    Task<OperationResult<IEnumerable<ClientDto>>> GetActiveClients();
    Task<OperationResult<ClientDto>> GetOneClient(string Id);
    Task<OperationResult<ClientDto>> FindClientByEmail(string email);
    #endregion

    #region Criação
    Task<OperationResult<ClientDto>> CreateClient(ClientDto entity);
    #endregion

    #region Atualizações
    Task<OperationResult<ClientDto>> UpdateClient(string Id, ClientDto entity);
    Task<OperationResult<ClientDto>> UpdateClientEmail(string Id, string newEmail);
    Task<OperationResult<ClientDto>> ChangeClientPassword(string Id, string newPassword);
    Task<OperationResult<ClientDto>> ResetClientPassword(string Id, string tempPassword);
    #endregion

    #region Ativação / Desativação
    Task<OperationResult<ClientDto>> ActivateClient(string Id);
    Task<OperationResult<bool>> DeactivateClient(string Id);
    #endregion

    #region Verificação de Email
    Task<OperationResult<ClientDto>> GenerateEmailVerificationToken(string Id);
    Task<OperationResult<ClientDto>> VerifyClientEmail(string Id, string token);
    #endregion
}
